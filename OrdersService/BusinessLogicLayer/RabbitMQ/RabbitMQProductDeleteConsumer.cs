using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeleteConsumer : IRabbitMQProductDeleteConsumer, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQProductDeleteConsumer> _logger;
    private readonly IDistributedCache _cache;

    public RabbitMQProductDeleteConsumer(IConfiguration configuration, 
                      ILogger<RabbitMQProductDeleteConsumer> logger,
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

        ConnectionFactory connectionFactory = new ConnectionFactory()
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
        string queueName = "orders.product.delete.queue";

        // Define headers
        var headers = new Dictionary<string, object>()
         {
            { "x-match", "all" },
            { "event", "product.delete" },
            { "RowCount", 1 }
         };

        // Create exchange
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        // Create queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Bind queue to exchange with headers
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            try
            {
                byte[] body = args.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);

                if (!string.IsNullOrEmpty(message))
                {
                    ProductDeleteMessage? productdeleteMessage = JsonSerializer.Deserialize<ProductDeleteMessage>(message);
                    if (productdeleteMessage != null)
                    {
                        await HandleProductDelete(productdeleteMessage.ProductID);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing ProductDeleteMessage");
            }
        };

        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }

    private async Task HandleProductDelete(Guid productID)
    {
        _logger.LogInformation($"Product deleted: {productID}");
        string cacheKey = $"product:{productID}";

        await _cache.RemoveAsync(cacheKey);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
