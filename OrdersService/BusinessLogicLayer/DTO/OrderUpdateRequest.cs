namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderUpdateRequest(
    Guid OrderID,
    Guid UserID,
    DateTime OrderDate,
    List<OrderItemUpdateRequest> OrderItems)
{
    // Initialize with non-null defaults
    public OrderUpdateRequest()
        : this(
            OrderID: Guid.Empty,
            UserID: Guid.Empty,
            OrderDate: DateTime.MinValue,
            OrderItems: new List<OrderItemUpdateRequest>())
    {
    }
}
