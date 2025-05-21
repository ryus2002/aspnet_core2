using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Messaging.Publishers;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// 訂單服務實現
    /// </summary>
    public partial class OrderService : IOrderService
    {
        private readonly OrderDbContext _dbContext;
        private readonly ICartService _cartService;
        private readonly ILogger<OrderService> _logger;
        private readonly OrderEventPublisher _orderEventPublisher;

        /// <summary>
        /// 建構函數
        /// </summary>
        public OrderService(
            OrderDbContext dbContext, 
            ICartService cartService, 
            ILogger<OrderService> logger,
            OrderEventPublisher orderEventPublisher)
        {
            _dbContext = dbContext;
            _cartService = cartService;
            _logger = logger;
            _orderEventPublisher = orderEventPublisher;
        }

        /// <summary>
        /// 創建訂單
        /// </summary>
        public async Task<OrderResponse> CreateOrderAsync(string userId, CreateOrderRequest request)
        {
            // 使用事務確保數據一致性
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 獲取購物車
                CartResponse cart;
                if (request.CartId.HasValue)
                {
                    var cartResult = await _cartService.GetCartByIdAsync(request.CartId.Value);
                    if (cartResult == null)
                    {
                        throw new KeyNotFoundException($"購物車不存在: {request.CartId.Value}");
                    }
                    cart = cartResult;
                }
                else if (!string.IsNullOrEmpty(request.SessionId))
                {
                    var cartResult = await _cartService.GetCartBySessionIdAsync(request.SessionId);
                    if (cartResult == null)
                    {
                        throw new KeyNotFoundException($"會話購物車不存在: {request.SessionId}");
                    }
                    cart = cartResult;
                }
                else
                {
                    throw new ArgumentException("必須提供CartId或SessionId");
                }

                // 檢查購物車是否為空
                if (cart.Items.Count == 0)
                {
                    throw new InvalidOperationException("購物車為空，無法創建訂單");
                }

                // 處理地址
                int? shippingAddressId = request.ShippingAddressId;
                int? billingAddressId = request.BillingAddressId;

                // 如果沒有提供現有地址ID，但提供了地址信息，則創建新地址
                if (!shippingAddressId.HasValue && request.ShippingAddress != null)
                {
                    var shippingAddress = new Address
                    {
                        UserId = userId,
                        Name = request.ShippingAddress.Name,
                        Phone = request.ShippingAddress.Phone,
                        AddressLine1 = request.ShippingAddress.AddressLine1,
                        AddressLine2 = request.ShippingAddress.AddressLine2,
                        City = request.ShippingAddress.City,
                        State = request.ShippingAddress.State,
                        PostalCode = request.ShippingAddress.PostalCode,
                        Country = request.ShippingAddress.Country,
                        IsDefault = request.ShippingAddress.SaveAsDefault,
                        AddressType = "shipping",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _dbContext.Addresses.Add(shippingAddress);
                    await _dbContext.SaveChangesAsync();
                    shippingAddressId = shippingAddress.Id;
                }

                if (!billingAddressId.HasValue && request.BillingAddress != null)
                {
                    var billingAddress = new Address
                    {
                        UserId = userId,
                        Name = request.BillingAddress.Name,
                        Phone = request.BillingAddress.Phone,
                        AddressLine1 = request.BillingAddress.AddressLine1,
                        AddressLine2 = request.BillingAddress.AddressLine2,
                        City = request.BillingAddress.City,
                        State = request.BillingAddress.State,
                        PostalCode = request.BillingAddress.PostalCode,
                        Country = request.BillingAddress.Country,
                        IsDefault = request.BillingAddress.SaveAsDefault,
                        AddressType = "billing",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _dbContext.Addresses.Add(billingAddress);
                    await _dbContext.SaveChangesAsync();
                    billingAddressId = billingAddress.Id;
                }

                // 生成訂單編號
                var orderNumber = await GenerateOrderNumberAsync();

                // 創建訂單
                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderNumber = orderNumber,
                    UserId = userId,
                    Status = "pending",
                    TotalAmount = cart.TotalAmount,
                    ItemsCount = cart.ItemsCount,
                    ShippingAddressId = shippingAddressId,
                    BillingAddressId = billingAddressId,
                    ShippingMethod = request.ShippingMethod,
                    Notes = request.Notes,
                    Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Orders.Add(order);

                // 創建訂單項目
                foreach (var cartItem in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        VariantId = cartItem.VariantId,
                        Name = cartItem.Name,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.UnitPrice,
                        TotalPrice = cartItem.UnitPrice * cartItem.Quantity,
                        Attributes = cartItem.Attributes != null ? JsonSerializer.Serialize(cartItem.Attributes) : null
                    };
                    _dbContext.OrderItems.Add(orderItem);
                }

                // 創建訂單狀態歷史
                var statusHistory = new OrderStatusHistory
                {
                    OrderId = order.Id,
                    Status = "pending",
                    Comment = "訂單已創建",
                    ChangedBy = userId,
                    ChangedAt = DateTime.UtcNow
                };
                _dbContext.OrderStatusHistories.Add(statusHistory);

                // 創建訂單事件
                var orderEvent = new OrderEvent
                {
                    OrderId = order.Id,
                    EventType = "created",
                    Payload = JsonSerializer.Serialize(new { OrderId = order.Id, UserId = userId }),
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.OrderEvents.Add(orderEvent);

                // 更新購物車狀態為已轉換
                await _cartService.UpdateCartStatusAsync(cart.Id, "converted");

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // 發送訂單創建事件
                await _orderEventPublisher.PublishOrderCreatedEventAsync(order);

                _logger.LogInformation("Created order {OrderId} for user {UserId}", order.Id, userId);

                return await GetOrderResponseAsync(order.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating order for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// 根據ID獲取訂單
        /// </summary>
        public async Task<OrderResponse?> GetOrderByIdAsync(string id)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", id);
                return null;
            }

            return await GetOrderResponseAsync(id);
        }

        /// <summary>
        /// 根據訂單編號獲取訂單
        /// </summary>
        public async Task<OrderResponse?> GetOrderByNumberAsync(string orderNumber)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

            if (order == null)
            {
                _logger.LogWarning("Order with number {OrderNumber} not found", orderNumber);
                return null;
            }

            return await GetOrderResponseAsync(order.Id);
        }

        /// <summary>
        /// 生成訂單編號
        /// </summary>
        public async Task<string> GenerateOrderNumberAsync()
        {
            // 生成格式: ORD-YYYYMMDD-XXXXX (年月日-5位數序號)
            string prefix = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-";
            
            // 獲取當天最後一個訂單編號
            var lastOrder = await _dbContext.Orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastOrder != null)
            {
                // 從最後一個訂單編號中提取序號
                string sequenceStr = lastOrder.OrderNumber.Substring(prefix.Length);
                if (int.TryParse(sequenceStr, out int lastSequence))
                {
                    sequence = lastSequence + 1;
                }
            }

            return prefix + sequence.ToString("D5");
        }
    }
}