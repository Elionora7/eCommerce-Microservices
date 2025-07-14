using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameConsumer : IDisposable, IRabbitMQProductNameConsumer
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQProductNameConsumer> _logger;
    private readonly IDistributedCache _cache;

    public RabbitMQProductNameConsumer(IConfiguration configuration, 
                                       ILogger<RabbitMQProductNameConsumer> logger,
                                       IDistributedCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;

        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        string portString = _configuration["RabbitMQ_Port"]!;

        Console.WriteLine($"RabbitMQ_HostName: {hostName}");
        Console.WriteLine($"RabbitMQ_UserName: {userName}");
        Console.WriteLine($"RabbitMQ_Password: {password}");
        Console.WriteLine($"RabbitMQ_Port: {portString}");

        int port;
        if (portString.StartsWith("tcp://"))
        {
            var uri = new Uri(portString);
            port = uri.Port;
        }
        else
        {
            port = Convert.ToInt32(portString);
        }

        var connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = port
        };

        _connection = connectionFactory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Consume()
    {
        string queueName = "orders.product.update.name.queue";

        var headers = new Dictionary<string, object>()
        {
            { "x-match", "all" },
            { "event", "product.update" },
            { "RowCount", 1 }
        };

        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            try
            {
                var body = args.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    ProductDTO? productDTO = JsonSerializer.Deserialize<ProductDTO>(message);

                    if (productDTO != null)
                    {
                        await HandleProductUpdate(productDTO);
                    }
                    else
                    {
                        _logger.LogWarning("Received null ProductDTO after deserialization.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing product update message.");
            }
        };

        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }

    private async Task HandleProductUpdate(ProductDTO productDTO)
    {
        _logger.LogInformation($"Product name updated: {productDTO.ProductID}, Product name: {productDTO.ProductName}");

        string productJson = JsonSerializer.Serialize(productDTO);

        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(400));

        string cacheKey = $"product:{productDTO.ProductID}";

        await _cache.SetStringAsync(cacheKey, productJson, options);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
