using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductService.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProductService.BackgroundServices
{
    /// <summary>
    /// 庫存監控背景服務
    /// </summary>
    public class InventoryMonitoringService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InventoryMonitoringService> _logger;
        private readonly TimeSpan _checkInterval;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="serviceProvider">服務提供者</param>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="configuration">配置</param>
        public InventoryMonitoringService(
            IServiceProvider serviceProvider,
            ILogger<InventoryMonitoringService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            // 從配置中獲取檢查間隔，預設為每6小時檢查一次
            int intervalHours = configuration.GetValue<int>("InventoryMonitoring:CheckIntervalHours", 6);
            _checkInterval = TimeSpan.FromHours(intervalHours);
        }

        /// <summary>
        /// 執行背景任務
        /// </summary>
        /// <param name="stoppingToken">取消令牌</param>
        /// <returns>任務</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("庫存監控服務已啟動，檢查間隔: {Interval} 小時", _checkInterval.TotalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("開始執行庫存監控檢查");
                    
                    // 使用範圍服務提供者以確保服務的正確釋放
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var inventoryAlertService = scope.ServiceProvider.GetRequiredService<IInventoryAlertService>();
                        
                        // 執行庫存檢查
                        var alerts = await inventoryAlertService.CheckAllInventoryAsync();
                        
                        _logger.LogInformation("庫存監控檢查完成，發現 {Count} 個預警", alerts.Count);
                        
                        // 如果有預警，可以在這裡添加通知邏輯
                        if (alerts.Count > 0)
                        {
                            await NotifyAboutAlerts(alerts, scope.ServiceProvider);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "執行庫存監控檢查時發生錯誤");
                }

                // 等待下一次檢查
                await Task.Delay(_checkInterval, stoppingToken);
            }
            
            _logger.LogInformation("庫存監控服務已停止");
        }

        /// <summary>
        /// 通知有關預警
        /// </summary>
        /// <param name="alerts">預警列表</param>
        /// <param name="serviceProvider">服務提供者</param>
        /// <returns>任務</returns>
        private async Task NotifyAboutAlerts(List<Models.InventoryAlert> alerts, IServiceProvider serviceProvider)
        {
            // 這裡可以實現通知邏輯，例如發送電子郵件、推送通知等
            // 目前僅記錄日誌，實際應用中可以擴展
            
            _logger.LogWarning("發現 {Count} 個庫存預警需要處理:", alerts.Count);
            
            foreach (var alert in alerts)
            {
                string itemName = alert.ProductName;
                if (!string.IsNullOrEmpty(alert.VariantName))
                {
                    itemName += $" - {alert.VariantName}";
                }
                
                _logger.LogWarning(
                    "商品 {ItemName} ({ProductId}) 庫存預警: {AlertType}, 嚴重程度: {Severity}, 當前庫存: {CurrentStock}, 閾值: {Threshold}",
                    itemName,
                    alert.ProductId,
                    alert.AlertType,
                    alert.Severity,
                    alert.CurrentStock,
                    alert.Threshold
                );
                
                // 更新預警為已通知狀態
                try
                {
                    var dbContext = serviceProvider.GetRequiredService<ProductService.Data.IProductDbContext>();
                    var filter = MongoDB.Driver.Builders<Models.InventoryAlert>.Filter.Eq(a => a.Id, alert.Id);
                    var update = MongoDB.Driver.Builders<Models.InventoryAlert>.Update
                        .Set(a => a.Status, Models.AlertStatus.Notified)
                        .Set(a => a.NotifiedAt, DateTime.UtcNow)
                        .Set(a => a.UpdatedAt, DateTime.UtcNow);
                    
                    await dbContext.InventoryAlerts.UpdateOneAsync(filter, update);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "更新預警狀態時發生錯誤: {AlertId}", alert.Id);
                }
            }
        }
    }
}