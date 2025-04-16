using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;

public interface IOrdersRepository
{
    /// <summary>
    /// Retrieves all Orders asynchronously
    /// </summary>
    /// <returns>Returns all orders from the orders collection</returns>
    Task<IEnumerable<Order>> GetOrders();


    /// <summary>
    /// Retrieve all Orders based on the specified condition asynchronously
    /// </summary>
    /// <param name="condition">The condition to filter orders</param>
    /// <returns>Returns a collection of matching orders</returns>
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> condition);


    /// <summary>
    /// Retrieve an order based on the specified condition asynchronously
    /// </summary>
    /// <param name="condition">The condition to filter Orders</param>
    /// <returns>Returns matching order</returns>
    Task<Order?> GetOrderByCondition(FilterDefinition<Order> condition);


    /// <summary>
    /// Add a new Order into the Orders collection asynchronously
    /// </summary>
    /// <param name="order">The order to be added</param>
    /// <returns>Returns the added Order object or null if unsuccessful</returns>
    Task<Order?> AddOrder(Order order);


    /// <summary>
    /// Update an existing order asynchronously
    /// </summary>
    /// <param name="order">The order to be added</param>
    /// <returns>Returns the updated order object; or null if not found</returns>
    Task<Order?> UpdateOrder(Order order);


    /// <summary>
    /// Delete the order asynchronously
    /// </summary>
    /// <param name="orderID">The Order ID based on which we need to delete the order</param>
    /// <returns>Returns true if the deletion is successfull,otherwise return false</returns>
    Task<bool> DeleteOrder(Guid orderID);
}