using ProductService.DTOs;
using ProductService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Services
{
    /// <summary>
    /// 庫存預警服務介面
    /// </summary>
    public interface IInventoryAlertService
    {
        /// <summary>
        /// 檢查商品庫存並生成預警
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID (可選)</param>
        /// <returns>生成的預警</returns>
        Task<InventoryAlert?> CheckAndCreateAlertAsync(string productId, string? variantId = null);

        /// <summary>
        /// 批量檢查所有商品庫存並生成預警
        /// </summary>
        /// <returns>生成的預警列表</returns>
        Task<List<InventoryAlert>> CheckAllInventoryAsync();

        /// <summary>
        /// 獲取所有未解決的預警
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>分頁預警列表</returns>
        Task<PagedResponse<InventoryAlert>> GetActiveAlertsAsync(int page = 1, int pageSize = 20);

        /// <summary>
        /// 獲取特定商品的預警
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="includeResolved">是否包含已解決的預警</param>
        /// <returns>預警列表</returns>
        Task<List<InventoryAlert>> GetAlertsByProductAsync(string productId, bool includeResolved = false);

        /// <summary>
        /// 解決預警
        /// </summary>
        /// <param name="alertId">預警ID</param>
        /// <param name="userId">處理者ID</param>
        /// <param name="notes">解決備註</param>
        /// <returns>更新後的預警</returns>
        Task<InventoryAlert?> ResolveAlertAsync(string alertId, string userId, string? notes = null);

        /// <summary>
        /// 忽略預警
        /// </summary>
        /// <param name="alertId">預警ID</param>
        /// <param name="userId">處理者ID</param>
        /// <param name="notes">忽略備註</param>
        /// <returns>更新後的預警</returns>
        Task<InventoryAlert?> IgnoreAlertAsync(string alertId, string userId, string? notes = null);

        /// <summary>
        /// 設定商品的低庫存閾值
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="threshold">閾值</param>
        /// <param name="variantId">變體ID (可選)</param>
        /// <returns>是否成功</returns>
        Task<bool> SetLowStockThresholdAsync(string productId, int threshold, string? variantId = null);
    }
}