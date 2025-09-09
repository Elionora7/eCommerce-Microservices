using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeleteConsumer : IRabbitMQProductDeleteConsumer, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProductDeleteConsumer> _logger;
    private readonly IDistributedCache _cache;
    private const string ProductCacheKeyPrefix = "product:";
    private bool _disposed;

    // Make fields nullable since they're initialized in a separate method
    private IModel? _channel;
    private IConnection? _connection;

    public RabbitMQProductDeleteConsumer(IConfiguration configuration,
                                      ILogger<RabbitMQProductDeleteConsumer> logger,
                                      IDistributedCache cache)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        InitializeRabbitMQConnection();
    }

    private void InitializeRabbitMQConnection()
    {
        try
        {
            // Get configuration values with validation
            string hostName = _configuration["RabbitMQ_HostName"] ?? "rabbitmq";
            string userName = _configuration["RabbitMQ_UserName"] ?? throw new ArgumentNullException("RabbitMQ_UserName");
            string password = _configuration["RabbitMQ_Password"] ?? throw new ArgumentNullException("RabbitMQ_Password");
            int port = _configuration.GetValue("RABBITMQ_PORT", 5672);

            _logger.LogInformation("Initializing RabbitMQ connection to {Host}:{Port}", hostName, port);

            var connectionFactory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            // Retry policy with exponential backoff
            var policy = Policy.Handle<BrokerUnreachableException>()
                .WaitAndRetry(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "RabbitMQ connection attempt {RetryCount}/5 failed", retryCount);
                    });

            policy.Execute(() =>
            {
                _connection = connectionFactory.CreateConnection("OrdersService-Delete-Connection");
                _channel = _connection.CreateModel();
                _logger.LogInformation("Successfully connected to RabbitMQ for product deletion");
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize RabbitMQ connection for product deletion");
            throw;
        }
    }

    public void Consume()
    {
        if (_channel == null || _connection == null)
        {
            throw new InvalidOperationException("RabbitMQ connection is not initialized");
        }

        try
        {
            string queueName = "orders.product.delete.queue";
            string exchangeName = _configuration["RabbitMQ_Products_Exchange"] ?? "products.exchange";

            _channel.ExchangeDeclare(
                exchange: exchangeName,
                type: ExchangeType.Headers,
                durable: true,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var headers = new Dictionary<string, object>
            {
                { "x-match", "all" },
                { "event", "product.delete" },
                { "RowCount", 1 }
            };

            _channel.QueueBind(
                queue: queueName,
                exchange: exchangeName,
                routingKey: string.Empty,
                arguments: headers);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogDebug("Received product delete message: {Message}", message);

                    var deleteMessage = JsonSerializer.Deserialize<ProductDeleteMessage>(message)
                        ?? throw new InvalidOperationException("Deserialized delete message is null");

                    await HandleProductDelete(deleteMessage.ProductID);

                    // Manual acknowledgment
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing delete message");
                    // Reject message and don't requeue
                    _channel.BasicReject(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false, // Manual acknowledgment
                consumer: consumer);

            _logger.LogInformation("Started consuming product delete messages from {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to start consuming delete messages");
            throw;
        }
    }

    private async Task HandleProductDelete(Guid productId)
    {
        try
        {
            string cacheKey = $"{ProductCacheKeyPrefix}{productId}";
            await _cache.RemoveAsync(cacheKey);
            _logger.LogInformation("Removed product {ProductId} from cache", productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove product {ProductId} from cache", productId);
            throw;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                _channel?.Dispose();
                _connection?.Dispose();
                _logger.LogInformation("RabbitMQ product delete connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ product delete connection");
            }
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}