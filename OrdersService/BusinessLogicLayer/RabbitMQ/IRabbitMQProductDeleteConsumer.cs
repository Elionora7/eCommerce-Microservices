
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductDeleteConsumer
{
    void Consume();
    void Dispose();
}
