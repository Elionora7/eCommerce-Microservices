using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Driver;
using Xunit;
using FluentAssertions;

namespace OrdersUnitTests
{
    public class OrdersServiceTests
    {
        private readonly OrdersService _ordersService;
        private readonly Mock<IOrdersRepository> _mockOrdersRepo = new();
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IValidator<OrderAddRequest>> _mockOrderAddValidator = new();
        private readonly Mock<IValidator<OrderItemAddRequest>> _mockOrderItemAddValidator = new();
        private readonly Mock<IValidator<OrderUpdateRequest>> _mockOrderUpdateValidator = new();
        private readonly Mock<IValidator<OrderItemUpdateRequest>> _mockOrderItemUpdateValidator = new();
        private readonly Mock<IUserMicroserviceClient> _mockUserClient = new();
        private readonly Mock<IProductMicroserviceClient> _mockProductClient = new();
        private readonly Mock<ILogger<OrdersService>> _mockLogger = new();

        public OrdersServiceTests()
        {
            // Validators always succeed
            _mockOrderAddValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderAddRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockOrderItemAddValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderItemAddRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockOrderUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderUpdateRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockOrderItemUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderItemUpdateRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            //tell AutoMapper mock how to map requests -> entity
            _mockMapper.Setup(m => m.Map<Order>(It.IsAny<OrderAddRequest>()))
                .Returns((OrderAddRequest req) => new Order
                {
                    OrderID = Guid.NewGuid(),
                    UserID = req.UserID,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = req.OrderItems.Select(i => new OrderItem
                    {
                        ProductID = i.ProductID,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.Quantity * i.UnitPrice
                    }).ToList()
                });

            _mockMapper.Setup(m => m.Map<Order>(It.IsAny<OrderUpdateRequest>()))
                .Returns((OrderUpdateRequest req) => new Order
                {
                    OrderID = req.OrderID,
                    UserID = req.UserID,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = req.OrderItems.Select(i => new OrderItem
                    {
                        ProductID = i.ProductID,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.Quantity * i.UnitPrice
                    }).ToList()
                });

            _ordersService = new OrdersService(
                _mockOrdersRepo.Object,
                _mockMapper.Object,
                _mockOrderAddValidator.Object,
                _mockOrderItemAddValidator.Object,
                _mockOrderUpdateValidator.Object,
                _mockOrderItemUpdateValidator.Object,
                _mockUserClient.Object,
                _mockProductClient.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task AddOrder_ReturnsEnrichedOrder()
        {
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var orderRequest = new OrderAddRequest
            {
                UserID = userId,
                OrderItems = new List<OrderItemAddRequest>
                {
                    new() { ProductID = productId, Quantity = 2, UnitPrice = 10 }
                }
            };

            var orderEntity = new Order
            {
                OrderID = Guid.NewGuid(),
                UserID = userId,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductID = productId, Quantity = 2, UnitPrice = 10, TotalPrice = 20 }
                },
                TotalBill = 20
            };

            var orderResponse = new OrderResponse
            {
                OrderID = orderEntity.OrderID,
                UserID = userId,
                OrderItems = new List<OrderItemResponse>
                {
                    new OrderItemResponse { ProductID = productId, Quantity = 2, UnitPrice = 10, TotalPrice = 20 }
                }
            };

            _mockProductClient.Setup(p => p.GetProductByProductId(productId))
                .ReturnsAsync(new ProductDTO(productId, "Test Product", "Electronics", 99.99, 10, "test.jpg"));

            _mockUserClient.Setup(u => u.GetUserByUserId(userId))
                .ReturnsAsync(new UserDTO(userId, "john@test.com", "John Smith", "Male"));

            _mockOrdersRepo.Setup(r => r.AddOrder(It.IsAny<Order>()))
                .ReturnsAsync(orderEntity);

            _mockMapper.Setup(m => m.Map<OrderResponse>(It.IsAny<Order>()))
                .Returns(orderResponse);

            var result = await _ordersService.AddOrder(orderRequest);

            result.Should().NotBeNull();
            result!.Name.Should().Be("John Smith");
            result.OrderItems.First().ProductName.Should().Be("Test Product");
        }

        [Fact]
        public async Task UpdateOrder_ReturnsUpdatedEnrichedOrder()
        {
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var updateRequest = new OrderUpdateRequest
            {
                OrderID = Guid.NewGuid(),
                UserID = userId,
                OrderItems = new List<OrderItemUpdateRequest>
                {
                    new() { ProductID = productId, Quantity = 1, UnitPrice = 5 }
                }
            };

            var orderEntity = new Order
            {
                OrderID = updateRequest.OrderID,
                UserID = userId,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductID = productId, Quantity = 1, UnitPrice = 5, TotalPrice = 5 }
                },
                TotalBill = 5
            };

            var orderResponse = new OrderResponse
            {
                OrderID = orderEntity.OrderID,
                UserID = userId,
                OrderItems = new List<OrderItemResponse>
                {
                    new OrderItemResponse { ProductID = productId, Quantity = 1, UnitPrice = 5, TotalPrice = 5 }
                }
            };

            _mockProductClient.Setup(p => p.GetProductByProductId(productId))
                .ReturnsAsync(new ProductDTO(productId, "Updated Product", "Electronics", 49.99, 5, "updated.jpg"));

            _mockUserClient.Setup(u => u.GetUserByUserId(userId))
                .ReturnsAsync(new UserDTO(userId, "jane@test.com", "Jane Doe", "Female"));

            _mockOrdersRepo.Setup(r => r.UpdateOrder(It.IsAny<Order>())).ReturnsAsync(orderEntity);
            _mockMapper.Setup(m => m.Map<OrderResponse>(It.IsAny<Order>())).Returns(orderResponse);

            var result = await _ordersService.UpdateOrder(updateRequest);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Jane Doe");
            result.OrderItems.First().ProductName.Should().Be("Updated Product");
        }

        [Fact]
        public async Task DeleteOrder_ReturnsTrue_WhenOrderExists()
        {
            var orderId = Guid.NewGuid();
            _mockOrdersRepo.Setup(r => r.GetOrderByCondition(It.IsAny<FilterDefinition<Order>>()))
                .ReturnsAsync(new Order { OrderID = orderId });
            _mockOrdersRepo.Setup(r => r.DeleteOrder(orderId)).ReturnsAsync(true);

            var result = await _ordersService.DeleteOrder(orderId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetOrders_ReturnsEnrichedOrders()
        {
            var orders = new List<Order> { new Order { OrderID = Guid.NewGuid(), OrderItems = new List<OrderItem>() } };
            var orderResponses = new List<OrderResponse?> { new OrderResponse { OrderID = orders.First().OrderID } };

            _mockOrdersRepo.Setup(r => r.GetOrders()).ReturnsAsync(orders);
            _mockMapper.Setup(m => m.Map<List<OrderResponse?>>(orders)).Returns(orderResponses);

            var result = await _ordersService.GetOrders();

            result.Should().HaveCount(1);
        }
    }
}
