

using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using System.Net.Http.Json;
using Polly.Bulkhead;
using Microsoft.Extensions.Logging;
using DnsClient.Internal;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductMicroserviceClient
{
    public readonly HttpClient _httpClient;
    public readonly ILogger<ProductMicroserviceClient> _logger;

    public ProductMicroserviceClient(HttpClient httpClient,ILogger<ProductMicroserviceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException("Bad Request", null, System.Net.HttpStatusCode.BadRequest);
                }
                else
                {
                    throw new HttpRequestException("Http request failed with status code {response.statuscode}");
                }
            }
            ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();

            if (product == null)
            {
                throw new ArgumentException("Invalid product Id!");
            }

            return product;
        }
        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex,"Bulkhead isolation is blocking the request since the request queue is full!");

            return new ProductDTO(
              ProductID: Guid.NewGuid(),
              ProductName: "Temporarily Unavailable(Bulkhead)!",
              Category: "Temporarily Unavailable(Bulkhead)!",
              UnitPrice: 0,
              QuantityInStock: 0);
        }
    }
}
