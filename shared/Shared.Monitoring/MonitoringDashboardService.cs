using App.Metrics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Shared.Monitoring
{
    /// <summary>
    /// 監控儀表板服務接口
    /// </summary>
    public interface IMonitoringDashboardService
    {
        /// <summary>
        /// 獲取系統指標數據
        /// </summary>
        /// <returns>系統指標數據</returns>
        Task<SystemMetricsData> GetSystemMetricsAsync();

        /// <summary>
        /// 獲取應用程式指標數據
        /// </summary>
        /// <returns>應用程式指標數據</returns>
        Task<ApplicationMetricsData> GetApplicationMetricsAsync();

        /// <summary>
        /// 獲取業務指標數據
        /// </summary>
        /// <returns>業務指標數據</returns>
        Task<BusinessMetricsData> GetBusinessMetricsAsync();
    }

    /// <summary>
    /// 監控儀表板服務實現
    /// </summary>
    public class MonitoringDashboardService : IMonitoringDashboardService
    {
        private readonly IMetrics _metrics;
        private readonly ILogger<MonitoringDashboardService> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="metrics">指標服務</param>
        /// <param name="logger">日誌記錄器</param>
        public MonitoringDashboardService(IMetrics metrics, ILogger<MonitoringDashboardService> logger)
        {
            _metrics = metrics;
            _logger = logger;
        }

        /// <summary>
        /// 獲取系統指標數據
        /// </summary>
        /// <returns>系統指標數據</returns>
        public async Task<SystemMetricsData> GetSystemMetricsAsync()
        {
            try
            {
                var snapshot = _metrics.Snapshot.Get();
                
                // 獲取 CPU 使用率
                var cpuUsage = GetGaugeValue(snapshot, "system.cpu.usage");
                
                // 獲取記憶體使用情況
                var memoryWorkingSet = GetGaugeValue(snapshot, "system.memory.working_set");
                var memoryManaged = GetGaugeValue(snapshot, "system.memory.managed");
                
                // 獲取線程數
                var threadCount = (int)GetGaugeValue(snapshot, "system.threads.count");
                
                // 獲取句柄數
                var handleCount = (int)GetGaugeValue(snapshot, "system.handles.count");
                
                // 獲取 GC 信息
                var gcPauseTime = GetGaugeValue(snapshot, "system.gc.pause_time");
                
                var gcCollectionCounts = new Dictionary<string, int>
                {
                    { "Gen0", (int)GetGaugeValue(snapshot, "system.gc.count.gen0") },
                    { "Gen1", (int)GetGaugeValue(snapshot, "system.gc.count.gen1") },
                    { "Gen2", (int)GetGaugeValue(snapshot, "system.gc.count.gen2") }
                };
                
                var gcSizes = new Dictionary<string, double>
                {
                    { "Gen0", GetGaugeValue(snapshot, "system.gc.size.gen0") },
                    { "Gen1", GetGaugeValue(snapshot, "system.gc.size.gen1") },
                    { "Gen2", GetGaugeValue(snapshot, "system.gc.size.gen2") }
                };

                return new SystemMetricsData
                {
                    CpuUsage = cpuUsage,
                    MemoryWorkingSet = memoryWorkingSet,
                    MemoryManaged = memoryManaged,
                    ThreadCount = threadCount,
                    HandleCount = handleCount,
                    GcPauseTime = gcPauseTime,
                    GcCollectionCounts = gcCollectionCounts,
                    GcSizes = gcSizes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取系統指標數據時發生錯誤");
                return new SystemMetricsData();
            }
        }

        /// <summary>
        /// 獲取應用程式指標數據
        /// </summary>
        /// <returns>應用程式指標數據</returns>
        public async Task<ApplicationMetricsData> GetApplicationMetricsAsync()
        {
            try
            {
                var snapshot = _metrics.Snapshot.Get();
                
                // 獲取請求統計
                var requestRate = GetTimerValue(snapshot, "http.request.duration", TimerValueType.Rate);
                var requestCount = GetCounterValue(snapshot, "http.requests.total");
                
                // 獲取錯誤統計
                var errorCount = GetCounterValue(snapshot, "http.requests.errors");
                var errorRate = requestCount > 0 ? (double)errorCount / requestCount * 100 : 0;
                
                // 獲取 HTTP 狀態碼統計
                var status200Count = GetCounterValue(snapshot, "http.statuscode.200");
                var status400Count = GetCounterValue(snapshot, "http.statuscode.400");
                var status401Count = GetCounterValue(snapshot, "http.statuscode.401");
                var status403Count = GetCounterValue(snapshot, "http.statuscode.403");
                var status404Count = GetCounterValue(snapshot, "http.statuscode.404");
                var status500Count = GetCounterValue(snapshot, "http.statuscode.500");
                
                // 獲取請求延遲統計
                var requestLatencyP50 = GetTimerValue(snapshot, "http.request.duration", TimerValueType.P50);
                var requestLatencyP95 = GetTimerValue(snapshot, "http.request.duration", TimerValueType.P95);
                var requestLatencyP99 = GetTimerValue(snapshot, "http.request.duration", TimerValueType.P99);

                return new ApplicationMetricsData
                {
                    RequestRate = requestRate,
                    RequestCount = requestCount,
                    ErrorCount = errorCount,
                    ErrorRate = errorRate,
                    StatusCodes = new Dictionary<string, long>
                    {
                        { "200", status200Count },
                        { "400", status400Count },
                        { "401", status401Count },
                        { "403", status403Count },
                        { "404", status404Count },
                        { "500", status500Count }
                    },
                    RequestLatency = new Dictionary<string, double>
                    {
                        { "P50", requestLatencyP50 },
                        { "P95", requestLatencyP95 },
                        { "P99", requestLatencyP99 }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取應用程式指標數據時發生錯誤");
                return new ApplicationMetricsData();
            }
        }

        /// <summary>
        /// 獲取業務指標數據
        /// </summary>
        /// <returns>業務指標數據</returns>
        public async Task<BusinessMetricsData> GetBusinessMetricsAsync()
        {
            try
            {
                var snapshot = _metrics.Snapshot.Get();
                
                // 獲取訂單相關指標
                var orderCreationCount = GetCounterValue(snapshot, "business.order.created");
                var orderCompletedCount = GetCounterValue(snapshot, "business.order.completed");
                var orderCancelledCount = GetCounterValue(snapshot, "business.order.cancelled");
                
                // 獲取支付相關指標
                var paymentSuccessCount = GetCounterValue(snapshot, "business.payment.success");
                var paymentFailureCount = GetCounterValue(snapshot, "business.payment.failure");
                
                // 計算支付成功率
                var paymentTotalCount = paymentSuccessCount + paymentFailureCount;
                var paymentSuccessRate = paymentTotalCount > 0 ? (double)paymentSuccessCount / paymentTotalCount * 100 : 0;
                
                // 獲取用戶相關指標
                var userRegistrationCount = GetCounterValue(snapshot, "business.user.registered");
                var userLoginCount = GetCounterValue(snapshot, "business.user.login");
                
                // 獲取產品相關指標
                var productViewCount = GetCounterValue(snapshot, "business.product.view");
                var productSearchCount = GetCounterValue(snapshot, "business.product.search");
                var cartAddCount = GetCounterValue(snapshot, "business.cart.add");

                return new BusinessMetricsData
                {
                    OrderMetrics = new Dictionary<string, long>
                    {
                        { "Created", orderCreationCount },
                        { "Completed", orderCompletedCount },
                        { "Cancelled", orderCancelledCount }
                    },
                    PaymentMetrics = new Dictionary<string, long>
                    {
                        { "Success", paymentSuccessCount },
                        { "Failure", paymentFailureCount }
                    },
                    PaymentSuccessRate = paymentSuccessRate,
                    UserMetrics = new Dictionary<string, long>
                    {
                        { "Registered", userRegistrationCount },
                        { "Login", userLoginCount }
                    },
                    ProductMetrics = new Dictionary<string, long>
                    {
                        { "View", productViewCount },
                        { "Search", productSearchCount },
                        { "CartAdd", cartAddCount }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取業務指標數據時發生錯誤");
                return new BusinessMetricsData();
            }
        }

        /// <summary>
        /// 從快照中獲取計數器值
        /// </summary>
        /// <param name="snapshot">指標快照</param>
        /// <param name="name">指標名稱</param>
        /// <returns>計數器值</returns>
        private long GetCounterValue(MetricsDataValueSource snapshot, string name)
        {
            var counter = snapshot.Contexts
                .SelectMany(c => c.Counters)
                .FirstOrDefault(c => c.Name == name);

            return counter?.Value ?? 0;
        }

        /// <summary>
        /// 從快照中獲取儀表值
        /// </summary>
        /// <param name="snapshot">指標快照</param>
        /// <param name="name">指標名稱</param>
        /// <returns>儀表值</returns>
        private double GetGaugeValue(MetricsDataValueSource snapshot, string name)
        {
            var gauge = snapshot.Contexts
                .SelectMany(c => c.Gauges)
                .FirstOrDefault(g => g.Name == name);

            return gauge?.Value ?? 0;
        }

        /// <summary>
        /// 計時器值類型
        /// </summary>
        private enum TimerValueType
        {
            Rate,
            P50,
            P95,
            P99
        }

        /// <summary>
        /// 從快照中獲取計時器值
        /// </summary>
        /// <param name="snapshot">指標快照</param>
        /// <param name="name">指標名稱</param>
        /// <param name="valueType">值類型</param>
        /// <returns>計時器值</returns>
        private double GetTimerValue(MetricsDataValueSource snapshot, string name, TimerValueType valueType)
        {
            var timer = snapshot.Contexts
                .SelectMany(c => c.Timers)
                .FirstOrDefault(t => t.Name == name);

            if (timer == null)
            {
                return 0;
            }

            return valueType switch
            {
                TimerValueType.Rate => timer.Rate.OneMinute,
                TimerValueType.P50 => timer.Histogram.Percentile50,
                TimerValueType.P95 => timer.Histogram.Percentile95,
                TimerValueType.P99 => timer.Histogram.Percentile99,
                _ => 0
            };
        }
    }

    /// <summary>
    /// 系統指標數據
    /// </summary>
    public class SystemMetricsData
    {
        /// <summary>
        /// CPU 使用率 (%)
        /// </summary>
        public double CpuUsage { get; set; }

        /// <summary>
        /// 工作集記憶體 (MB)
        /// </summary>
        public double MemoryWorkingSet { get; set; }

        /// <summary>
        /// 托管記憶體 (MB)
        /// </summary>
        public double MemoryManaged { get; set; }

        /// <summary>
        /// 線程數
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// 句柄數
        /// </summary>
        public int HandleCount { get; set; }

        /// <summary>
        /// GC 暫停時間百分比
        /// </summary>
        public double GcPauseTime { get; set; }

        /// <summary>
        /// GC 收集次數 (按代數)
        /// </summary>
        public Dictionary<string, int> GcCollectionCounts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// GC 大小 (按代數，MB)
        /// </summary>
        public Dictionary<string, double> GcSizes { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// 應用程式指標數據
    /// </summary>
    public class ApplicationMetricsData
    {
        /// <summary>
        /// 請求率 (每秒)
        /// </summary>
        public double RequestRate { get; set; }

        /// <summary>
        /// 請求總數
        /// </summary>
        public long RequestCount { get; set; }

        /// <summary>
        /// 錯誤總數
        /// </summary>
        public long ErrorCount { get; set; }

        /// <summary>
        /// 錯誤率 (%)
        /// </summary>
        public double ErrorRate { get; set; }

        /// <summary>
        /// HTTP 狀態碼計數
        /// </summary>
        public Dictionary<string, long> StatusCodes { get; set; } = new Dictionary<string, long>();

        /// <summary>
        /// 請求延遲 (毫秒)
        /// </summary>
        public Dictionary<string, double> RequestLatency { get; set; } = new Dictionary<string, double>();
    }

    /// <summary>
    /// 業務指標數據
    /// </summary>
    public class BusinessMetricsData
    {
        /// <summary>
        /// 訂單相關指標
        /// </summary>
        public Dictionary<string, long> OrderMetrics { get; set; } = new Dictionary<string, long>();

        /// <summary>
        /// 支付相關指標
        /// </summary>
        public Dictionary<string, long> PaymentMetrics { get; set; } = new Dictionary<string, long>();

        /// <summary>
        /// 支付成功率 (%)
        /// </summary>
        public double PaymentSuccessRate { get; set; }

        /// <summary>
        /// 用戶相關指標
        /// </summary>
        public Dictionary<string, long> UserMetrics { get; set; } = new Dictionary<string, long>();

        /// <summary>
        /// 產品相關指標
        /// </summary>
        public Dictionary<string, long> ProductMetrics { get; set; } = new Dictionary<string, long>();
    }
}