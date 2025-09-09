namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderAddRequest(
    Guid UserID,
    DateTime OrderDate,
    List<OrderItemAddRequest> OrderItems)
{
    public OrderAddRequest(): this(
            UserID: Guid.Empty,
            OrderDate: DateTime.MinValue,
            OrderItems: new List<OrderItemAddRequest>())
    {
    }
}