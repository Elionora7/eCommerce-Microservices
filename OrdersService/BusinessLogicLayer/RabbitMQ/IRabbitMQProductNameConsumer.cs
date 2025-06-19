namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ
{
    public interface IRabbitMQProductNameConsumer
    {
        void Consume();
        void Dispose();

    }
}