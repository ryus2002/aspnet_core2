using OrderService.DTOs;
using OrderService.Models;
using OrderService.Messaging.Publishers;
using Microsoft.Extensions.Logging;

namespace OrderService.Services
{
    /// <summary>
    /// 訂單服務的創建訂單相關功能
    /// </summary>
    public partial class OrderService : IOrderService
    {
        private readonly OrderEventPublisher _orderEventPublisher;

        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="request">創建訂單請求</param>
        /// <returns>訂單響應</returns>
        public async Task<OrderResponse> CreateOrderAsync(string userId, CreateOrderRequest request)
        {
            _logger.LogInformation("開始創建訂單: UserId={UserId}, CartId={CartId}", userId, request.CartId);

            // 獲取購物車
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == request.CartId && c.UserId == userId);

            if (cart == null)
            {
                throw new NotFoundException($"找不到購物車: {request.CartId}");
            }

            if (cart.Items.Count == 0)
            {
                throw new BadRequestException("購物車為空，無法創建訂單");
            }

            // 生成訂單編號
            var orderNumber = await GenerateOrderNumberAsync();

            // 創建訂單
            var order = new Order
            {
                Id = Guid.NewGuid().ToString(),
                OrderNumber = orderNumber,
                UserId = userId,
                Status = "Pending",
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.UnitPrice),
                ShippingAddress = new Address
                {
                    RecipientName = request.ShippingAddress.RecipientName,
                    PhoneNumber = request.ShippingAddress.PhoneNumber,
                    AddressLine1 = request.ShippingAddress.AddressLine1,
                    AddressLine2 = request.ShippingAddress.AddressLine2,
                    City = request.ShippingAddress.City,
                    State = request.ShippingAddress.State,
                    Country = request.ShippingAddress.Country,
                    PostalCode = request.ShippingAddress.PostalCode
                },
                BillingAddress = request.BillingAddress != null ? new Address
                {
                    RecipientName = request.BillingAddress.RecipientName,
                    PhoneNumber = request.BillingAddress.PhoneNumber,
                    AddressLine1 = request.BillingAddress.AddressLine1,
                    AddressLine2 = request.BillingAddress.AddressLine2,
                    City = request.BillingAddress.City,
                    State = request.BillingAddress.State,
                    Country = request.BillingAddress.Country,
                    PostalCode = request.BillingAddress.PostalCode
                } : null,
                PaymentMethod = request.PaymentMethod,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 添加訂單項目
            foreach (var cartItem in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    VariantId = cartItem.VariantId,
                    ProductName = cartItem.ProductName,
                    ProductImage = cartItem.ProductImage,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    SubTotal = cartItem.Quantity * cartItem.UnitPrice,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // 添加訂單狀態歷史
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = order.Id,
                Status = "Pending",
                Comment = "訂單已創建",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });

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

            // 返回訂單響應
            return await GetOrderResponseAsync(order);
        }
    }
}