using AutoMapper;
using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.ProductsService.BusinessLogicLayer
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
            // First create the configuration to validate it
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(ProductAddRequestToProductMappingProfile).Assembly);
            });

            try
            {
                config.AssertConfigurationIsValid();
                Console.WriteLine("AutoMapper configuration is valid!");
            }
            catch (AutoMapperConfigurationException ex)
            {
                Console.WriteLine("AutoMapper configuration errors:");
                foreach (var error in ex.Errors)
                {
                    Console.WriteLine($"- {error.TypeMap.SourceType.Name} -> {error.TypeMap.DestinationType.Name}");
                    Console.WriteLine($"  Property: {error.UnmappedPropertyNames.FirstOrDefault()}");
                }
                // Don't continue if configuration is invalid
                throw;
            }

            services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);
            services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();
            services.AddScoped<IProductsService, eCommerce.BusinessLogicLayer.Services.ProductsService>();
            services.AddTransient<IRabbitMQPublisher, RabbitMQPublisher>();

            return services;
        }
    }
}