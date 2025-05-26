using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using MongoDB.Driver;
using System.Text.Json;

namespace Shared.Monitoring
{
    /// <summary>
    /// 業務指標收集器，用於收集和發布業務相關的指標
    /// </summary>
    public class BusinessMetricsCollector : BackgroundService
    {
        private readonly IMetricsCollector _metricsCollector;
        private readonly ILogger<BusinessMetricsCollector> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _collectionInterval = TimeSpan.FromMinutes(1);
        private readonly string _serviceName;
        private IMongoDatabase? _mongoDb;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="metricsCollector">指標收集器</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="configuration">配置</param>
        /// <param name="serviceName">服務名稱</param>
        public BusinessMetricsCollector(
            IMetricsCollector metricsCollector, 
            ILogger<BusinessMetricsCollector> logger,
            IConfiguration configuration,
            string serviceName = "unknown")
        {
            _metricsCollector = metricsCollector;
            _logger = logger;
            _configuration = configuration;
            _serviceName = serviceName;

            // 初始化資料庫連接
            InitializeDatabaseConnection();
        }

        /// <summary>
        /// 初始化資料庫連接
        /// </summary>
        private void InitializeDatabaseConnection()
        {
            try
            {
                // 根據服務類型初始化不同的資料庫連接
                switch (_serviceName.ToLower())
                {
                    case "product-service":
                        InitializeMongoDb();
                        break;
                    case "order-service":
                    case "payment-service":
                    case "auth-service":
                        // 這些服務可能使用其他類型的資料庫，可以在這裡初始化
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化資料庫連接時發生錯誤");
            }
            }
        /// <summary>
        /// 初始化 MongoDB 連接
        /// </summary>
        private void InitializeMongoDb()
        {
            var mongoSettings = _configuration.GetSection("MongoDb").Get<MongoDbSettings>();
            if (mongoSettings != null)
            {
                var client = new MongoClient(mongoSettings.ConnectionString);
                _mongoDb = client.GetDatabase(mongoSettings.DatabaseName);
                _logger.LogInformation("MongoDB 連接已初始化");
            }
            else
            {
                _logger.LogWarning("無法獲取 MongoDB 設定");
        }
    }

    /// <summary>
        /// 執行背景任務，定期收集業務指標
    /// </summary>
        /// <param name="stoppingToken">取消令牌</param>
        /// <returns>任務</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
            _logger.LogInformation("業務指標收集器已啟動 - 服務: {ServiceName}", _serviceName);
            try
        {
                while (!stoppingToken.IsCancellationRequested)
        {
                    await CollectBusinessMetricsAsync();
                    await Task.Delay(_collectionInterval, stoppingToken);
        }
            }
            catch (OperationCanceledException)
        {
                _logger.LogInformation("業務指標收集器已停止");
        }
            catch (Exception ex)
        {
                _logger.LogError(ex, "業務指標收集器發生錯誤");
        }
        }

        /// <summary>
        /// 收集業務指標
        /// </summary>
        private async Task CollectBusinessMetricsAsync()
        {
            try
            {
                switch (_serviceName.ToLower())
        {
                    case "product-service":
                        await CollectProductServiceMetricsAsync();
                        break;
                    case "order-service":
                        await CollectOrderServiceMetricsAsync();
                        break;
                    case "payment-service":
                        await CollectPaymentServiceMetricsAsync();
                        break;
                    case "auth-service":
                        await CollectAuthServiceMetricsAsync();
                        break;
                    default:
                        _logger.LogWarning("未知服務類型: {ServiceName}, 無法收集特定業務指標", _serviceName);
                        break;
        }

                _logger.LogDebug("業務指標已收集 - 服務: {ServiceName}", _serviceName);
            }
            catch (Exception ex)
        {
                _logger.LogError(ex, "收集業務指標時發生錯誤");
        }
        }

        /// <summary>
        /// 收集產品服務的業務指標
        /// </summary>
        private async Task CollectProductServiceMetricsAsync()
        {
            if (_mongoDb == null)
            {
                _logger.LogWarning("MongoDB 連接未初始化，無法收集產品服務指標");
                return;
            }

            try
        {
                // 收集產品總數
                var productsCollection = _mongoDb.GetCollection<dynamic>("products");
                var totalProducts = await productsCollection.CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
                _metricsCollector.SetGauge("business.product.total", totalProducts);

                // 收集活躍產品數量
                var activeProductsFilter = Builders<dynamic>.Filter.Eq("status", "active");
                var activeProducts = await productsCollection.CountDocumentsAsync(activeProductsFilter);
                _metricsCollector.SetGauge("business.product.active", activeProducts);

                // 收集庫存不足的產品數量
                var lowStockFilter = Builders<dynamic>.Filter.Lt("stock.quantity", 10);
                var lowStockProducts = await productsCollection.CountDocumentsAsync(lowStockFilter);
                _metricsCollector.SetGauge("business.product.low_stock", lowStockProducts);

                // 收集分類數量
                var categoriesCollection = _mongoDb.GetCollection<dynamic>("categories");
                var totalCategories = await categoriesCollection.CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
                _metricsCollector.SetGauge("business.category.total", totalCategories);

                // 收集庫存變動記錄數量
                var inventoryChangesCollection = _mongoDb.GetCollection<dynamic>("inventoryChanges");
                var totalInventoryChanges = await inventoryChangesCollection.CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
                _metricsCollector.SetGauge("business.inventory.changes", totalInventoryChanges);

                // 收集預留數量
                var reservationsCollection = _mongoDb.GetCollection<dynamic>("reservations");
                var totalReservations = await reservationsCollection.CountDocumentsAsync(FilterDefinition<dynamic>.Empty);
                _metricsCollector.SetGauge("business.reservation.total", totalReservations);

                _logger.LogDebug("產品服務業務指標已收集");
        }
            catch (Exception ex)
        {
                _logger.LogError(ex, "收集產品服務業務指標時發生錯誤");
        }
    }

        /// <summary>
        /// 收集訂單服務的業務指標
        /// </summary>
        private async Task CollectOrderServiceMetricsAsync()
        {
            // 這裡可以實現訂單服務的業務指標收集邏輯
            // 由於我們沒有訂單服務的具體資料庫實現，這裡只是示例
            _logger.LogDebug("訂單服務業務指標收集功能尚未實現");
}

        /// <summary>
        /// 收集支付服務的業務指標
        /// </summary>
        private async Task CollectPaymentServiceMetricsAsync()
        {
            // 這裡可以實現支付服務的業務指標收集邏輯
            // 由於我們沒有支付服務的具體資料庫實現，這裡只是示例
            _logger.LogDebug("支付服務業務指標收集功能尚未實現");
        }

        /// <summary>
        /// 收集認證服務的業務指標
        /// </summary>
        private async Task CollectAuthServiceMetricsAsync()
        {
            // 這裡可以實現認證服務的業務指標收集邏輯
            // 由於我們沒有認證服務的具體資料庫實現，這裡只是示例
            _logger.LogDebug("認證服務業務指標收集功能尚未實現");
        }
    }

    /// <summary>
    /// MongoDB 設定類
    /// </summary>
    public class MongoDbSettings
    {
        /// <summary>
        /// 連接字串
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// 資料庫名稱
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
    }
}