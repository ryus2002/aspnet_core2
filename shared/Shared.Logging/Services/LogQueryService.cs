using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Logging.Models;
using System.Text.Json;

namespace Shared.Logging.Services
{
    /// <summary>
    /// 日誌查詢服務實現
    /// </summary>
    public class LogQueryService : ILogQueryService
    {
        private readonly ILogger<LogQueryService> _logger;
        private readonly string _logDirectory;

        /// <summary>
        /// 初始化日誌查詢服務
        /// </summary>
        /// <param name="logger">日誌記錄器</param>
        /// <param name="configuration">配置</param>
        public LogQueryService(ILogger<LogQueryService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _logDirectory = configuration["Logging:LogDirectory"] ?? "logs";

            // 確保日誌目錄存在
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// 獲取指定服務的日誌文件列表
        /// </summary>
        /// <param name="serviceName">服務名稱，如為 null 則獲取所有服務</param>
        /// <returns>日誌文件列表</returns>
        public async Task<IEnumerable<string>> GetLogFilesAsync(string? serviceName = null)
        {
            try
            {
                // 確定要搜索的目錄
                var searchDirectory = string.IsNullOrEmpty(serviceName)
                    ? _logDirectory
                    : Path.Combine(_logDirectory, serviceName);

                // 如果目錄不存在，返回空列表
                if (!Directory.Exists(searchDirectory))
                {
                    return Enumerable.Empty<string>();
                }

                // 獲取所有 .json 文件
                var files = Directory.GetFiles(searchDirectory, "*.json", SearchOption.AllDirectories)
                    .OrderByDescending(f => new FileInfo(f).CreationTime);

                return await Task.FromResult(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取日誌文件列表時發生錯誤");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// 從指定文件讀取日誌事件
        /// </summary>
        /// <param name="filePath">文件路徑</param>
        /// <param name="skip">跳過的記錄數</param>
        /// <param name="take">獲取的記錄數</param>
        /// <returns>日誌事件列表</returns>
        public async Task<IEnumerable<LogEvent>> ReadLogEventsFromFileAsync(string filePath, int skip = 0, int take = 100)
        {
            try
            {
                // 檢查文件是否存在
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("日誌文件不存在: {FilePath}", filePath);
                    return Enumerable.Empty<LogEvent>();
                }

                // 讀取文件內容
                var logEvents = new List<LogEvent>();
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream);

                string? line;
                int lineCount = 0;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    // 跳過前面的記錄
                    if (lineCount++ < skip)
                    {
                        continue;
                    }

                    // 達到獲取數量上限時停止
                    if (logEvents.Count >= take)
                    {
                        break;
                    }

                    // 解析 JSON 日誌事件
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        try
                        {
                            var logEvent = JsonSerializer.Deserialize<LogEvent>(line);
                            if (logEvent != null)
                            {
                                logEvents.Add(logEvent);
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, "解析日誌事件時發生錯誤: {Line}", line);
                        }
                    }
                }

                return logEvents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從文件讀取日誌事件時發生錯誤: {FilePath}", filePath);
                return Enumerable.Empty<LogEvent>();
            }
        }

        /// <summary>
        /// 搜索日誌事件
        /// </summary>
        /// <param name="serviceName">服務名稱，如為 null 則搜索所有服務</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">結束時間</param>
        /// <param name="level">日誌級別</param>
        /// <param name="searchText">搜索文本</param>
        /// <param name="skip">跳過的記錄數</param>
        /// <param name="take">獲取的記錄數</param>
        /// <returns>日誌事件列表</returns>
        public async Task<IEnumerable<LogEvent>> SearchLogEventsAsync(
            string? serviceName = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null,
            string? level = null,
            string? searchText = null,
            int skip = 0,
            int take = 100)
        {
            try
            {
                // 獲取日誌文件列表
                var logFiles = await GetLogFilesAsync(serviceName);
                var allEvents = new List<LogEvent>();

                // 從每個文件讀取日誌事件
                foreach (var file in logFiles)
                {
                    // 如果已經獲取足夠的記錄，則停止
                    if (allEvents.Count >= skip + take)
                    {
                        break;
                    }

                    // 讀取文件中的所有日誌事件
                    var fileEvents = await ReadLogEventsFromFileAsync(file, 0, int.MaxValue);

                    // 應用過濾條件
                    var filteredEvents = fileEvents.Where(e =>
                        (!startTime.HasValue || e.Timestamp >= startTime.Value) &&
                        (!endTime.HasValue || e.Timestamp <= endTime.Value) &&
                        (string.IsNullOrEmpty(level) || e.Level.Equals(level, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(searchText) ||
                         e.RenderedMessage.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                         (e.Exception != null && e.Exception.Contains(searchText, StringComparison.OrdinalIgnoreCase))));

                    allEvents.AddRange(filteredEvents);
                }

                // 按時間降序排序
                allEvents = allEvents.OrderByDescending(e => e.Timestamp).ToList();

                // 應用分頁
                return allEvents.Skip(skip).Take(take);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索日誌事件時發生錯誤");
                return Enumerable.Empty<LogEvent>();
            }
        }

        /// <summary>
        /// 獲取日誌統計信息
        /// </summary>
        /// <param name="serviceName">服務名稱，如為 null 則獲取所有服務</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">結束時間</param>
        /// <returns>日誌統計信息</returns>
        public async Task<LogStatistics> GetLogStatisticsAsync(
            string? serviceName = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null)
        {
            try
            {
                // 搜索日誌事件
                var events = await SearchLogEventsAsync(
                    serviceName,
                    startTime,
                    endTime,
                    null,
                    null,
                    0,
                    10000); // 限制最大記錄數

                var statistics = new LogStatistics();
                var eventsList = events.ToList();

                // 按日誌級別統計
                statistics.CountByLevel = eventsList
                    .GroupBy(e => e.Level)
                    .ToDictionary(g => g.Key, g => g.Count());

                // 按服務名稱統計
                statistics.CountByService = eventsList
                    .Where(e => e.Properties.ContainsKey("ServiceName"))
                    .GroupBy(e => e.Properties["ServiceName"].ToString() ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count());

                // 按小時統計
                statistics.CountByHour = eventsList
                    .GroupBy(e => e.Timestamp.ToString("yyyy-MM-dd HH:00"))
                    .ToDictionary(g => g.Key, g => g.Count());

                // 最常見的錯誤
                statistics.MostCommonErrors = eventsList
                    .Where(e => e.Level == "Error" || e.Level == "Fatal")
                    .GroupBy(e => e.RenderedMessage)
                    .Select(g => new KeyValuePair<string, int>(g.Key, g.Count()))
                    .OrderByDescending(kv => kv.Value)
                    .Take(10)
                    .ToList();

                // 最慢的請求
                statistics.SlowestRequests = eventsList
                    .Where(e => e.Properties.ContainsKey("ElapsedMs"))
                    .OrderByDescending(e => 
                    {
                        if (e.Properties["ElapsedMs"] is long longValue)
                        {
                            return longValue;
                        }
                        else if (e.Properties["ElapsedMs"] is int intValue)
                        {
                            return intValue;
                        }
                        else if (e.Properties["ElapsedMs"] is string strValue && long.TryParse(strValue, out var parsedValue))
                        {
                            return parsedValue;
                        }
                        return 0;
                    })
                    .Take(10)
                    .ToList();

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取日誌統計信息時發生錯誤");
                return new LogStatistics();
            }
        }
    }
}