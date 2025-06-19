using Microsoft.Extensions.Hosting;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameHostedService : IHostedService
{
    private readonly IRabbitMQProductNameConsumer _productNameConsumer;

    public RabbitMQProductNameHostedService(IRabbitMQProductNameConsumer consumer)
    {
        _productNameConsumer = consumer;
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        _productNameConsumer.Consume();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _productNameConsumer.Dispose();

        return Task.CompletedTask;
    }
}