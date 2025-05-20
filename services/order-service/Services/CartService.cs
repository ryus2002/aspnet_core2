using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// 購物車服務實現
    /// </summary>
    public class CartService : ICartService
    {
        private readonly OrderDbContext _dbContext;
        private readonly ILogger<CartService> _logger;

        /// <summary>
        /// 建構函數
        /// </summary>
        public CartService(OrderDbContext dbContext, ILogger<CartService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// 創建購物車
        /// </summary>
        public async Task<CartResponse> CreateCartAsync(CreateCartRequest request)
        {
            // 檢查是否已存在相同會話ID的購物車
            var existingCart = await _dbContext.Carts
                .FirstOrDefaultAsync(c => c.SessionId == request.SessionId);

            if (existingCart != null)
            {
                // 如果存在且用戶ID不同，則更新用戶ID
                if (!string.IsNullOrEmpty(request.UserId) && existingCart.UserId != request.UserId)
                {
                    existingCart.UserId = request.UserId;
                    existingCart.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }
                
                return await GetCartResponseAsync(existingCart);
            }

            // 創建新購物車
            var cart = new Cart
            {
                SessionId = request.SessionId,
                UserId = request.UserId,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30), // 30天過期
                Metadata = request.Metadata != null ? JsonSerializer.Serialize(request.Metadata) : null
            };

            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created new cart with ID {CartId} for session {SessionId}", cart.Id, cart.SessionId);
            
            return await GetCartResponseAsync(cart);
        }

        /// <summary>
        /// 根據ID獲取購物車
        /// </summary>
        public async Task<CartResponse?> GetCartByIdAsync(int id)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == "active");

            if (cart == null)
            {
                _logger.LogWarning("Cart with ID {CartId} not found or not active", id);
                return null;
            }

            return await GetCartResponseAsync(cart);
        }

        /// <summary>
        /// 根據會話ID獲取購物車
        /// </summary>
        public async Task<CartResponse?> GetCartBySessionIdAsync(string sessionId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.Status == "active");

            if (cart == null)
            {
                _logger.LogWarning("Active cart for session {SessionId} not found", sessionId);
                return null;
            }

            return await GetCartResponseAsync(cart);
        }

        /// <summary>
        /// 根據用戶ID獲取購物車
        /// </summary>
        public async Task<CartResponse?> GetCartByUserIdAsync(string userId)
        {
            var cart = await _dbContext.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "active");

            if (cart == null)
            {
                _logger.LogWarning("Active cart for user {UserId} not found", userId);
                return null;
            }

            return await GetCartResponseAsync(cart);
        }

        // 將在下一部分繼續實現
    }
}