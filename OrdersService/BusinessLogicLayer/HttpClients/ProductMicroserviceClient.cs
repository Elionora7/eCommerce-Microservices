

using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using System.Net.Http.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductMicroserviceClient
{
    public readonly HttpClient _httpClient;

    public ProductMicroserviceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDTO?> GetProductByProductId(Guid productId)
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
}
