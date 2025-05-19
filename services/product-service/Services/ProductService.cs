using MongoDB.Bson;
using MongoDB.Driver;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Services
{
    /// <summary>
    /// 商品服務實現
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductDbContext _dbContext;
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ProductService> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="dbContext">數據庫上下文</param>
        /// <param name="inventoryService">庫存服務</param>
        /// <param name="logger">日誌記錄器</param>
        public ProductService(
            IProductDbContext dbContext,
            IInventoryService inventoryService,
            ILogger<ProductService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 創建商品
        /// </summary>
        /// <param name="request">創建商品請求</param>
        /// <returns>創建的商品</returns>
        public async Task<Product> CreateProductAsync(CreateProductRequest request)
        {
            _logger.LogInformation("創建商品: {Name}", request.Name);

            // 驗證請求
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("商品名稱不能為空", nameof(request.Name));
            }

            if (string.IsNullOrWhiteSpace(request.CategoryId))
            {
                throw new ArgumentException("分類ID不能為空", nameof(request.CategoryId));
            }
            // 檢查分類是否存在
            var categoryExists = await _dbContext.Categories
                .Find(c => c.Id == request.CategoryId)
                .AnyAsync();

            if (!categoryExists)
            {
                throw new ArgumentException($"分類不存在: {request.CategoryId}", nameof(request.CategoryId));
            }

            // 創建商品
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Price = new PriceInfo
                {
                    Regular = request.Price,
                    Discount = request.DiscountPrice,
                    Currency = request.Currency ?? "TWD"
                },
                CategoryId = request.CategoryId,
                Status = request.Status ?? "active",
                Tags = request.Tags ?? new List<string>(),
                Stock = new StockInfo
                {
                    Quantity = request.StockQuantity,
                    Reserved = 0,
                    Available = request.StockQuantity,
                    LowStockThreshold = request.LowStockThreshold ?? 5
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            // 設置商品屬性
            if (request.Attributes != null)
            {
                product.Attributes = request.Attributes;
            }

            // 設置商品圖片
            if (request.Images != null && request.Images.Any())
            {
                product.Images = request.Images.Select(i => new ProductImage
                {
                    Url = i.Url,
                    Alt = i.Alt ?? string.Empty,
                    IsMain = i.IsMain
                }).ToList();
            }

            // 設置商品變體
            if (request.Variants != null && request.Variants.Any())
            {
                product.Variants = request.Variants.Select(v => new ProductVariant
                {
                    VariantId = v.VariantId ?? ObjectId.GenerateNewId().ToString(),
                    Attributes = v.Attributes ?? new Dictionary<string, object>(),
                    Price = v.Price,
                    Sku = v.Sku ?? $"{request.Name.Substring(0, Math.Min(3, request.Name.Length))}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    Stock = v.Stock
                }).ToList();
            }

            // 設置商品元數據
            if (request.Metadata != null)
            {
                product.Metadata = new ProductMetadata
                {
                    SearchKeywords = request.Metadata.SearchKeywords ?? new List<string>(),
                    SeoTitle = request.Metadata.SeoTitle ?? request.Name,
                    SeoDescription = request.Metadata.SeoDescription ?? request.Description ?? string.Empty
                };
            }
            // 保存商品
            await _dbContext.Products.InsertOneAsync(product);
            // 創建庫存變動記錄
            if (request.StockQuantity > 0)
            {
                await _inventoryService.CreateInventoryChangeAsync(
                    product.Id,
                    null,
                    "increment",
                    request.StockQuantity,
                    "initial",
                    null,
                    null);
            }
            return product;
        }
            
        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="request">更新商品請求</param>
        /// <returns>更新後的商品</returns>
        public async Task<Product> UpdateProductAsync(string id, UpdateProductRequest request)
        {
            _logger.LogInformation("更新商品: ID={Id}", id);
            // 獲取商品
            var product = await GetProductByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"商品不存在: {id}");
            }

            // 更新基本信息
            var update = Builders<Product>.Update.Set(p => p.UpdatedAt, DateTime.UtcNow);
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                update = update.Set(p => p.Name, request.Name);
            }

            if (request.Description != null)
            {
                update = update.Set(p => p.Description, request.Description);
            }

            if (request.Price.HasValue)
            {
                update = update.Set(p => p.Price.Regular, request.Price.Value);
            }

            if (request.DiscountPrice.HasValue)
            {
                update = update.Set(p => p.Price.Discount, request.DiscountPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Currency))
            {
                update = update.Set(p => p.Price.Currency, request.Currency);
            }

            if (!string.IsNullOrWhiteSpace(request.CategoryId))
            {
                // 檢查分類是否存在
                var categoryExists = await _dbContext.Categories
                    .Find(c => c.Id == request.CategoryId)
                    .AnyAsync();

                if (!categoryExists)
                {
                    throw new ArgumentException($"分類不存在: {request.CategoryId}", nameof(request.CategoryId));
                }

                update = update.Set(p => p.CategoryId, request.CategoryId);
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                update = update.Set(p => p.Status, request.Status);
            }

            if (request.Tags != null)
            {
                update = update.Set(p => p.Tags, request.Tags);
            }

            if (request.Attributes != null)
            {
                update = update.Set(p => p.Attributes, request.Attributes);
            }

            if (request.Images != null)
            {
                var images = request.Images.Select(i => new ProductImage
                {
                    Url = i.Url,
                    Alt = i.Alt ?? string.Empty,
                    IsMain = i.IsMain
                }).ToList();

                update = update.Set(p => p.Images, images);
            }

            if (request.Variants != null)
            {
                var variants = request.Variants.Select(v => new ProductVariant
                {
                    VariantId = v.VariantId ?? ObjectId.GenerateNewId().ToString(),
                    Attributes = v.Attributes ?? new Dictionary<string, object>(),
                    Price = v.Price,
                    Sku = v.Sku ?? $"{product.Name.Substring(0, Math.Min(3, product.Name.Length))}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    Stock = v.Stock
                }).ToList();

                update = update.Set(p => p.Variants, variants);
            }

            if (request.Metadata != null)
            {
                var metadata = new ProductMetadata
                {
                    SearchKeywords = request.Metadata.SearchKeywords ?? product.Metadata?.SearchKeywords ?? new List<string>(),
                    SeoTitle = request.Metadata.SeoTitle ?? product.Metadata?.SeoTitle ?? product.Name,
                    SeoDescription = request.Metadata.SeoDescription ?? product.Metadata?.SeoDescription ?? product.Description
                };

                update = update.Set(p => p.Metadata, metadata);
            }

            // 更新商品
            var result = await _dbContext.Products
                .UpdateOneAsync(p => p.Id == id, update);

            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException($"更新商品失敗: {id}");
            }

            // 返回更新後的商品
            return await GetProductByIdAsync(id) ?? throw new InvalidOperationException($"無法獲取更新後的商品: {id}");
        }

        /// <summary>
        /// 獲取商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>商品</returns>
        public async Task<Product?> GetProductByIdAsync(string id)
        {
            _logger.LogInformation("獲取商品: ID={Id}", id);

            return await _dbContext.Products
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 刪除商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> DeleteProductAsync(string id)
        {
            _logger.LogInformation("刪除商品: ID={Id}", id);

            var result = await _dbContext.Products
                .DeleteOneAsync(p => p.Id == id);

            return result.DeletedCount > 0;
        }

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
        public async Task<PagedResponse<Product>> GetProductsAsync(
            int page = 1,
            int pageSize = 10,
            string? categoryId = null,
            string? status = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sortBy = "createdAt",
            string sortDirection = "desc")
        {
            _logger.LogInformation("獲取商品列表: 頁碼={Page}, 每頁大小={PageSize}", page, pageSize);

            // 構建過濾條件
            var filterBuilder = Builders<Product>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                filter = filter & filterBuilder.Eq(p => p.CategoryId, categoryId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                filter = filter & filterBuilder.Eq(p => p.Status, status);
            }

            if (minPrice.HasValue)
            {
                filter = filter & filterBuilder.Gte(p => p.Price.Regular, minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                filter = filter & filterBuilder.Lte(p => p.Price.Regular, maxPrice.Value);
            }

            // 構建排序
            var sort = sortDirection.ToLower() == "asc"
                ? Builders<Product>.Sort.Ascending(sortBy)
                : Builders<Product>.Sort.Descending(sortBy);

            // 計算總數
            var totalItems = await _dbContext.Products
                .CountDocumentsAsync(filter);

            // 獲取數據
            var products = await _dbContext.Products
                .Find(filter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // 計算總頁數
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 返回分頁結果
            return new PagedResponse<Product>
            {
                Items = products,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

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
        public async Task<PagedResponse<Product>> SearchProductsAsync(
            string keyword,
            int page = 1,
            int pageSize = 10,
            string? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sortBy = "score",
            string sortDirection = "desc")
        {
            _logger.LogInformation("搜尋商品: 關鍵字={Keyword}, 頁碼={Page}, 每頁大小={PageSize}", keyword, page, pageSize);

            // 構建文本搜尋
            var textSearch = Builders<Product>.Filter.Text(keyword);

            // 構建其他過濾條件
            var filterBuilder = Builders<Product>.Filter;
            var filter = textSearch;

            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                filter = filter & filterBuilder.Eq(p => p.CategoryId, categoryId);
            }

            if (minPrice.HasValue)
            {
                filter = filter & filterBuilder.Gte(p => p.Price.Regular, minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                filter = filter & filterBuilder.Lte(p => p.Price.Regular, maxPrice.Value);
            }

            // 構建排序
            SortDefinition<Product> sort;
            if (sortBy.ToLower() == "score")
            {
                // 按相關性排序
                sort = sortDirection.ToLower() == "asc"
                    ? Builders<Product>.Sort.MetaTextScore("score")
                    : Builders<Product>.Sort.MetaTextScore("score").Descending(p => p.CreatedAt);
            }
            else
            {
                // 按其他字段排序
                sort = sortDirection.ToLower() == "asc"
                    ? Builders<Product>.Sort.Ascending(sortBy)
                    : Builders<Product>.Sort.Descending(sortBy);
            }

            // 計算總數
            var totalItems = await _dbContext.Products
                .CountDocumentsAsync(filter);

            // 獲取數據
            var products = await _dbContext.Products
                .Find(filter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // 計算總頁數
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 返回分頁結果
            return new PagedResponse<Product>
            {
                Items = products,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// 更新商品庫存
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="request">庫存更新請求</param>
        /// <returns>更新後的商品</returns>
        public async Task<Product> UpdateProductStockAsync(string id, UpdateStockRequest request)
        {
            _logger.LogInformation("更新商品庫存: ID={Id}, 數量={Quantity}", id, request.Quantity);

            // 獲取商品
            var product = await GetProductByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"商品不存在: {id}");
            }

            // 驗證請求
            if (request.Quantity < 0)
            {
                throw new ArgumentException("庫存數量不能為負數", nameof(request.Quantity));
            }

            int previousQuantity;
            string type;
            string reason = request.Reason ?? "manual";

            if (string.IsNullOrWhiteSpace(request.VariantId))
            {
                // 更新主商品庫存
                previousQuantity = product.Stock.Quantity;
                
                // 確定庫存變動類型
                if (request.Quantity > previousQuantity)
                {
                    type = "increment";
                }
                else if (request.Quantity < previousQuantity)
                {
                    type = "decrement";
                }
                else
                {
                    // 庫存沒有變化
                    return product;
                }

                // 計算可用庫存
                int available = request.Quantity - product.Stock.Reserved;
                if (available < 0)
                {
                    throw new InvalidOperationException($"新庫存數量不能小於已預留數量: 預留={product.Stock.Reserved}, 請求={request.Quantity}");
                }

                // 更新庫存
                var update = Builders<Product>.Update
                    .Set(p => p.Stock.Quantity, request.Quantity)
                    .Set(p => p.Stock.Available, available)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                if (request.LowStockThreshold.HasValue)
                {
                    update = update.Set(p => p.Stock.LowStockThreshold, request.LowStockThreshold.Value);
                }

                var result = await _dbContext.Products
                    .UpdateOneAsync(p => p.Id == id, update);

                if (result.ModifiedCount == 0)
                {
                    throw new InvalidOperationException($"更新商品庫存失敗: {id}");
                }
            }
            else
            {
                // 更新變體庫存
                var variant = product.Variants?.FirstOrDefault(v => v.VariantId == request.VariantId);
                if (variant == null)
                {
                    throw new KeyNotFoundException($"商品變體不存在: {request.VariantId}");
                }

                previousQuantity = variant.Stock;
                
                // 確定庫存變動類型
                if (request.Quantity > previousQuantity)
                {
                    type = "increment";
                }
                else if (request.Quantity < previousQuantity)
                {
                    type = "decrement";
                }
                else
                {
                    // 庫存沒有變化
                    return product;
                }

                // 更新變體庫存
                var update = Builders<Product>.Update
                    .Set(p => p.Variants[-1].Stock, request.Quantity)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                var result = await _dbContext.Products
                    .UpdateOneAsync(
                        p => p.Id == id && p.Variants.Any(v => v.VariantId == request.VariantId),
                        update,
                        new UpdateOptions { ArrayFilters = new List<ArrayFilterDefinition<BsonValue>> { new BsonDocument("elem.variantId", request.VariantId) } });

                if (result.ModifiedCount == 0)
                {
                    throw new InvalidOperationException($"更新商品變體庫存失敗: {id}, 變體ID: {request.VariantId}");
                }
            }

            // 創建庫存變動記錄
            await _inventoryService.CreateInventoryChangeAsync(
                id,
                request.VariantId,
                type,
                Math.Abs(request.Quantity - previousQuantity),
                reason,
                null,
                request.UserId);

            // 返回更新後的商品
            return await GetProductByIdAsync(id) ?? throw new InvalidOperationException($"無法獲取更新後的商品: {id}");
        }

        /// <summary>
        /// 獲取商品庫存
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <returns>庫存信息</returns>
        public async Task<StockInfo> GetProductStockAsync(string id, string? variantId = null)
        {
            _logger.LogInformation("獲取商品庫存: ID={Id}, 變體ID={VariantId}", id, variantId ?? "無");

            if (string.IsNullOrWhiteSpace(variantId))
            {
                // 獲取主商品庫存
                var product = await _dbContext.Products
                    .Find(p => p.Id == id)
                    .Project(p => p.Stock)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    throw new KeyNotFoundException($"商品不存在: {id}");
                }

                return product;
            }
            else
            {
                // 獲取變體庫存
                var product = await _dbContext.Products
                    .Find(p => p.Id == id && p.Variants.Any(v => v.VariantId == variantId))
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    throw new KeyNotFoundException($"商品不存在: {id}");
                }

                var variant = product.Variants?.FirstOrDefault(v => v.VariantId == variantId);
                if (variant == null)
                {
                    throw new KeyNotFoundException($"商品變體不存在: {variantId}");
                }

                // 變體只有庫存數量，沒有預留和可用數量的概念
                return new StockInfo
                {
                    Quantity = variant.Stock,
                    Reserved = 0,
                    Available = variant.Stock,
                    LowStockThreshold = product.Stock.LowStockThreshold
                };
            }
        }
    }
}