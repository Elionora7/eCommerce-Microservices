namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderItemUpdateRequest(Guid ProductID, decimal UnitPrice, int Quantity, decimal TotalPrice, string? ImgUrl)
{
    public OrderItemUpdateRequest() : this(default, default, default,default,default)
    {
    }
}