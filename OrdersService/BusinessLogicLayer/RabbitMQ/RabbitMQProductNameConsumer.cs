using DnsClient.Internal;
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
        string port = _configuration["RabbitMQ_Port"]!;

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };
        _connection = connectionFactory.CreateConnection();

        _channel = _connection.CreateModel();
    }


    public void Consume()
    {
        //string routingKey = "product.update.name";
        string queueName = "orders.product.update.name.queue";

        var headers = new Dictionary<string, object>()
      {
        { "x-match", "all" },
        { "event", "product.update" },
        { "RowCount", 1 }
      };


        //Create exchange
        string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

        //Create message queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); //x-message-ttl | x-max-length | x-expired 

        //Bind the message to exchange
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty, arguments: headers);

        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                ProductDTO? productDTO = JsonSerializer.Deserialize<ProductDTO>(message);

                if (productDTO != null)
                {
                    await HandleProductUpdate(productDTO);
                }
            }
        };

        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);
    }

    private async Task HandleProductUpdate(ProductDTO productDTO)
    {
        _logger.LogInformation($"Product name updated: {productDTO.ProductID}, Product name: {productDTO.ProductName}");

        string productJson = JsonSerializer.Serialize(productDTO);

        DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
          .SetAbsoluteExpiration(TimeSpan.FromSeconds(400));

        string cacheKey = $"product:{productDTO.ProductID}";

        await _cache.SetStringAsync(cacheKey, productJson, options);
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}