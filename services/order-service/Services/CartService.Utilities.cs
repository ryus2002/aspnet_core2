using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// 購物車服務實現 - 工具方法和輔助功能
    /// </summary>
    public partial class CartService : ICartService
    {
        /// <summary>
        /// 合併購物車
        /// </summary>
        public async Task<CartResponse> MergeCartsAsync(string sessionId, string userId)
        {
            // 獲取會話購物車
            var sessionCart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Status == "active");

            // 獲取用戶購物車
            var userCart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "active");

            // 如果會話購物車不存在，直接返回用戶購物車
            if (sessionCart == null)
            {
                if (userCart == null)
                {
                    // 如果兩個購物車都不存在，創建一個新的
                    return await CreateCartAsync(new CreateCartRequest 
                    { 
                        SessionId = sessionId, 
                        UserId = userId 
                    });
                }
                return await GetCartResponseAsync(userCart);
            }

            // 如果用戶購物車不存在，將會話購物車轉換為用戶購物車
            if (userCart == null)
            {
                sessionCart.UserId = userId;
                sessionCart.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                return await GetCartResponseAsync(sessionCart);
            }

            // 合併兩個購物車的項目
            foreach (var item in sessionCart.Items)
            {
                // 檢查用戶購物車中是否已存在相同商品
                var existingItem = userCart.Items.FirstOrDefault(i => 
                    i.ProductId == item.ProductId && i.VariantId == item.VariantId);

                if (existingItem != null)
                {
                    // 更新數量
                    existingItem.Quantity += item.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // 將項目添加到用戶購物車
                    var newItem = new CartItem
                    {
                        CartId = userCart.Id,
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Name = item.Name,
                        Attributes = item.Attributes,
                        AddedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _dbContext.CartItems.Add(newItem);
                }
            }

            // 更新用戶購物車
            userCart.UpdatedAt = DateTime.UtcNow;
            
            // 將會話購物車標記為已合併
            sessionCart.Status = "merged";
            sessionCart.UpdatedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Merged cart {SessionCartId} into cart {UserCartId}", sessionCart.Id, userCart.Id);
            
            return await GetCartResponseAsync(userCart);
        }

        /// <summary>
        /// 更新購物車狀態
        /// </summary>
        public async Task<CartResponse> UpdateCartStatusAsync(int cartId, string status)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null)
            {
                throw new KeyNotFoundException($"購物車不存在: {cartId}");
            }

            // 更新狀態
            cart.Status = status;
            cart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated cart {CartId} status to {Status}", cartId, status);
            
            return await GetCartResponseAsync(cart);
        }

        /// <summary>
        /// 清理過期購物車
        /// </summary>
        public async Task<int> CleanupExpiredCartsAsync()
        {
            var now = DateTime.UtcNow;
            var expiredCarts = await _dbContext.Carts
                .Where(c => c.Status == "active" && c.ExpiresAt < now)
                .ToListAsync();

            foreach (var cart in expiredCarts)
            {
                cart.Status = "expired";
                cart.UpdatedAt = now;
            }

            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Cleaned up {Count} expired carts", expiredCarts.Count);
            
            return expiredCarts.Count;
        }

        /// <summary>
        /// 將購物車模型轉換為響應DTO
        /// </summary>
        private async Task<CartResponse> GetCartResponseAsync(Cart cart)
        {
            // 確保購物車項目已加載
            if (!_dbContext.Entry(cart).Collection(c => c.Items).IsLoaded)
            {
                await _dbContext.Entry(cart).Collection(c => c.Items).LoadAsync();
            }

            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                SessionId = cart.SessionId,
                Status = cart.Status,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                ExpiresAt = cart.ExpiresAt,
                Metadata = cart.Metadata != null ? 
                    JsonSerializer.Deserialize<Dictionary<string, object>>(cart.Metadata) : null,
                Items = cart.Items.Select(i => new CartItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    VariantId = i.VariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Name = i.Name,
                    Attributes = i.Attributes != null ? 
                        JsonSerializer.Deserialize<Dictionary<string, object>>(i.Attributes) : null,
                    AddedAt = i.AddedAt,
                    UpdatedAt = i.UpdatedAt
                }).ToList()
            };

            return response;
        }
    }
}