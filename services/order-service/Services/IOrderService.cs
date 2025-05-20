using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Services
{
    /// <summary>
    /// 訂單服務介面
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="request">創建訂單請求</param>
        /// <returns>訂單響應</returns>
        Task<OrderResponse> CreateOrderAsync(string userId, CreateOrderRequest request);
        
        /// <summary>
        /// 根據ID獲取訂單
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <returns>訂單響應</returns>
        Task<OrderResponse?> GetOrderByIdAsync(string id);
        
        /// <summary>
        /// 根據訂單編號獲取訂單
        /// </summary>
        /// <param name="orderNumber">訂單編號</param>
        /// <returns>訂單響應</returns>
        Task<OrderResponse?> GetOrderByNumberAsync(string orderNumber);
        
        /// <summary>
        /// 獲取用戶的訂單列表
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="status">訂單狀態</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>分頁訂單響應</returns>
        Task<PagedResponse<OrderResponse>> GetUserOrdersAsync(
            string userId, 
            string? status = null, 
            int page = 1, 
            int pageSize = 10);
        
        /// <summary>
        /// 更新訂單狀態
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <param name="request">更新訂單狀態請求</param>
        /// <param name="userId">操作用戶ID</param>
        /// <returns>更新後的訂單響應</returns>
        Task<OrderResponse> UpdateOrderStatusAsync(
            string id, 
            UpdateOrderStatusRequest request, 
            string? userId = null);
        
        /// <summary>
        /// 取消訂單
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <param name="request">取消訂單請求</param>
        /// <param name="userId">操作用戶ID</param>
        /// <returns>取消後的訂單響應</returns>
        Task<OrderResponse> CancelOrderAsync(
            string id, 
            CancelOrderRequest request, 
            string? userId = null);
        
        /// <summary>
        /// 更新訂單支付信息
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <param name="paymentId">支付ID</param>
        /// <returns>更新後的訂單響應</returns>
        Task<OrderResponse> UpdateOrderPaymentAsync(string id, string paymentId);
        
        /// <summary>
        /// 獲取訂單狀態歷史
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <returns>訂單狀態歷史列表</returns>
        Task<List<OrderStatusHistoryResponse>> GetOrderStatusHistoryAsync(string id);
        
        /// <summary>
        /// 生成訂單編號
        /// </summary>
        /// <returns>訂單編號</returns>
        Task<string> GenerateOrderNumberAsync();
    }
}