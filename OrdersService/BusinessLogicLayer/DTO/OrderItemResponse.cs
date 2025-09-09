namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderItemResponse(Guid ProductID, decimal UnitPrice, int Quantity, decimal TotalPrice,
         String? ProductName =null,String? Category=null,String? ImgUrl=null)
{
    public OrderItemResponse() : this(default, default, default, default,null,null,null)
    {
    }
}