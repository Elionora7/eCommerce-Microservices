using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients
{
    public interface IProductMicroserviceClient
    {
        Task<ProductDTO?> GetProductByProductId(Guid productId);
    }
}
