﻿namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderResponse(Guid OrderID, Guid UserID, decimal TotalBill, DateTime OrderDate, string? Name, string? Email, List<OrderItemResponse> OrderItems)
{
    public OrderResponse() : this(default, default, default, default, default, default, default)
    {
    }
}