namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderItemAddRequest(Guid ProductID, decimal UnitPrice, int Quantity,decimal TotalPrice,string? ImgUrl)
{
    public OrderItemAddRequest() : this(default, default, default,default,default)
    {
    }
}