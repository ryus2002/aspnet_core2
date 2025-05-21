using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Services;
using OrderService.Messaging.Publishers;
using Xunit;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Messaging;

namespace OrderService.Tests.Services
{
    // 創建一個接口，用於模擬 OrderEventPublisher
    public interface IOrderEventPublisher
    {
        Task PublishOrderCreatedEventAsync(Order order);
        Task PublishOrderStatusChangedEventAsync(Order order, string oldStatus, string newStatus, string? reason = null);
    }

    public class OrderServiceTests
    {
        private readonly OrderDbContext _dbContext;
        private readonly Mock<ICartService> _mockCartService;
        private readonly Mock<ILogger<OrderService.Services.OrderService>> _mockLogger;
        private readonly Mock<IOrderEventPublisher> _mockOrderEventPublisher;
        private readonly IOrderService _orderService;

        public OrderServiceTests()
        {
            // 設置真實的數據庫上下文，但使用 In-Memory 數據庫
            // 忽略事務相關的警告，因為 In-Memory 數據庫不支持事務
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings => 
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            
            _dbContext = new OrderDbContext(options);
            _mockCartService = new Mock<ICartService>();
            _mockLogger = new Mock<ILogger<OrderService.Services.OrderService>>();
            _mockOrderEventPublisher = new Mock<IOrderEventPublisher>();

            // 創建一個包裝類，將 IOrderEventPublisher 轉換為 OrderEventPublisher
            var orderEventPublisher = new OrderEventPublisherWrapper(_mockOrderEventPublisher.Object);

            _orderService = new OrderService.Services.OrderService(
                _dbContext,
                _mockCartService.Object,
                _mockLogger.Object,
                orderEventPublisher
            );
        }

        [Fact]
        public async Task CreateOrderAsync_WithValidCartId_ShouldCreateOrder()
        {
            // Arrange
            var userId = "user1";
            var cartId = 1;
            var shippingAddressId = 1;
            var billingAddressId = 2;

            // 模擬購物車項目
            var cartItems = new List<CartItemResponse>
            {
                new CartItemResponse
                {
                    Id = 1,
                    ProductId = "product1",
                    Name = "Product 1",
                    Quantity = 2,
                    UnitPrice = 100.00m,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new CartItemResponse
                {
                    Id = 2,
                    ProductId = "product2",
                    Name = "Product 2",
                    Quantity = 1,
                    UnitPrice = 50.00m,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            // 創建一個有效的購物車響應
            var cart = new CartResponse
            {
                Id = cartId,
                UserId = userId,
                SessionId = "session1",
                Status = "active",
                Items = cartItems,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 模擬購物車服務
            _mockCartService.Setup(s => s.GetCartByIdAsync(cartId))
                .ReturnsAsync(cart);
            
            // 創建一個更新狀態後的購物車響應
            var updatedCart = new CartResponse
            {
                Id = cartId,
                UserId = userId,
                SessionId = "session1",
                Status = "converted",
                Items = cartItems,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockCartService.Setup(s => s.UpdateCartStatusAsync(cartId, "converted"))
                .ReturnsAsync(updatedCart);

            // 模擬 GetOrderResponseAsync 方法
            // 由於這是一個私有方法，我們需要在數據庫中添加必要的數據，以便 GetOrderResponseAsync 能夠正常工作
            // 添加測試用的地址數據
            var shippingAddress = new Address
            {
                Id = shippingAddressId,
                UserId = userId,
                Name = "Shipping Name",
                Phone = "123456789",
                AddressLine1 = "Shipping Address Line 1",
                City = "Shipping City",
                PostalCode = "12345",
                Country = "Shipping Country",
                AddressType = "shipping",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Addresses.Add(shippingAddress);

            var billingAddress = new Address
            {
                Id = billingAddressId,
                UserId = userId,
                Name = "Billing Name",
                Phone = "987654321",
                AddressLine1 = "Billing Address Line 1",
                City = "Billing City",
                PostalCode = "54321",
                Country = "Billing Country",
                AddressType = "billing",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Addresses.Add(billingAddress);

            await _dbContext.SaveChangesAsync();

            // 創建訂單請求
            var request = new CreateOrderRequest
            {
                CartId = cartId,
                ShippingAddressId = shippingAddressId,
                BillingAddressId = billingAddressId,
                ShippingMethod = "standard",
                Notes = "Test order"
            };

            // Act
            var result = await _orderService.CreateOrderAsync(userId, request);

            // Assert
            // 驗證訂單是否被創建
            var orders = await _dbContext.Orders.ToListAsync();
            orders.Should().HaveCount(1);
            
            // 驗證訂單項目是否被創建
            var orderItems = await _dbContext.OrderItems.ToListAsync();
            orderItems.Should().HaveCount(2);
            
            // 驗證訂單狀態歷史是否被創建
            var statusHistories = await _dbContext.OrderStatusHistories.ToListAsync();
            statusHistories.Should().HaveCount(1);
            
            // 驗證購物車狀態是否被更新
            _mockCartService.Verify(s => s.UpdateCartStatusAsync(cartId, "converted"), Times.Once);
            
            // 驗證訂單創建事件是否被發布
            _mockOrderEventPublisher.Verify(p => p.PublishOrderCreatedEventAsync(It.IsAny<Order>()), Times.Once);
            
            // 驗證返回的訂單響應
            result.Should().NotBeNull();
            result.UserId.Should().Be(userId);
            result.TotalAmount.Should().Be(250.00m); // 2*100 + 1*50
            result.Items.Should().HaveCount(2);
            
            // 驗證訂單中的元數據是否包含購物車ID (如果OrderService實現中有存儲這個信息)
            if (result.Metadata != null && result.Metadata.ContainsKey("CartId"))
            {
                result.Metadata["CartId"].Should().Be(cartId.ToString());
            }
        }

        [Fact]
        public async Task CreateOrderAsync_WithEmptyCart_ShouldThrowException()
        {
            // Arrange
            var userId = "user1";
            var cartId = 1;

            // 模擬空購物車
            var cart = new CartResponse
            {
                Id = cartId,
                UserId = userId,
                SessionId = "session1",
                Status = "active",
                Items = new List<CartItemResponse>(), // 空購物車項目
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 模擬購物車服務
            _mockCartService.Setup(s => s.GetCartByIdAsync(cartId))
                .ReturnsAsync(cart);

            // 創建訂單請求
            var request = new CreateOrderRequest
            {
                CartId = cartId,
                ShippingAddressId = 1,
                BillingAddressId = 2,
                ShippingMethod = "standard"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _orderService.CreateOrderAsync(userId, request));
        }

        [Fact]
        public async Task CreateOrderAsync_WithNonExistingCart_ShouldThrowException()
        {
            // Arrange
            var userId = "user1";
            var cartId = 999; // 不存在的購物車 ID

            // 模擬購物車服務
            _mockCartService.Setup(s => s.GetCartByIdAsync(cartId))
                .ReturnsAsync((CartResponse?)null);

            // 創建訂單請求
            var request = new CreateOrderRequest
            {
                CartId = cartId,
                ShippingAddressId = 1,
                BillingAddressId = 2,
                ShippingMethod = "standard"
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _orderService.CreateOrderAsync(userId, request));
        }
    }

    // 創建一個包裝類，將 IOrderEventPublisher 轉換為 OrderEventPublisher
    public class OrderEventPublisherWrapper : OrderEventPublisher
    {
        private readonly IOrderEventPublisher _publisher;

        public OrderEventPublisherWrapper(IOrderEventPublisher publisher)
            : base(Mock.Of<IMessageBus>(), Mock.Of<ILogger<OrderEventPublisher>>())
        {
            _publisher = publisher;
        }

        public override async Task PublishOrderCreatedEventAsync(Order order)
        {
            await _publisher.PublishOrderCreatedEventAsync(order);
        }

        public override async Task PublishOrderStatusChangedEventAsync(Order order, string oldStatus, string newStatus, string? reason = null)
        {
            await _publisher.PublishOrderStatusChangedEventAsync(order, oldStatus, newStatus, reason);
        }
    }
}