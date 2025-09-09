using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IModel _channel;
        private readonly IConnection _connection;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _configuration = configuration;

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
                port = int.Parse(portString);
            }

            var factory = new ConnectionFactory()
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                Port = port
            };

            try
            {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange on startup (idempotent)
            string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);
        }

        public void Publish<T>(Dictionary<string, object> headers, T message)
        {
            string messageJson = JsonSerializer.Serialize(message);
            byte[] messageBody = Encoding.UTF8.GetBytes(messageJson);

            string exchangeName = _configuration["RabbitMQ_Products_Exchange"]!;

            // Declare exchange again for idempotency/safety
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Headers, durable: true);

            var basicProperties = _channel.CreateBasicProperties();
            basicProperties.Headers = headers;

            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: string.Empty,
                basicProperties: basicProperties,
                body: messageBody);
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
