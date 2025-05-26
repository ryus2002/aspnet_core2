using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Counter;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.Gauge;
using App.Metrics.Histogram;
using App.Metrics.Timer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Monitoring
{
    /// <summary>
    /// 監控系統擴展類，提供指標收集和監控儀表板的配置方法
    /// </summary>
    public static class MonitoringExtensions
    {
        /// <summary>
        /// 添加指標監控服務
        /// </summary>
        /// <param name="services">服務集合</param>
        /// <param name="configuration">配置</param>
        /// <param name="serviceName">服務名稱</param>
        /// <returns>服務集合</returns>
        public static IServiceCollection AddMetricsMonitoring(
            this IServiceCollection services,
            IConfiguration configuration,
            string serviceName)
        {
            // 創建指標構建器
            var metrics = AppMetrics.CreateDefaultBuilder()
                .Configuration.Configure(options =>
                {
                    options.GlobalTags.Add("service", serviceName);
                    options.DefaultContextLabel = serviceName;
                })
                .OutputMetrics.AsPrometheusPlainText()
                .OutputMetrics.AsPrometheusProtobuf()
                .Build();

            // 註冊指標服務
            services.AddMetrics(metrics);
            services.AddMetricsTrackingMiddleware();
            services.AddMetricsEndpoints();
            services.AddMetricsReportingHostedService();

            // 註冊自定義指標收集器
            services.AddSingleton<IMetricsCollector, MetricsCollector>();
            
            // 註冊監控儀表板服務
            services.AddSingleton<IMonitoringDashboardService, MonitoringDashboardService>();

            // 註冊系統資源監控服務
            services.AddHostedService<SystemMetricsCollector>();

            // 註冊業務指標收集器
            services.AddHostedService<BusinessMetricsCollector>();

            return services;
        }

        /// <summary>
        /// 使用指標監控中間件
        /// </summary>
        /// <param name="app">應用程序構建器</param>
        /// <returns>應用程序構建器</returns>
        public static IApplicationBuilder UseMetricsMonitoring(this IApplicationBuilder app)
        {
            // 使用指標追蹤中間件
            app.UseMetricsAllMiddleware();
            
            // 啟用指標端點
            app.UseMetricsAllEndpoints();

            // 使用監控儀表板
            app.UseMonitoringDashboard();

            return app;
        }
    }

    /// <summary>
    /// 指標收集器接口，定義了收集各種指標的方法
    /// </summary>
    public interface IMetricsCollector
    {
        /// <summary>
        /// 增加計數器
        /// </summary>
        /// <param name="name">指標名稱</param>
        /// <param name="value">增加值</param>
        /// <param name="tags">標籤</param>
        void IncrementCounter(string name, long value = 1, params (string Key, string Value)[] tags);

        /// <summary>
        /// 記錄時間
        /// </summary>
        /// <param name="name">指標名稱</param>
        /// <param name="milliseconds">毫秒數</param>
        /// <param name="tags">標籤</param>
        void RecordTimer(string name, long milliseconds, params (string Key, string Value)[] tags);

        /// <summary>
        /// 記錄直方圖數據
        /// </summary>
        /// <param name="name">指標名稱</param>
        /// <param name="value">值</param>
        /// <param name="tags">標籤</param>
        void RecordHistogram(string name, long value, params (string Key, string Value)[] tags);

        /// <summary>
        /// 設置儀表值
        /// </summary>
        /// <param name="name">指標名稱</param>
        /// <param name="value">值</param>
        /// <param name="tags">標籤</param>
        void SetGauge(string name, double value, params (string Key, string Value)[] tags);

        /// <summary>
        /// 測量操作執行時間
        /// </summary>
        /// <param name="name">指標名稱</param>
        /// <param name="action">要測量的操作</param>
        /// <param name="tags">標籤</param>
        void MeasureExecutionTime(string name, Action action, params (string Key, string Value)[] tags);

        /// <summary>
        /// 測量操作執行時間（異步）
        /// </summary>
        /// <typeparam name="T">返回值類型</typeparam>
        /// <param name="name">指標名稱</param>
        /// <param name="func">要測量的異步操作</param>
        /// <param name="tags">標籤</param>
        /// <returns>操作結果</returns>
        Task<T> MeasureExecutionTimeAsync<T>(string name, Func<Task<T>> func, params (string Key, string Value)[] tags);

        /// <summary>
        /// 測量操作執行時間（異步，無返回值）
        /// </summary>
        /// <param name="name">指標名稱</param>
        /// <param name="func">要測量的異步操作</param>
        /// <param name="tags">標籤</param>
        /// <returns>任務</returns>
        Task MeasureExecutionTimeAsync(string name, Func<Task> func, params (string Key, string Value)[] tags);
    }

    /// <summary>
    /// 指標收集器實現類
    /// </summary>
    public class MetricsCollector : IMetricsCollector
    {
        private readonly IMetrics _metrics;
        private readonly ILogger<MetricsCollector> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="metrics">指標服務</param>
        /// <param name="logger">日誌記錄器</param>
        public MetricsCollector(IMetrics metrics, ILogger<MetricsCollector> logger)
        {
            _metrics = metrics;
            _logger = logger;
        }

        /// <summary>
        /// 增加計數器
        /// </summary>
        public void IncrementCounter(string name, long value = 1, params (string Key, string Value)[] tags)
        {
            try
            {
                var counterOptions = new CounterOptions
                {
                    Name = name,
                    MeasurementUnit = Unit.Calls
                };

                var metricTags = ConvertToMetricTags(tags);
                _metrics.Measure.Counter.Increment(counterOptions, metricTags, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "增加計數器 {Name} 時發生錯誤", name);
            }
        }

        /// <summary>
        /// 記錄時間
        /// </summary>
        public void RecordTimer(string name, long milliseconds, params (string Key, string Value)[] tags)
        {
            try
            {
                var timerOptions = new TimerOptions
                {
                    Name = name,
                    MeasurementUnit = Unit.Requests,
                    DurationUnit = TimeUnit.Milliseconds,
                    RateUnit = TimeUnit.Seconds
                };

                var metricTags = ConvertToMetricTags(tags);
                _metrics.Measure.Timer.Time(timerOptions, milliseconds, metricTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄計時器 {Name} 時發生錯誤", name);
            }
        }

        /// <summary>
        /// 記錄直方圖數據
        /// </summary>
        public void RecordHistogram(string name, long value, params (string Key, string Value)[] tags)
        {
            try
            {
                var histogramOptions = new HistogramOptions
                {
                    Name = name,
                    MeasurementUnit = Unit.Items
                };

                var metricTags = ConvertToMetricTags(tags);
                _metrics.Measure.Histogram.Update(histogramOptions, value, metricTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "記錄直方圖 {Name} 時發生錯誤", name);
            }
        }

        /// <summary>
        /// 設置儀表值
        /// </summary>
        public void SetGauge(string name, double value, params (string Key, string Value)[] tags)
        {
            try
            {
                var gaugeOptions = new GaugeOptions
                {
                    Name = name,
                    MeasurementUnit = Unit.Items
                };

                var metricTags = ConvertToMetricTags(tags);
                _metrics.Measure.Gauge.SetValue(gaugeOptions, value, metricTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設置儀表 {Name} 時發生錯誤", name);
            }
        }

        /// <summary>
        /// 測量操作執行時間
        /// </summary>
        public void MeasureExecutionTime(string name, Action action, params (string Key, string Value)[] tags)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                stopwatch.Stop();
                RecordTimer(name, stopwatch.ElapsedMilliseconds, tags);
            }
        }

        /// <summary>
        /// 測量操作執行時間（異步，有返回值）
        /// </summary>
        public async Task<T> MeasureExecutionTimeAsync<T>(string name, Func<Task<T>> func, params (string Key, string Value)[] tags)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await func();
            }
            finally
            {
                stopwatch.Stop();
                RecordTimer(name, stopwatch.ElapsedMilliseconds, tags);
            }
        }
                    {
                return await func();
            }
            finally
            {
                stopwatch.Stop();
                RecordTimer(name, stopwatch.ElapsedMilliseconds, tags);
            }
        }

        /// <summary>
        /// 測量操作執行時間（異步，無返回值）
        /// </summary>
        public async Task MeasureExecutionTimeAsync(string name, Func<Task> func, params (string Key, string Value)[] tags)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await func();
            }
            finally
            {
                stopwatch.Stop();
                RecordTimer(name, stopwatch.ElapsedMilliseconds, tags);
            }
        }

        /// <summary>
        /// 將元組標籤轉換為指標標籤
        /// </summary>
        /// <param name="tags">元組標籤</param>
        /// <returns>指標標籤</returns>
        private MetricTags ConvertToMetricTags(params (string Key, string Value)[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                return MetricTags.Empty;
            }

            var tagNames = new string[tags.Length];
            var tagValues = new string[tags.Length];

            for (var i = 0; i < tags.Length; i++)
            {
                tagNames[i] = tags[i].Key;
                tagValues[i] = tags[i].Value;
            }

            return new MetricTags(tagNames, tagValues);
        }
    }
}
