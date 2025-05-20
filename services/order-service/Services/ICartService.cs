using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Services
{
    /// <summary>
    /// 購物車服務介面
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// 創建購物車
        /// </summary>
        /// <param name="request">創建購物車請求</param>
        /// <returns>購物車響應</returns>
        Task<CartResponse> CreateCartAsync(CreateCartRequest request);
        
        /// <summary>
        /// 根據ID獲取購物車
        /// </summary>
        /// <param name="id">購物車ID</param>
        /// <returns>購物車響應</returns>
        Task<CartResponse?> GetCartByIdAsync(int id);
        
        /// <summary>
        /// 根據會話ID獲取購物車
        /// </summary>
        /// <param name="sessionId">會話ID</param>
        /// <returns>購物車響應</returns>
        Task<CartResponse?> GetCartBySessionIdAsync(string sessionId);
        
        /// <summary>
        /// 根據用戶ID獲取購物車
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>購物車響應</returns>
        Task<CartResponse?> GetCartByUserIdAsync(string userId);
        
        /// <summary>
        /// 添加購物車項目
        /// </summary>
        /// <param name="cartId">購物車ID</param>
        /// <param name="request">添加購物車項目請求</param>
        /// <returns>購物車響應</returns>
        Task<CartResponse> AddCartItemAsync(int cartId, AddCartItemRequest request);
        
        /// <summary>
        /// 更新購物車項目
        /// </summary>
        /// <param name="cartId">購物車ID</param>
        /// <param name="itemId">購物車項目ID</param>
        /// <param name="request">更新購物車項目請求</param>
        /// <returns>購物車響應</returns>
        Task<CartResponse> UpdateCartItemAsync(int cartId, int itemId, UpdateCartItemRequest request);
        
        /// <summary>
        /// 刪除購物車項目
        /// </summary>
        /// <param name="cartId">購物車ID</param>
        /// <param name="itemId">購物車項目ID</param>
        /// <returns>購物車響應</returns>
        Task<CartResponse> RemoveCartItemAsync(int cartId, int itemId);
        
        /// <summary>
        /// 清空購物車
        /// </summary>
        /// <param name="cartId">購物車ID</param>
        /// <returns>購物車響應</returns>
        Task<CartResponse> ClearCartAsync(int cartId);
        
        /// <summary>
        /// 合併購物車
        /// </summary>
        /// <param name="sessionId">會話ID</param>
        /// <param name="userId">用戶ID</param>
        /// <returns>合併後的購物車響應</returns>
        Task<CartResponse> MergeCartsAsync(string sessionId, string userId);
        
        /// <summary>
        /// 更新購物車狀態
        /// </summary>
        /// <param name="cartId">購物車ID</param>
        /// <param name="status">新狀態</param>
        /// <returns>更新後的購物車響應</returns>
        Task<CartResponse> UpdateCartStatusAsync(int cartId, string status);
        
        /// <summary>
        /// 清理過期購物車
        /// </summary>
        /// <returns>清理的購物車數量</returns>
        Task<int> CleanupExpiredCartsAsync();
    }
}