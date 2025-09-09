namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderResponse(
    Guid OrderID,
    Guid UserID,
    decimal TotalBill,
    DateTime OrderDate,
    string Name,                     
    string Email,                    
    List<OrderItemResponse> OrderItems)
{
    public OrderResponse()
        : this(
            OrderID: Guid.Empty,
            UserID: Guid.Empty,
            TotalBill: 0m,
            OrderDate: DateTime.UtcNow,
            Name: "[Name Not Provided]",   
            Email: "[Email Not Provided]", 
            OrderItems: new List<OrderItemResponse>()) 
    {
    }
}