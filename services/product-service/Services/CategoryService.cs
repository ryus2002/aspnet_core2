using MongoDB.Bson;
using MongoDB.Driver;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    /// <summary>
    /// 分類服務實現
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IProductDbContext _dbContext;
        private readonly ILogger<CategoryService> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="dbContext">數據庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        public CategoryService(
            IProductDbContext dbContext,
            ILogger<CategoryService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 創建分類
        /// </summary>
        /// <param name="request">創建分類請求</param>
        /// <returns>創建的分類</returns>
        public async Task<Category> CreateCategoryAsync(CreateCategoryRequest request)
        {
            _logger.LogInformation("創建分類: {Name}", request.Name);

            // 驗證請求
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("分類名稱不能為空", nameof(request.Name));
            }

            if (string.IsNullOrWhiteSpace(request.Slug))
            {
                request.Slug = request.Name.ToLower().Replace(" ", "-");
            }

            // 檢查Slug是否已存在
            var slugExists = await _dbContext.Categories
                .Find(c => c.Slug == request.Slug)
                .AnyAsync();

            if (slugExists)
            {
                throw new InvalidOperationException($"Slug已存在: {request.Slug}");
            }

            // 處理父分類
            string path = "/";
            if (!string.IsNullOrWhiteSpace(request.ParentId))
            {
                var parentCategory = await _dbContext.Categories
                    .Find(c => c.Id == request.ParentId)
                    .FirstOrDefaultAsync();

                if (parentCategory == null)
                {
                    throw new KeyNotFoundException($"父分類不存在: {request.ParentId}");
                }

                path = $"{parentCategory.Path}{parentCategory.Id}/";
            }

            // 創建分類
            var category = new Category
            {
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description ?? string.Empty,
                ParentId = request.ParentId,
                Path = path,
                Status = request.Status ?? "active",
                SortOrder = request.SortOrder ?? 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // 設置分類圖片
            if (request.Image != null)
            {
                category.Image = new CategoryImage
                {
                    Url = request.Image.Url,
                    Alt = request.Image.Alt ?? string.Empty
                };
            }

            // 保存分類
            await _dbContext.Categories.InsertOneAsync(category);

            return category;
        }

        /// <summary>
        /// 更新分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <param name="request">更新分類請求</param>
        /// <returns>更新後的分類</returns>
        public async Task<Category> UpdateCategoryAsync(string id, UpdateCategoryRequest request)
        {
            _logger.LogInformation("更新分類: ID={Id}", id);

            // 獲取分類
            var category = await GetCategoryByIdAsync(id);
            if (category == null)
            {
                throw new KeyNotFoundException($"分類不存在: {id}");
            }

            // 更新基本信息
            var update = Builders<Category>.Update.Set(c => c.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                update = update.Set(c => c.Name, request.Name);
            }

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                update = update.Set(c => c.Description, request.Description);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                update = update.Set(c => c.Status, request.Status);
            }

            if (request.SortOrder.HasValue)
            {
                update = update.Set(c => c.SortOrder, request.SortOrder.Value);
            }

            // 處理Slug
            if (!string.IsNullOrWhiteSpace(request.Slug) && request.Slug != category.Slug)
            {
                // 檢查Slug是否已存在
                var slugExists = await _dbContext.Categories
                    .Find(c => c.Slug == request.Slug && c.Id != id)
                    .AnyAsync();

                if (slugExists)
                {
                    throw new InvalidOperationException($"Slug已存在: {request.Slug}");
                }

                update = update.Set(c => c.Slug, request.Slug);
            }

            // 處理父分類
            if (request.ParentId != category.ParentId)
            {
                if (string.IsNullOrWhiteSpace(request.ParentId))
                {
                    // 設為頂級分類
                    update = update.Set(c => c.ParentId, null)
                        .Set(c => c.Path, "/");
                }
                else
                {
                    // 檢查是否形成循環引用
                    if (id == request.ParentId)
                    {
                        throw new InvalidOperationException("分類不能作為自己的父分類");
                    }

                    // 檢查是否將分類設為其子分類的父分類
                    var childCategories = await GetChildCategoriesAsync(id, true);
                    if (childCategories.Any(c => c.Id == request.ParentId))
                    {
                        throw new InvalidOperationException("不能將分類設為其子分類的父分類");
                    }

                    // 獲取新的父分類
                    var parentCategory = await _dbContext.Categories
                        .Find(c => c.Id == request.ParentId)
                        .FirstOrDefaultAsync();

                    if (parentCategory == null)
                    {
                        throw new KeyNotFoundException($"父分類不存在: {request.ParentId}");
                    }

                    // 更新路徑
                    var newPath = $"{parentCategory.Path}{parentCategory.Id}/";
                    update = update.Set(c => c.ParentId, request.ParentId)
                        .Set(c => c.Path, newPath);

                    // 更新所有子分類的路徑
                    await UpdateChildCategoriesPathAsync(id, category.Path, newPath);
                }
            }

            // 處理圖片
            if (request.Image != null)
            {
                update = update.Set(c => c.Image, new CategoryImage
                {
                    Url = request.Image.Url,
                    Alt = request.Image.Alt ?? string.Empty
                });
            }

            // 更新分類
            var result = await _dbContext.Categories
                .UpdateOneAsync(c => c.Id == id, update);

            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException($"更新分類失敗: {id}");
            }

            // 返回更新後的分類
            return await GetCategoryByIdAsync(id) ?? throw new InvalidOperationException($"無法獲取更新後的分類: {id}");
        }

        /// <summary>
        /// 獲取分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <returns>分類</returns>
        public async Task<Category?> GetCategoryByIdAsync(string id)
        {
            _logger.LogInformation("獲取分類: ID={Id}", id);

            return await _dbContext.Categories
                .Find(c => c.Id == id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 根據Slug獲取分類
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <returns>分類</returns>
        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            _logger.LogInformation("根據Slug獲取分類: Slug={Slug}", slug);

            return await _dbContext.Categories
                .Find(c => c.Slug == slug)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 刪除分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteCategoryAsync(string id)
        {
            _logger.LogInformation("刪除分類: ID={Id}", id);

            // 檢查是否有子分類
            var childCategoriesCount = await _dbContext.Categories
                .CountDocumentsAsync(c => c.ParentId == id);

            if (childCategoriesCount > 0)
            {
                throw new InvalidOperationException("無法刪除有子分類的分類");
            }

            // 檢查是否有關聯的商品
            var productsCount = await _dbContext.Products
                .CountDocumentsAsync(p => p.CategoryId == id);

            if (productsCount > 0)
            {
                throw new InvalidOperationException("無法刪除有關聯商品的分類");
            }

            // 刪除分類
            var result = await _dbContext.Categories
                .DeleteOneAsync(c => c.Id == id);

            return result.DeletedCount > 0;
        }

        /// <summary>
        /// 獲取所有分類
        /// </summary>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>分類列表</returns>
        public async Task<List<Category>> GetAllCategoriesAsync(bool includeInactive = false)
        {
            _logger.LogInformation("獲取所有分類: 包含未啟用={IncludeInactive}", includeInactive);

            var filter = includeInactive
                ? Builders<Category>.Filter.Empty
                : Builders<Category>.Filter.Eq(c => c.Status, "active");

            return await _dbContext.Categories
                .Find(filter)
                .Sort(Builders<Category>.Sort.Ascending(c => c.Path).Ascending(c => c.SortOrder))
                .ToListAsync();
        }

        /// <summary>
        /// 獲取子分類
        /// </summary>
        /// <param name="parentId">父分類ID</param>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>子分類列表</returns>
        public async Task<List<Category>> GetChildCategoriesAsync(string parentId, bool includeInactive = false)
        {
            _logger.LogInformation("獲取子分類: 父分類ID={ParentId}, 包含未啟用={IncludeInactive}", parentId, includeInactive);

            var filterBuilder = Builders<Category>.Filter;
            var filter = filterBuilder.Eq(c => c.ParentId, parentId);

            if (!includeInactive)
            {
                filter = filter & filterBuilder.Eq(c => c.Status, "active");
            }

            return await _dbContext.Categories
                .Find(filter)
                .Sort(Builders<Category>.Sort.Ascending(c => c.SortOrder))
                .ToListAsync();
        }

        /// <summary>
        /// 獲取分類樹
        /// </summary>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>分類樹</returns>
        public async Task<List<CategoryTreeNode>> GetCategoryTreeAsync(bool includeInactive = false)
        {
            _logger.LogInformation("獲取分類樹: 包含未啟用={IncludeInactive}", includeInactive);

            // 獲取所有分類
            var allCategories = await GetAllCategoriesAsync(includeInactive);

            // 構建分類樹
            var categoryTree = BuildCategoryTree(allCategories);

            return categoryTree;
        }

        /// <summary>
        /// 更新子分類的路徑
        /// </summary>
        /// <param name="parentId">父分類ID</param>
        /// <param name="oldPath">舊路徑</param>
        /// <param name="newPath">新路徑</param>
        /// <returns>更新的分類數量</returns>
        private async Task<long> UpdateChildCategoriesPathAsync(string parentId, string oldPath, string newPath)
        {
            var filter = Builders<Category>.Filter.Regex(c => c.Path, $"^{oldPath}{parentId}/");
            var update = Builders<Category>.Update
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            // 使用字符串替換更新路徑
            var pathUpdate = Builders<Category>.Update
                .Set(c => c.Path, newPath + parentId + "/");

            var result = await _dbContext.Categories.UpdateManyAsync(filter, pathUpdate);
            return result.ModifiedCount;
        }

        /// <summary>
        /// 構建分類樹
        /// </summary>
        /// <param name="categories">分類列表</param>
        /// <returns>分類樹</returns>
        private List<CategoryTreeNode> BuildCategoryTree(List<Category> categories)
        {
            var rootCategories = categories.Where(c => string.IsNullOrWhiteSpace(c.ParentId)).ToList();
            var categoryDict = categories.ToDictionary(c => c.Id);
            var result = new List<CategoryTreeNode>();

            foreach (var rootCategory in rootCategories)
            {
                var node = new CategoryTreeNode
                {
                    Id = rootCategory.Id,
                    Name = rootCategory.Name,
                    Slug = rootCategory.Slug,
                    Description = rootCategory.Description,
                    Status = rootCategory.Status,
                    SortOrder = rootCategory.SortOrder,
                    Image = rootCategory.Image != null ? new ProductImageDto
                    {
                        Url = rootCategory.Image.Url,
                        Alt = rootCategory.Image.Alt
                    } : null,
                    Children = new List<CategoryTreeNode>()
                };

                AddChildCategories(node, categories, categoryDict);
                result.Add(node);
            }

            return result;
        }

        /// <summary>
        /// 添加子分類到分類樹節點
        /// </summary>
        /// <param name="parentNode">父節點</param>
        /// <param name="allCategories">所有分類</param>
        /// <param name="categoryDict">分類字典</param>
        private void AddChildCategories(CategoryTreeNode parentNode, List<Category> allCategories, Dictionary<string, Category> categoryDict)
        {
            var childCategories = allCategories.Where(c => c.ParentId == parentNode.Id).ToList();

            foreach (var childCategory in childCategories)
            {
                var childNode = new CategoryTreeNode
                {
                    Id = childCategory.Id,
                    Name = childCategory.Name,
                    Slug = childCategory.Slug,
                    Description = childCategory.Description,
                    Status = childCategory.Status,
                    SortOrder = childCategory.SortOrder,
                    Image = childCategory.Image != null ? new ProductImageDto
                    {
                        Url = childCategory.Image.Url,
                        Alt = childCategory.Image.Alt
                    } : null,
                    Children = new List<CategoryTreeNode>()
                };

                AddChildCategories(childNode, allCategories, categoryDict);
                parentNode.Children.Add(childNode);
            }
        }
    }
}