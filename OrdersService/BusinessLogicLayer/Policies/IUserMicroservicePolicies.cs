
using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUserMicroservicePolicies
{
    IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();

}

