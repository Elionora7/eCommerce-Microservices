using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients
{
    public interface IUserMicroserviceClient
    {
        Task<UserDTO?> GetUserByUserId(Guid userId);
    }
}
