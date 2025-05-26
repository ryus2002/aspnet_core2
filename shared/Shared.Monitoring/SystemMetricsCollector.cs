using App.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Monitoring
{
    /// <summary>
    /// 系統資源指標收集器，用於收集 CPU、記憶體等系統資源指標
    /// </summary>
    public class SystemMetricsCollector : BackgroundService
    {
        private readonly IMetricsCollector _metricsCollector;
        private readonly ILogger<SystemMetricsCollector> _logger;
        private readonly Process _currentProcess;
        private readonly TimeSpan _collectionInterval = TimeSpan.FromSeconds(15);
        
        // 上次 CPU 時間測量
        private DateTime _lastCpuTime;
        private TimeSpan _lastTotalProcessorTime;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="metricsCollector">指標收集器</param>
        /// <param name="logger">日誌記錄器</param>
        public SystemMetricsCollector(IMetricsCollector metricsCollector, ILogger<SystemMetricsCollector> logger)
        {
            _metricsCollector = metricsCollector;
            _logger = logger;
            _currentProcess = Process.GetCurrentProcess();
            _lastCpuTime = DateTime.UtcNow;
            _lastTotalProcessorTime = _currentProcess.TotalProcessorTime;
        }

        /// <summary>
        /// 執行背景任務，定期收集系統資源指標
        /// </summary>
        /// <param name="stoppingToken">取消令牌</param>
        /// <returns>任務</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("系統資源指標收集器已啟動");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    CollectSystemMetrics();
                    await Task.Delay(_collectionInterval, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("系統資源指標收集器已停止");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "系統資源指標收集器發生錯誤");
            }
        }

        /// <summary>
        /// 收集系統資源指標
        /// </summary>
        private void CollectSystemMetrics()
        {
            try
            {
                // 更新進程信息
                _currentProcess.Refresh();

                // 收集 CPU 使用率
                CollectCpuUsage();

                // 收集記憶體使用情況
                CollectMemoryUsage();

                // 收集線程數
                CollectThreadCount();

                // 收集句柄數
                CollectHandleCount();

                // 收集 GC 信息
                CollectGCInfo();

                _logger.LogDebug("系統資源指標已收集");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "收集系統資源指標時發生錯誤");
            }
        }

        /// <summary>
        /// 收集 CPU 使用率
        /// </summary>
        private void CollectCpuUsage()
        {
            var currentTime = DateTime.UtcNow;
            var currentTotalProcessorTime = _currentProcess.TotalProcessorTime;

            var cpuElapsedTime = currentTime - _lastCpuTime;
            var cpuUsedTime = currentTotalProcessorTime - _lastTotalProcessorTime;

            var cpuUsagePercentage = cpuUsedTime.TotalMilliseconds / 
                (Environment.ProcessorCount * cpuElapsedTime.TotalMilliseconds) * 100;

            _metricsCollector.SetGauge("system.cpu.usage", Math.Round(cpuUsagePercentage, 2));

            _lastCpuTime = currentTime;
            _lastTotalProcessorTime = currentTotalProcessorTime;
        }

        /// <summary>
        /// 收集記憶體使用情況
        /// </summary>
        private void CollectMemoryUsage()
        {
            // 物理記憶體 (MB)
            var workingSetMB = _currentProcess.WorkingSet64 / 1024.0 / 1024.0;
            _metricsCollector.SetGauge("system.memory.working_set", Math.Round(workingSetMB, 2));

            // 私有記憶體 (MB)
            var privateMemoryMB = _currentProcess.PrivateMemorySize64 / 1024.0 / 1024.0;
            _metricsCollector.SetGauge("system.memory.private", Math.Round(privateMemoryMB, 2));

            // 虛擬記憶體 (MB)
            var virtualMemoryMB = _currentProcess.VirtualMemorySize64 / 1024.0 / 1024.0;
            _metricsCollector.SetGauge("system.memory.virtual", Math.Round(virtualMemoryMB, 2));

            // 分頁記憶體 (MB)
            var pagedMemoryMB = _currentProcess.PagedMemorySize64 / 1024.0 / 1024.0;
            _metricsCollector.SetGauge("system.memory.paged", Math.Round(pagedMemoryMB, 2));

            // 分頁系統記憶體 (MB)
            var pagedSystemMemoryMB = _currentProcess.PagedSystemMemorySize64 / 1024.0 / 1024.0;
            _metricsCollector.SetGauge("system.memory.paged_system", Math.Round(pagedSystemMemoryMB, 2));

            // 非分頁系統記憶體 (MB)
            var nonpagedSystemMemoryMB = _currentProcess.NonpagedSystemMemorySize64 / 1024.0 / 1024.0;
            _metricsCollector.SetGauge("system.memory.nonpaged_system", Math.Round(nonpagedSystemMemoryMB, 2));

            // 托管記憶體 (MB)
            var managedMemoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            _metricsCollector.SetGauge("system.memory.managed", Math.Round(managedMemoryMB, 2));
        }

        /// <summary>
        /// 收集線程數
        /// </summary>
        private void CollectThreadCount()
        {
            _metricsCollector.SetGauge("system.threads.count", _currentProcess.Threads.Count);
        }

        /// <summary>
        /// 收集句柄數
        /// </summary>
        private void CollectHandleCount()
        {
            _metricsCollector.SetGauge("system.handles.count", _currentProcess.HandleCount);
        }

        /// <summary>
        /// 收集 GC 信息
        /// </summary>
        private void CollectGCInfo()
        {
            // GC 代數大小
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                _metricsCollector.SetGauge($"system.gc.size.gen{i}", GC.GetGCMemoryInfo().GenerationSizes[i] / 1024.0 / 1024.0);
            }

            // GC 次數
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                _metricsCollector.SetGauge($"system.gc.count.gen{i}", GC.CollectionCount(i));
            }

            // GC 暫停時間
            _metricsCollector.SetGauge("system.gc.pause_time", GC.GetGCMemoryInfo().PauseTimePercentage);
        }
    }
}