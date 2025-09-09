using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;

public class OrdersService : IOrdersService
{
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IOrdersRepository _ordersRepository;
    private readonly UserMicroserviceClient _userMicroserviceClient;
    private readonly ProductMicroserviceClient _productMicroserviceClient;
    private readonly ILogger<OrdersService> _logger;

    public OrdersService(
        IOrdersRepository ordersRepository,
        IMapper mapper,
        IValidator<OrderAddRequest> orderAddRequestValidator,
        IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
        IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
        IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
        UserMicroserviceClient userMicroserviceClient,
        ProductMicroserviceClient productMicroserviceClient,
        ILogger<OrdersService> logger)
    {
        _ordersRepository = ordersRepository ?? throw new ArgumentNullException(nameof(ordersRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _orderAddRequestValidator = orderAddRequestValidator ?? throw new ArgumentNullException(nameof(orderAddRequestValidator));
        _orderItemAddRequestValidator = orderItemAddRequestValidator ?? throw new ArgumentNullException(nameof(orderItemAddRequestValidator));
        _orderUpdateRequestValidator = orderUpdateRequestValidator ?? throw new ArgumentNullException(nameof(orderUpdateRequestValidator));
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator ?? throw new ArgumentNullException(nameof(orderItemUpdateRequestValidator));
        _userMicroserviceClient = userMicroserviceClient ?? throw new ArgumentNullException(nameof(userMicroserviceClient));
        _productMicroserviceClient = productMicroserviceClient ?? throw new ArgumentNullException(nameof(productMicroserviceClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        ArgumentNullException.ThrowIfNull(orderAddRequest);

        await ValidateRequest(_orderAddRequestValidator, orderAddRequest);

        // Validate and fetch products
        var products = new List<ProductDTO>();
        foreach (var item in orderAddRequest.OrderItems)
        {
            await ValidateRequest(_orderItemAddRequestValidator, item);
            var product = await _productMicroserviceClient.GetProductByProductId(item.ProductID)
                ?? throw new ArgumentException("Invalid Product ID", nameof(item.ProductID));
            products.Add(product);
        }

        // Validate user
        var user = await _userMicroserviceClient.GetUserByUserId(orderAddRequest.UserID)
            ?? throw new ArgumentException("Invalid User ID", nameof(orderAddRequest.UserID));

        // Map & calculate
        var order = _mapper.Map<Order>(orderAddRequest);
        CalculateTotals(order);

        var addedOrder = await _ordersRepository.AddOrder(order);
        if (addedOrder is null) return null;

        var orderResponse = _mapper.Map<OrderResponse>(addedOrder);
        var enrichedWithProducts = EnrichOrderWithProducts(orderResponse, products);
        var enrichedWithUser = EnrichOrderWithUser(enrichedWithProducts, user);

        return enrichedWithUser;
    }

    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        ArgumentNullException.ThrowIfNull(orderUpdateRequest);
        await ValidateRequest(_orderUpdateRequestValidator, orderUpdateRequest);

        var products = new List<ProductDTO>();
        foreach (var item in orderUpdateRequest.OrderItems)
        {
            await ValidateRequest(_orderItemUpdateRequestValidator, item);
            var product = await _productMicroserviceClient.GetProductByProductId(item.ProductID)
                ?? throw new ArgumentException($"Product {item.ProductID} does not exist", nameof(item.ProductID));
            products.Add(product);
        }

        var user = await _userMicroserviceClient.GetUserByUserId(orderUpdateRequest.UserID)
            ?? throw new ArgumentException("Invalid User ID", nameof(orderUpdateRequest.UserID));

        var order = _mapper.Map<Order>(orderUpdateRequest);
        CalculateTotals(order);

        var updatedOrder = await _ordersRepository.UpdateOrder(order);
        if (updatedOrder is null) return null;

        var orderResponse = _mapper.Map<OrderResponse>(updatedOrder);
        var enrichedWithProducts = EnrichOrderWithProducts(orderResponse, products);
        var enrichedWithUser = EnrichOrderWithUser(enrichedWithProducts, user);

        return enrichedWithUser;
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        var existingOrder = await _ordersRepository.GetOrderByCondition(Builders<Order>.Filter.Eq(o => o.OrderID, orderID));
        if (existingOrder is null) return false;

        return await _ordersRepository.DeleteOrder(orderID);
    }

    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        var order = await _ordersRepository.GetOrderByCondition(filter);
        if (order is null) return null;

        var orderResponse = _mapper.Map<OrderResponse>(order);
        return await EnrichOrderWithExternalData(orderResponse);
    }

    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        var orders = await _ordersRepository.GetOrdersByCondition(filter);
        return await MapAndEnrichOrders(orders);
    }

    public async Task<List<OrderResponse?>> GetOrders()
    {
        var orders = await _ordersRepository.GetOrders();
        return await MapAndEnrichOrders(orders);
    }

    // =====================
    // Private helper methods
    // =====================
    private static void CalculateTotals(Order order)
    {
        foreach (var item in order.OrderItems)
            item.TotalPrice = item.Quantity * item.UnitPrice;

        order.TotalBill = order.OrderItems.Sum(i => i.TotalPrice);
    }

    private static async Task ValidateRequest<T>(IValidator<T> validator, T request)
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new ArgumentException(string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
    }

    private OrderResponse EnrichOrderWithProducts(OrderResponse orderResponse, IEnumerable<ProductDTO> products)
    {
        var enrichedItems = new List<OrderItemResponse>();

        foreach (var item in orderResponse.OrderItems)
        {
            var product = products.FirstOrDefault(p => p.ProductID == item.ProductID);
            if (product != null)
            {
                var enrichedItem = item with
                {
                    ProductName = product.ProductName,
                    Category = product.Category,
                };
                enrichedItems.Add(enrichedItem);
            }
            else
            {
                // Handle missing product data
                var fallbackItem = item with
                {
                    ProductName = "Product temporarily unavailable",
                    Category = "Unknown"
                };
                enrichedItems.Add(fallbackItem);
                _logger.LogWarning("Product {ProductId} data not available for order {OrderId}", item.ProductID, orderResponse.OrderID);
            }
        }

        return orderResponse with
        {
            OrderItems = enrichedItems
        };
    }

    private OrderResponse EnrichOrderWithUser(OrderResponse orderResponse, UserDTO user)
    {
        if (user != null && user.UserID != Guid.Empty)
        {
            return orderResponse with
            {
                Name = user.Name ?? "Unknown User",
                Email = user.Email ?? "No email provided"
            };
        }
        else
        {
            _logger.LogWarning("Invalid user data provided for order enrichment");
            return orderResponse with
            {
                Name = "User information unavailable",
                Email = "Service temporarily down"
            };
        }
    }

    private async Task<OrderResponse> EnrichOrderWithExternalData(OrderResponse orderResponse)
    {
        // First, enrich the order items
        var enrichedItems = new List<OrderItemResponse>();

        foreach (var item in orderResponse.OrderItems)
        {
            var product = await _productMicroserviceClient.GetProductByProductId(item.ProductID);
            if (product != null)
            {
                // Create a new enriched item
                var enrichedItem = item with
                {
                    ProductName = product.ProductName,  // Use ProductName, not Name
                    Category = product.Category
                };
                enrichedItems.Add(enrichedItem);
            }
            else
            {
                enrichedItems.Add(item);
            }
        }

        // Enrich user data
        var user = await _userMicroserviceClient.GetUserByUserId(orderResponse.UserID);

        if (user != null && user.UserID != Guid.Empty)
        {
            // Return a new enriched order response
            return orderResponse with
            {
                Name = user.Name ?? "Unknown User",
                Email = user.Email ?? "No email provided",
                OrderItems = enrichedItems
            };
        }
        else
        {
            // Return with fallback values
            return orderResponse with
            {
                Name = "User information temporarily unavailable",
                Email = "Check back later",
                OrderItems = enrichedItems
            };
        }
    }

    private async Task<List<OrderResponse?>> MapAndEnrichOrders(IEnumerable<Order?> orders)
    {
        var orderResponses = _mapper.Map<List<OrderResponse?>>(orders);
        var enrichedResponses = new List<OrderResponse?>();

        foreach (var orderResponse in orderResponses.Where(o => o != null))
        {
            var enrichedResponse = await EnrichOrderWithExternalData(orderResponse!);
            enrichedResponses.Add(enrichedResponse);
        }

        return enrichedResponses;
    }
}