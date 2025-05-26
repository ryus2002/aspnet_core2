using MongoDB.Driver;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Messaging.Publishers;
using MongoDB.Bson;
using Microsoft.Extensions.DependencyInjection;

namespace ProductService.Services
{
    /// <summary>
    /// 庫存服務實現
    /// </summary>
    public partial class InventoryService : IInventoryService
    {
        private readonly IProductDbContext _dbContext;
        private readonly ILogger<InventoryService> _logger;
        private readonly IServiceProvider _serviceProvider; // 替換為服務提供者
        private readonly InventoryEventPublisher _inventoryEventPublisher;

        // 延遲解析的產品服務
        private IProductService? _productService;
        protected IProductService ProductService => 
            _productService ??= _serviceProvider.GetRequiredService<IProductService>();

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="dbContext">數據庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="serviceProvider">服務提供者</param>
        /// <param name="inventoryEventPublisher">庫存事件發布器</param>
        public InventoryService(
            IProductDbContext dbContext,
            ILogger<InventoryService> logger,
            IServiceProvider serviceProvider, // 修改為注入服務提供者
            InventoryEventPublisher inventoryEventPublisher)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _inventoryEventPublisher = inventoryEventPublisher ?? throw new ArgumentNullException(nameof(inventoryEventPublisher));
        }

        /// <summary>
        /// 獲取商品庫存變動記錄
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>分頁庫存變動記錄</returns>
        public async Task<PagedResponse<InventoryChange>> GetInventoryChangesAsync(
            string productId,
            string? variantId = null,
            int page = 1,
            int pageSize = 10)
            {
            _logger.LogInformation("獲取庫存變動記錄: 商品ID={ProductId}, 變體ID={VariantId}, 頁碼={Page}, 每頁大小={PageSize}",
                productId, variantId ?? "無", page, pageSize);
            // 構建過濾條件
            var filterBuilder = Builders<InventoryChange>.Filter;
            var filter = filterBuilder.Eq(i => i.ProductId, productId);

            if (!string.IsNullOrWhiteSpace(variantId))
            {
                filter = filter & filterBuilder.Eq(i => i.VariantId, variantId);
            }

            // 計算總數
            var totalItems = await _dbContext.InventoryChanges
                .CountDocumentsAsync(filter);

            // 獲取數據
            var inventoryChanges = await _dbContext.InventoryChanges
                .Find(filter)
                .Sort(Builders<InventoryChange>.Sort.Descending(i => i.Timestamp))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // 計算總頁數
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 返回分頁結果
            return new PagedResponse<InventoryChange>
            {
                Items = inventoryChanges,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }
    }
}