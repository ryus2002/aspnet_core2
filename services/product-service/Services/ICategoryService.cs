using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    /// <summary>
    /// 分類服務接口
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// 創建分類
        /// </summary>
        /// <param name="request">創建分類請求</param>
        /// <returns>創建的分類</returns>
        Task<Category> CreateCategoryAsync(CreateCategoryRequest request);

        /// <summary>
        /// 更新分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <param name="request">更新分類請求</param>
        /// <returns>更新後的分類</returns>
        Task<Category> UpdateCategoryAsync(string id, UpdateCategoryRequest request);

        /// <summary>
        /// 獲取分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <returns>分類</returns>
        Task<Category?> GetCategoryByIdAsync(string id);

        /// <summary>
        /// 根據Slug獲取分類
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <returns>分類</returns>
        Task<Category?> GetCategoryBySlugAsync(string slug);

        /// <summary>
        /// 刪除分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteCategoryAsync(string id);

        /// <summary>
        /// 獲取所有分類
        /// </summary>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>分類列表</returns>
        Task<List<Category>> GetAllCategoriesAsync(bool includeInactive = false);

        /// <summary>
        /// 獲取子分類
        /// </summary>
        /// <param name="parentId">父分類ID</param>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>子分類列表</returns>
        Task<List<Category>> GetChildCategoriesAsync(string parentId, bool includeInactive = false);

        /// <summary>
        /// 獲取分類樹
        /// </summary>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>分類樹</returns>
        Task<List<CategoryTreeNode>> GetCategoryTreeAsync(bool includeInactive = false);
    }
}