using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    /// <summary>
    /// 庫存服務接口
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// 創建庫存變動記錄
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <param name="type">變動類型</param>
        /// <param name="quantity">變動數量</param>
        /// <param name="reason">變動原因</param>
        /// <param name="referenceId">相關單據ID</param>
        /// <param name="userId">操作用戶ID</param>
        /// <returns>庫存變動記錄</returns>
        Task<InventoryChange> CreateInventoryChangeAsync(
            string productId, 
            string? variantId, 
            string type, 
            int quantity, 
            string reason, 
            string? referenceId = null, 
            string? userId = null);

        /// <summary>
        /// 獲取商品庫存變動記錄
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>分頁庫存變動記錄</returns>
        Task<PagedResponse<InventoryChange>> GetInventoryChangesAsync(
            string productId, 
            string? variantId = null, 
            int page = 1, 
            int pageSize = 10);

        /// <summary>
        /// 創建商品預留
        /// </summary>
        /// <param name="request">預留請求</param>
        /// <returns>預留</returns>
        Task<Reservation> CreateReservationAsync(CreateReservationRequest request);

        /// <summary>
        /// 確認預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <param name="referenceId">相關單據ID</param>
        /// <returns>是否成功</returns>
        Task<bool> ConfirmReservationAsync(string id, string referenceId);

        /// <summary>
        /// 取消預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>是否成功</returns>
        Task<bool> CancelReservationAsync(string id);

        /// <summary>
        /// 清理過期預留
        /// </summary>
        /// <returns>清理的預留數量</returns>
        Task<int> CleanupExpiredReservationsAsync();

        /// <summary>
        /// 獲取會話的預留
        /// </summary>
        /// <param name="sessionId">會話ID</param>
        /// <returns>預留列表</returns>
        Task<List<Reservation>> GetReservationsBySessionAsync(string sessionId);

        /// <summary>
        /// 獲取用戶的預留
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>預留列表</returns>
        Task<List<Reservation>> GetReservationsByUserAsync(string userId);
    }
}