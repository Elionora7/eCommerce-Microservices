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

public class RabbitMQProductNameConsumer : IDisposable, IRabbitMQProductNameConsumer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProductNameConsumer> _logger;
    private readonly IDistributedCache _cache;
    private const string ProductCacheKeyPrefix = "product:";
    private bool _disposed;

    // Make fields nullable since they're initialized in a separate method
    private IModel? _channel;
    private IConnection? _connection;

    public RabbitMQProductNameConsumer(IConfiguration configuration,
                                     ILogger<RabbitMQProductNameConsumer> logger,
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
            // Get all relevant config values for debugging
            string hostName = _configuration["RabbitMQ_HostName"] ?? "rabbitmq";
            string userName = _configuration["RabbitMQ_UserName"] ?? throw new ArgumentNullException("RabbitMQ_UserName");
            string password = _configuration["RabbitMQ_Password"] ?? throw new ArgumentNullException("RabbitMQ_Password");
            int port = _configuration.GetValue("RabbitMQ_Port", 5672);

            _logger.LogInformation("RabbitMQ Connection Details - Host: {Host}, Port: {Port}, User: {User}",
                hostName, port, userName);

            var connectionFactory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30) // Add timeout
            };

            var policy = Policy.Handle<BrokerUnreachableException>()
                .WaitAndRetry(6, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, "RabbitMQ connection attempt {RetryCount}/6 failed. Waiting {WaitTime}s",
                            retryCount, timeSpan.TotalSeconds);
                    });

            policy.Execute(() =>
            {
                _logger.LogInformation("Attempting RabbitMQ connection...");
                _connection = connectionFactory.CreateConnection("OrdersService-Connection");
                _channel = _connection.CreateModel();
                _logger.LogInformation("Successfully connected to RabbitMQ");
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize RabbitMQ connection after multiple attempts");
            throw;
        }
    }

    public void Consume() // Implementing the interface method
    {
        if (_channel == null || _connection == null)
        {
            throw new InvalidOperationException("RabbitMQ connection is not initialized");
        }

        try
        {
            string queueName = "orders.product.update.name.queue";
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
                { "event", "product.update" },
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
                    _logger.LogDebug("Received product update message: {Message}", message);

                    var productDTO = JsonSerializer.Deserialize<ProductDTO>(message)
                        ?? throw new InvalidOperationException("Deserialized product is null");

                    await HandleProductUpdate(productDTO);

                    // Manual acknowledgment
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    // Reject message and don't requeue
                    _channel.BasicReject(ea.DeliveryTag, false);
                }
            };

            _channel.BasicConsume(
                queue: queueName,
                autoAck: false, // Manual acknowledgment
                consumer: consumer);

            _logger.LogInformation("Started consuming messages from {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to start consuming messages");
            throw;
        }
    }

    private async Task HandleProductUpdate(ProductDTO productDTO)
    {
        try
        {
            string cacheKey = $"{ProductCacheKeyPrefix}{productDTO.ProductID}";
            string productJson = JsonSerializer.Serialize(productDTO);

            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(400));

            await _cache.SetStringAsync(cacheKey, productJson, options);

            _logger.LogInformation("Updated cache for product {ProductId}", productDTO.ProductID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product cache for {ProductId}", productDTO.ProductID);
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
                _logger.LogInformation("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
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