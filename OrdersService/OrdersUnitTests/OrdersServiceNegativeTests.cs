using System;
using System.Collections.Generic;
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
    public class OrdersServiceNegativeTests
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

        public OrdersServiceNegativeTests()
        {
            // Validators succeed unless overridden in individual tests
            _mockOrderAddValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderAddRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockOrderItemAddValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderItemAddRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockOrderUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderUpdateRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
            _mockOrderItemUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<OrderItemUpdateRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

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
        public async Task AddOrder_Throws_WhenUserNotFound()
        {
            // Arrange
            var badRequest = new OrderAddRequest
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItemAddRequest>
                {
                    new() { ProductID = Guid.NewGuid(), Quantity = 1, UnitPrice = 10 }
                }
            };

            // Product exists, but user lookup fails
            _mockProductClient.Setup(p => p.GetProductByProductId(It.IsAny<Guid>()))
                .ReturnsAsync(new ProductDTO(Guid.NewGuid(), "Name", "Cat", 10, 1, "img.jpg"));
            _mockUserClient.Setup(u => u.GetUserByUserId(It.IsAny<Guid>()))
                .ReturnsAsync((UserDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _ordersService.AddOrder(badRequest));
        }

        [Fact]
        public async Task AddOrder_Throws_WhenProductNotFound()
        {
            var request = new OrderAddRequest
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItemAddRequest>
                {
                    new() { ProductID = Guid.NewGuid(), Quantity = 2, UnitPrice = 20 }
                }
            };

            // User exists but product fails
            _mockUserClient.Setup(u => u.GetUserByUserId(It.IsAny<Guid>()))
                .ReturnsAsync(new UserDTO(Guid.NewGuid(), "a@b.com", "User", "M"));
            _mockProductClient.Setup(p => p.GetProductByProductId(It.IsAny<Guid>()))
                .ReturnsAsync((ProductDTO?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _ordersService.AddOrder(request));
        }

        [Fact]
        public async Task UpdateOrder_Throws_WhenOrderNotInRepo()
        {
            var orderUpdateRequest = new OrderUpdateRequest
            {
                OrderID = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItemUpdateRequest>
                {
                    new() { ProductID = Guid.NewGuid(), Quantity = 1, UnitPrice = 5 }
                }
            };

            var mappedOrder = new Order
            {
                OrderID = orderUpdateRequest.OrderID,
                UserID = orderUpdateRequest.UserID,
                OrderItems = orderUpdateRequest.OrderItems.Select(i => new OrderItem
                {
                    ProductID = i.ProductID,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            _mockMapper.Setup(m => m.Map<Order>(orderUpdateRequest)).Returns(mappedOrder);


            // External services OK
            _mockUserClient.Setup(u => u.GetUserByUserId(It.IsAny<Guid>()))
                .ReturnsAsync(new UserDTO(Guid.NewGuid(), "x@y.com", "Jane", "F"));
            _mockProductClient.Setup(p => p.GetProductByProductId(It.IsAny<Guid>()))
                .ReturnsAsync(new ProductDTO(Guid.NewGuid(), "Prod", "Cat", 10, 1, "img.jpg"));

            // Repo returns null on update
            _mockOrdersRepo.Setup(r => r.UpdateOrder(It.IsAny<Order>()))
                .ReturnsAsync((Order?)null);

            var result = await _ordersService.UpdateOrder(orderUpdateRequest);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteOrder_ReturnsFalse_WhenOrderNotFound()
        {
            var orderId = Guid.NewGuid();
            _mockOrdersRepo.Setup(r => r.GetOrderByCondition(It.IsAny<FilterDefinition<Order>>()))
                .ReturnsAsync((Order?)null);

            var result = await _ordersService.DeleteOrder(orderId);
            result.Should().BeFalse();
        }
    }
}
