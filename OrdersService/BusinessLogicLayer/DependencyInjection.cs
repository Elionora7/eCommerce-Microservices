using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly.Bulkhead;
using Polly.Timeout;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
        services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);
        services.AddScoped<IOrdersService, OrdersService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
        });

        services.AddTransient<IRabbitMQProductNameConsumer, RabbitMQProductNameConsumer>();
        services.AddHostedService<RabbitMQProductNameHostedService>();
        services.AddTransient<IRabbitMQProductDeleteConsumer, RabbitMQProductDeleteConsumer>();
        services.AddHostedService<RabbitMQProductDeleteHostedService>();

        // Configure HTTP Clients HERE
        ConfigureHttpClients(services, configuration);

        return services;
    }

    private static void ConfigureHttpClients(IServiceCollection services, IConfiguration configuration)
    {
        // User Microservice Client
        services.AddHttpClient<IUserMicroserviceClient, UserMicroserviceClient>((provider, client) =>
        {
            var userServiceName = configuration["UserMicroServiceName"] ?? "apigateway";
            var userServicePort = configuration["UserMicroServicePort"] ?? "8080";

            client.BaseAddress = new Uri($"http://{userServiceName}:{userServicePort}/gateway/users/");
        })
        .AddPolicyHandler((services, _) =>
            services.GetRequiredService<IUserMicroservicePolicies>().GetCombinedPolicy());

        // Product Microservice Client
        services.AddHttpClient<IProductMicroserviceClient, ProductMicroserviceClient>((provider, client) =>
        {
            var productServiceName = configuration["ProductMicroServiceName"] ?? "apigateway";
            var productServicePort = configuration["ProductMicroServicePort"] ?? "8080";

            client.BaseAddress = new Uri($"http://{productServiceName}:{productServicePort}/gateway/products/");
        })
        .AddPolicyHandler((services, _) =>
            services.GetRequiredService<IProductMicroservicePolicies>().GetCombinedPolicy())
        .AddPolicyHandler((services, _) =>
            services.GetRequiredService<IProductMicroservicePolicies>().GetBulkheadIsolationPolicy());
    }
}