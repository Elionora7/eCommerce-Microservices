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

            // Support both naming : Docker Compose and .env 
            string hostName = _configuration["RabbitMQ_HostName"]
                ?? _configuration["RABBITMQ_HOSTNAME"]
                ?? throw new InvalidOperationException("RabbitMQ hostname is required. Set RabbitMQ_HostName or RABBITMQ_HOSTNAME");

            string userName = _configuration["RabbitMQ_UserName"]
                ?? _configuration["RABBITMQ_USERNAME"]
                ?? throw new InvalidOperationException("RabbitMQ username is required. Set RabbitMQ_UserName or RABBITMQ_USERNAME");

            string password = _configuration["RabbitMQ_Password"]
                ?? _configuration["RABBITMQ_PASSWORD"]
                ?? throw new InvalidOperationException("RabbitMQ password is required. Set RabbitMQ_Password or RABBITMQ_PASSWORD");

            string portString = _configuration["RabbitMQ_Port"]
                ?? _configuration["RABBITMQ_PORT"]
                ?? "5672"; // Default RabbitMQ port

            string exchangeName = _configuration["RabbitMQ_Products_Exchange"]
                ?? _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]
                ?? "products_exchange"; // Default exchange name


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
                Port = port,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(
                    exchange: exchangeName,
                    type: ExchangeType.Headers,
                    durable: true,
                    autoDelete: false);

                Console.WriteLine($"RabbitMQ connection established to {hostName}:{port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to RabbitMQ: {ex.Message}");
                throw;
            }
        }

        public void Publish<T>(Dictionary<string, object> headers, T message)
        {
            if (_channel?.IsOpen != true)
            {
                throw new InvalidOperationException("RabbitMQ channel is not open");
            }

            
            string exchangeName = _configuration["RabbitMQ_Products_Exchange"]
                ?? _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]
                ?? "products_exchange";

            try
            {
                string messageJson = JsonSerializer.Serialize(message);
                byte[] messageBody = Encoding.UTF8.GetBytes(messageJson);

                var basicProperties = _channel.CreateBasicProperties();
                basicProperties.Headers = headers;
                basicProperties.Persistent = true;

                _channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: string.Empty,
                    mandatory: true,
                    basicProperties: basicProperties,
                    body: messageBody);

                Console.WriteLine($"Message published to exchange: {exchangeName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to publish message: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                Console.WriteLine("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing RabbitMQ resources: {ex.Message}");
            }
        }
    }
}