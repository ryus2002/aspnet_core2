using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Services
{
    /// <summary>
    /// 庫存預警服務實現
    /// </summary>
    public class InventoryAlertService : IInventoryAlertService
    {
        private readonly IProductDbContext _dbContext;
        private readonly IProductService _productService;
        private readonly ILogger<InventoryAlertService> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="dbContext">數據庫上下文</param>
        /// <param name="productService">產品服務</param>
        /// <param name="logger">日誌記錄器</param>
        public InventoryAlertService(
            IProductDbContext dbContext,
            IProductService productService,
            ILogger<InventoryAlertService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 檢查商品庫存並生成預警
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID (可選)</param>
        /// <returns>生成的預警</returns>
        public async Task<InventoryAlert?> CheckAndCreateAlertAsync(string productId, string? variantId = null)
        {
            try
            {
                _logger.LogInformation("檢查商品庫存: 商品ID={ProductId}, 變體ID={VariantId}", productId, variantId ?? "無");

                // 獲取商品
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("檢查庫存時找不到商品: {ProductId}", productId);
                    return null;
                }

                // 檢查是否已有未解決的預警
                var existingAlert = await GetExistingActiveAlert(productId, variantId);
                if (existingAlert != null)
                {
                    _logger.LogInformation("商品已有未解決的預警: {AlertId}", existingAlert.Id);
                    return existingAlert;
                }

                // 檢查庫存狀態
                int currentStock;
                int threshold;
                string itemName;
                
                if (!string.IsNullOrEmpty(variantId))
                {
                    // 檢查變體庫存
                    var variant = product.Variants?.FirstOrDefault(v => v.VariantId == variantId);
                    if (variant == null)
                    {
                        _logger.LogWarning("找不到商品變體: 商品ID={ProductId}, 變體ID={VariantId}", productId, variantId);
                        return null;
                    }
                    
                    currentStock = variant.Stock.Available;
                    threshold = variant.Stock.LowStockThreshold;
                    itemName = $"{product.Name} - {variant.Name}";
                }
                else
                {
                    // 檢查主商品庫存
                    currentStock = product.Stock.Available;
                    threshold = product.Stock.LowStockThreshold;
                    itemName = product.Name;
                }

                // 檢查是否需要生成預警
                if (currentStock <= 0)
                {
                    // 缺貨預警
                    return await CreateInventoryAlert(
                        productId, 
                        product.Name,
                        variantId, 
                        variantId != null ? product.Variants.First(v => v.VariantId == variantId).Name : null,
                        AlertType.OutOfStock,
                        AlertSeverity.High,
                        currentStock,
                        threshold,
                        $"{itemName} 已缺貨，目前庫存為 0。",
                        "請立即補充庫存或暫停銷售此商品。"
                    );
                }
                else if (currentStock <= threshold)
                {
                    // 低庫存預警
                    var severity = currentStock <= threshold / 2 ? AlertSeverity.Medium : AlertSeverity.Low;
                    
                    return await CreateInventoryAlert(
                        productId,
                        product.Name,
                        variantId,
                        variantId != null ? product.Variants.First(v => v.VariantId == variantId).Name : null,
                        AlertType.LowStock,
                        severity,
                        currentStock,
                        threshold,
                        $"{itemName} 庫存過低，目前庫存為 {currentStock}，低於閾值 {threshold}。",
                        "請考慮補充庫存以避免缺貨。"
                    );
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查商品庫存時發生錯誤: 商品ID={ProductId}, 變體ID={VariantId}", productId, variantId ?? "無");
                throw;
            }
        }

        /// <summary>
        /// 批量檢查所有商品庫存並生成預警
        /// </summary>
        /// <returns>生成的預警列表</returns>
        public async Task<List<InventoryAlert>> CheckAllInventoryAsync()
        {
            try
            {
                _logger.LogInformation("開始批量檢查所有商品庫存");
                var alerts = new List<InventoryAlert>();
                
                // 獲取所有商品
                var products = await _productService.GetProductsAsync(new ProductFilterRequest { PageSize = 1000 });
                
                foreach (var product in products.Items)
                {
                    // 檢查主商品
                    var alert = await CheckAndCreateAlertAsync(product.Id);
                    if (alert != null)
                    {
                        alerts.Add(alert);
                    }
                    
                    // 檢查變體
                    if (product.Variants != null)
                    {
                        foreach (var variant in product.Variants)
                        {
                            var variantAlert = await CheckAndCreateAlertAsync(product.Id, variant.VariantId);
                            if (variantAlert != null)
                            {
                                alerts.Add(variantAlert);
                            }
                        }
                    }
                }
                
                _logger.LogInformation("批量檢查完成，生成了 {Count} 個預警", alerts.Count);
                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量檢查商品庫存時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 獲取所有未解決的預警
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>分頁預警列表</returns>
        public async Task<PagedResponse<InventoryAlert>> GetActiveAlertsAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("獲取未解決的預警: 頁碼={Page}, 每頁大小={PageSize}", page, pageSize);
                
                var filter = Builders<InventoryAlert>.Filter.In(a => a.Status, new[] { AlertStatus.Created, AlertStatus.Notified });
                
                var totalItems = await _dbContext.InventoryAlerts.CountDocumentsAsync(filter);
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                
                var alerts = await _dbContext.InventoryAlerts
                    .Find(filter)
                    .Sort(Builders<InventoryAlert>.Sort.Descending(a => a.CreatedAt))
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();
                
                return new PagedResponse<InventoryAlert>
                {
                    Items = alerts,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未解決的預警時發生錯誤");
                throw;
            }
        }

        /// <summary>
        /// 獲取特定商品的預警
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="includeResolved">是否包含已解決的預警</param>
        /// <returns>預警列表</returns>
        public async Task<List<InventoryAlert>> GetAlertsByProductAsync(string productId, bool includeResolved = false)
        {
            try
            {
                _logger.LogInformation("獲取商品的預警: 商品ID={ProductId}, 包含已解決={IncludeResolved}", productId, includeResolved);
                
                var filterBuilder = Builders<InventoryAlert>.Filter;
                var filter = filterBuilder.Eq(a => a.ProductId, productId);
                
                if (!includeResolved)
                {
                    filter &= filterBuilder.In(a => a.Status, new[] { AlertStatus.Created, AlertStatus.Notified });
                }
                
                var alerts = await _dbContext.InventoryAlerts
                    .Find(filter)
                    .Sort(Builders<InventoryAlert>.Sort.Descending(a => a.CreatedAt))
                    .ToListAsync();
                
                return alerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取商品預警時發生錯誤: 商品ID={ProductId}", productId);
                throw;
            }
        }

        /// <summary>
        /// 解決預警
        /// </summary>
        /// <param name="alertId">預警ID</param>
        /// <param name="userId">處理者ID</param>
        /// <param name="notes">解決備註</param>
        /// <returns>更新後的預警</returns>
        public async Task<InventoryAlert?> ResolveAlertAsync(string alertId, string userId, string? notes = null)
        {
            try
            {
                _logger.LogInformation("解決預警: 預警ID={AlertId}, 處理者ID={UserId}", alertId, userId);
                
                var filter = Builders<InventoryAlert>.Filter.Eq(a => a.Id, alertId);
                var alert = await _dbContext.InventoryAlerts.Find(filter).FirstOrDefaultAsync();
                
                if (alert == null)
                {
                    _logger.LogWarning("找不到預警: {AlertId}", alertId);
                    return null;
                }
                
                if (alert.Status == AlertStatus.Resolved)
                {
                    _logger.LogInformation("預警已經被解決: {AlertId}", alertId);
                    return alert;
                }
                
                var update = Builders<InventoryAlert>.Update
                    .Set(a => a.Status, AlertStatus.Resolved)
                    .Set(a => a.ResolvedBy, userId)
                    .Set(a => a.ResolutionNotes, notes)
                    .Set(a => a.ResolvedAt, DateTime.UtcNow)
                    .Set(a => a.UpdatedAt, DateTime.UtcNow);
                
                await _dbContext.InventoryAlerts.UpdateOneAsync(filter, update);
                
                return await _dbContext.InventoryAlerts.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解決預警時發生錯誤: 預警ID={AlertId}", alertId);
                throw;
            }
        }

        /// <summary>
        /// 忽略預警
        /// </summary>
        /// <param name="alertId">預警ID</param>
        /// <param name="userId">處理者ID</param>
        /// <param name="notes">忽略備註</param>
        /// <returns>更新後的預警</returns>
        public async Task<InventoryAlert?> IgnoreAlertAsync(string alertId, string userId, string? notes = null)
        {
            try
            {
                _logger.LogInformation("忽略預警: 預警ID={AlertId}, 處理者ID={UserId}", alertId, userId);
                
                var filter = Builders<InventoryAlert>.Filter.Eq(a => a.Id, alertId);
                var alert = await _dbContext.InventoryAlerts.Find(filter).FirstOrDefaultAsync();
                
                if (alert == null)
                {
                    _logger.LogWarning("找不到預警: {AlertId}", alertId);
                    return null;
                }
                
                if (alert.Status == AlertStatus.Ignored || alert.Status == AlertStatus.Resolved)
                {
                    _logger.LogInformation("預警已經被處理: {AlertId}, 狀態={Status}", alertId, alert.Status);
                    return alert;
                }
                
                var update = Builders<InventoryAlert>.Update
                    .Set(a => a.Status, AlertStatus.Ignored)
                    .Set(a => a.ResolvedBy, userId)
                    .Set(a => a.ResolutionNotes, notes)
                    .Set(a => a.ResolvedAt, DateTime.UtcNow)
                    .Set(a => a.UpdatedAt, DateTime.UtcNow);
                
                await _dbContext.InventoryAlerts.UpdateOneAsync(filter, update);
                
                return await _dbContext.InventoryAlerts.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "忽略預警時發生錯誤: 預警ID={AlertId}", alertId);
                throw;
            }
        }

        /// <summary>
        /// 設定商品的低庫存閾值
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="threshold">閾值</param>
        /// <param name="variantId">變體ID (可選)</param>
        /// <returns>是否成功</returns>
        public async Task<bool> SetLowStockThresholdAsync(string productId, int threshold, string? variantId = null)
        {
            try
            {
                _logger.LogInformation("設定低庫存閾值: 商品ID={ProductId}, 變體ID={VariantId}, 閾值={Threshold}", 
                    productId, variantId ?? "無", threshold);
                
                if (threshold < 0)
                {
                    _logger.LogWarning("閾值不能為負數: {Threshold}", threshold);
                    throw new ArgumentException("閾值不能為負數", nameof(threshold));
                }
                
                // 獲取商品
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    _logger.LogWarning("找不到商品: {ProductId}", productId);
                    return false;
                }
                
                if (string.IsNullOrEmpty(variantId))
                {
                    // 更新主商品閾值
                    var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
                    var update = Builders<Product>.Update
                        .Set(p => p.Stock.LowStockThreshold, threshold)
                        .Set(p => p.UpdatedAt, DateTime.UtcNow);
                    
                    var result = await _dbContext.Products.UpdateOneAsync(filter, update);
                    return result.ModifiedCount > 0;
                }
                else
                {
                    // 更新變體閾值
                    var filter = Builders<Product>.Filter.And(
                        Builders<Product>.Filter.Eq(p => p.Id, productId),
                        Builders<Product>.Filter.ElemMatch(p => p.Variants, v => v.VariantId == variantId)
                    );
                    
                    var update = Builders<Product>.Update
                        .Set("Variants.$.Stock.LowStockThreshold", threshold)
                        .Set(p => p.UpdatedAt, DateTime.UtcNow);
                    
                    var result = await _dbContext.Products.UpdateOneAsync(filter, update);
                    return result.ModifiedCount > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定低庫存閾值時發生錯誤: 商品ID={ProductId}, 變體ID={VariantId}", 
                    productId, variantId ?? "無");
                throw;
            }
        }

        /// <summary>
        /// 獲取已存在的未解決預警
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <returns>預警或null</returns>
        private async Task<InventoryAlert?> GetExistingActiveAlert(string productId, string? variantId)
        {
            var filterBuilder = Builders<InventoryAlert>.Filter;
            var filter = filterBuilder.Eq(a => a.ProductId, productId);
            
            if (string.IsNullOrEmpty(variantId))
            {
                filter &= filterBuilder.Eq(a => a.VariantId, null);
            }
            else
            {
                filter &= filterBuilder.Eq(a => a.VariantId, variantId);
            }
            
            filter &= filterBuilder.In(a => a.Status, new[] { AlertStatus.Created, AlertStatus.Notified });
            
            return await _dbContext.InventoryAlerts.Find(filter).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 建立庫存預警
        /// </summary>
        private async Task<InventoryAlert> CreateInventoryAlert(
            string productId,
            string productName,
            string? variantId,
            string? variantName,
            AlertType alertType,
            AlertSeverity severity,
            int currentStock,
            int threshold,
            string message,
            string suggestedAction)
        {
            var alert = new InventoryAlert
            {
                ProductId = productId,
                ProductName = productName,
                VariantId = variantId,
                VariantName = variantName,
                AlertType = alertType,
                Severity = severity,
                Status = AlertStatus.Created,
                CurrentStock = currentStock,
                Threshold = threshold,
                Message = message,
                SuggestedAction = suggestedAction,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            await _dbContext.InventoryAlerts.InsertOneAsync(alert);
            _logger.LogInformation("已創建庫存預警: {AlertId}, 商品={ProductName}, 類型={AlertType}", 
                alert.Id, productName + (variantName != null ? $" - {variantName}" : ""), alertType);
            
            return alert;
        }
    }
}