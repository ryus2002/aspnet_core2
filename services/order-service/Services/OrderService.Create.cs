using OrderService.DTOs;
using OrderService.Models;
using OrderService.Messaging.Publishers;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Services
{
    /// <summary>
    /// 訂單服務的創建訂單相關功能
    /// </summary>
    public partial class OrderService : IOrderService
    {
        /// <summary>
        /// 創建訂單並發布事件
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="request">創建訂單請求</param>
        /// <returns>訂單響應</returns>
        public async Task<OrderResponse> CreateOrderWithEventsAsync(string userId, CreateOrderRequest request)
        {
            _logger.LogInformation("開始創建訂單: UserId={UserId}, CartId={CartId}", userId, request.CartId);

            // 獲取購物車
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == request.CartId && c.UserId == userId);

            if (cart == null)
            {
                throw new KeyNotFoundException($"找不到購物車: {request.CartId}");
            }

            if (cart.Items.Count == 0)
            {
                throw new InvalidOperationException("購物車為空，無法創建訂單");
            }

            // 生成訂單編號
            var orderNumber = await GenerateOrderNumberAsync();

            // 先創建配送地址
            var shippingAddress = new Address
            {
                Name = request.ShippingAddress?.Name ?? "未提供",
                Phone = request.ShippingAddress?.Phone ?? "未提供",
                AddressLine1 = request.ShippingAddress?.AddressLine1 ?? "未提供",
                AddressLine2 = request.ShippingAddress?.AddressLine2,
                City = request.ShippingAddress?.City ?? "未提供",
                State = request.ShippingAddress?.State,
                PostalCode = request.ShippingAddress?.PostalCode ?? "未提供",
                Country = request.ShippingAddress?.Country ?? "未提供",
                UserId = userId,
                AddressType = "shipping"
            };
            _dbContext.Addresses.Add(shippingAddress);
            await _dbContext.SaveChangesAsync();

            // 創建帳單地址
            var billingAddress = new Address
            {
                Name = request.BillingAddress?.Name ?? "未提供",
                Phone = request.BillingAddress?.Phone ?? "未提供",
                AddressLine1 = request.BillingAddress?.AddressLine1 ?? "未提供",
                AddressLine2 = request.BillingAddress?.AddressLine2,
                City = request.BillingAddress?.City ?? "未提供",
                State = request.BillingAddress?.State,
                PostalCode = request.BillingAddress?.PostalCode ?? "未提供",
                Country = request.BillingAddress?.Country ?? "未提供",
                UserId = userId,
                AddressType = "billing"
            };
            _dbContext.Addresses.Add(billingAddress);
            await _dbContext.SaveChangesAsync();

            // 創建訂單
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                OrderNumber = orderNumber,
                UserId = userId,
                Status = "Pending",
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.UnitPrice),
                ShippingAddressId = shippingAddress.Id,
                BillingAddressId = billingAddress.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 添加訂單項目
            foreach (var cartItem in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    VariantId = cartItem.VariantId,
                    Name = cartItem.Name,
                    // 移除 ImageUrl = cartItem.ImageUrl 這一行，因為 CartItem 沒有 ImageUrl 屬性
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.Quantity * cartItem.UnitPrice
                };
                order.Items.Add(orderItem);
            }

            // 添加訂單狀態歷史
            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = "pending",
                Comment = "訂單已創建",
                ChangedAt = DateTime.UtcNow,
                ChangedBy = userId
            };
            order.StatusHistory.Add(statusHistory);

            // 保存訂單
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // 更新購物車狀態
            cart.Status = "Ordered";
            cart.UpdatedAt = DateTime.UtcNow;
            _dbContext.Carts.Update(cart);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("訂單創建成功: OrderId={OrderId}, OrderNumber={OrderNumber}", order.Id, order.OrderNumber);

            // 發布訂單創建事件
            try
            {
                await _orderEventPublisher.PublishOrderCreatedEventAsync(order);
            }  
            catch (Exception ex)
            {
                // 記錄錯誤但不影響訂單創建流程
                _logger.LogError(ex, "發布訂單創建事件失敗: OrderId={OrderId}", order.Id);
            }

            // 返回訂單響應 - 修改這裡，傳遞訂單ID而不是訂單物件
            return await GetOrderResponseAsync(order.Id);
        }
    }
}