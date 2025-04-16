using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;

public interface IOrdersService
{
    /// <summary>
    /// Retrieves the list of orders from the orders repository
    /// </summary>
    /// <returns>Returns list of OrderResponse objects</returns>
    Task<List<OrderResponse?>> GetOrders();


    /// <summary>
    /// Returns list of orders matching with given condition
    /// </summary>
    /// <param name="condition">Expression that represents condition to check</param>
    /// <returns>Return matching orders as OrderResponse objects</returns>
    Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> condition);

    /// <summary>
    /// Returns a single order that matches with given condition
    /// </summary>
    /// <param name="condition">Expression that represents the condition to check</param>
    /// <returns>Return matching order object as OrderResponse; or null if not found</returns>
    Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> condition);


    /// <summary>
    /// Add (inserts) order into the collection using orders repository
    /// </summary>
    /// <param name="orderAddRequest">Order to insert</param>
    /// <returns>Returns OrderResponse object that contains order details after inserting; or returns null if insert is unsuccessfull.</returns>
    Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest);


    /// <summary>
    /// Update the existing order based on the OrderID
    /// </summary>
    /// <param name="orderUpdateRequest">Order data to update</param>
    /// <returns>Return order object after successfull update; otherwise null</returns>
    Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest);


    /// <summary>
    /// Delete an existing order based on given order id
    /// </summary>
    /// <param name="orderID">OrderID to search and delete</param>
    /// <returns>Return true if the deletion is successfull; otherwise false</returns>
    Task<bool> DeleteOrder(Guid orderID);
}