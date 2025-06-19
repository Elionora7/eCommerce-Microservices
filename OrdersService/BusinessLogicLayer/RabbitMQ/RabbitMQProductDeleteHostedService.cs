using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeleteHostedService : IHostedService
{
    private readonly IRabbitMQProductDeleteConsumer _productDeleteConsumer;

    public RabbitMQProductDeleteHostedService(IRabbitMQProductDeleteConsumer consumer)
    {
        _productDeleteConsumer = consumer;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _productDeleteConsumer.Consume();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productDeleteConsumer.Dispose();

        return Task.CompletedTask;
    }
}