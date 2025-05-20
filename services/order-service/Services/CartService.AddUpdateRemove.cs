using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// 購物車服務實現 - 添加、更新和刪除購物車項目
    /// </summary>
    public partial class CartService : ICartService
    {
        /// <summary>
        /// 添加購物車項目
        /// </summary>
        public async Task<CartResponse> AddCartItemAsync(int cartId, AddCartItemRequest request)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.Status == "active");

            if (cart == null)
            {
                throw new KeyNotFoundException($"購物車不存在或已失效: {cartId}");
            }

            // 檢查是否已存在相同商品和變體的項目
            var existingItem = cart.Items.FirstOrDefault(i => 
                i.ProductId == request.ProductId && 
                i.VariantId == request.VariantId);

            if (existingItem != null)
            {
                // 如果存在，則更新數量
                existingItem.Quantity += request.Quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Updated quantity for existing item in cart {CartId}, item {ItemId}", cartId, existingItem.Id);
            }
            else
            {
                // 獲取商品信息 (在實際項目中，這裡應該調用產品服務API)
                // 這裡簡化處理，假設已經有商品信息
                var productName = "商品名稱"; // 應從產品服務獲取
                var unitPrice = 100.00m; // 應從產品服務獲取

                // 創建新項目
                var newItem = new CartItem
                {
                    CartId = cartId,
                    ProductId = request.ProductId,
                    VariantId = request.VariantId,
                    Quantity = request.Quantity,
                    UnitPrice = unitPrice,
                    Name = productName,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.CartItems.Add(newItem);
                _logger.LogInformation("Added new item to cart {CartId}", cartId);
            }

            // 更新購物車
            cart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return await GetCartResponseAsync(cart);
        }

        /// <summary>
        /// 更新購物車項目
        /// </summary>
        public async Task<CartResponse> UpdateCartItemAsync(int cartId, int itemId, UpdateCartItemRequest request)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.Status == "active");

            if (cart == null)
            {
                throw new KeyNotFoundException($"購物車不存在或已失效: {cartId}");
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                throw new KeyNotFoundException($"購物車項目不存在: {itemId}");
            }

            // 更新數量
            item.Quantity = request.Quantity;
            item.UpdatedAt = DateTime.UtcNow;
            
            // 更新購物車
            cart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Updated item {ItemId} in cart {CartId}", itemId, cartId);
            
            return await GetCartResponseAsync(cart);
        }

        /// <summary>
        /// 刪除購物車項目
        /// </summary>
        public async Task<CartResponse> RemoveCartItemAsync(int cartId, int itemId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.Status == "active");

            if (cart == null)
            {
                throw new KeyNotFoundException($"購物車不存在或已失效: {cartId}");
            }

            var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
            {
                throw new KeyNotFoundException($"購物車項目不存在: {itemId}");
            }

            // 刪除項目
            _dbContext.CartItems.Remove(item);
            
            // 更新購物車
            cart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Removed item {ItemId} from cart {CartId}", itemId, cartId);
            
            return await GetCartResponseAsync(cart);
        }

        /// <summary>
        /// 清空購物車
        /// </summary>
        public async Task<CartResponse> ClearCartAsync(int cartId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.Status == "active");

            if (cart == null)
            {
                throw new KeyNotFoundException($"購物車不存在或已失效: {cartId}");
            }

            // 刪除所有項目
            _dbContext.CartItems.RemoveRange(cart.Items);
            
            // 更新購物車
            cart.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Cleared all items from cart {CartId}", cartId);
            
            return await GetCartResponseAsync(cart);
        }
    }
}