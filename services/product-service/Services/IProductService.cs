using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    /// <summary>
    /// 商品服務接口
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// 創建商品
        /// </summary>
        /// <param name="request">創建商品請求</param>
        /// <returns>創建的商品</returns>
        Task<Product> CreateProductAsync(CreateProductRequest request);

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="request">更新商品請求</param>
        /// <returns>更新後的商品</returns>
        Task<Product> UpdateProductAsync(string id, UpdateProductRequest request);

        /// <summary>
        /// 獲取商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>商品</returns>
        Task<Product?> GetProductByIdAsync(string id);

        /// <summary>
        /// 刪除商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteProductAsync(string id);

        /// <summary>
        /// 分頁獲取商品列表
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <param name="categoryId">分類ID</param>
        /// <param name="status">商品狀態</param>
        /// <param name="minPrice">最低價格</param>
        /// <param name="maxPrice">最高價格</param>
        /// <param name="sortBy">排序字段</param>
        /// <param name="sortDirection">排序方向</param>
        /// <returns>分頁商品列表</returns>
        Task<PagedResponse<Product>> GetProductsAsync(
            int page = 1, 
            int pageSize = 10, 
            string? categoryId = null, 
            string? status = null, 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            string sortBy = "createdAt", 
            string sortDirection = "desc");

        /// <summary>
        /// 搜尋商品
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <param name="categoryId">分類ID</param>
        /// <param name="minPrice">最低價格</param>
        /// <param name="maxPrice">最高價格</param>
        /// <param name="sortBy">排序字段</param>
        /// <param name="sortDirection">排序方向</param>
        /// <returns>分頁商品列表</returns>
        Task<PagedResponse<Product>> SearchProductsAsync(
            string keyword, 
            int page = 1, 
            int pageSize = 10, 
            string? categoryId = null, 
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            string sortBy = "score", 
            string sortDirection = "desc");

        /// <summary>
        /// 更新商品庫存
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="request">庫存更新請求</param>
        /// <returns>更新後的商品</returns>
        Task<Product> UpdateProductStockAsync(string id, UpdateStockRequest request);

        /// <summary>
        /// 獲取商品庫存
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <returns>庫存信息</returns>
        Task<StockInfo> GetProductStockAsync(string id, string? variantId = null);
    }
}