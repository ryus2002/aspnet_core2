using Shared.Logging.Models;

namespace Shared.Logging.Services
{
    /// <summary>
    /// 日誌查詢服務接口
    /// </summary>
    public interface ILogQueryService
    {
        /// <summary>
        /// 獲取指定服務的日誌文件列表
        /// </summary>
        /// <param name="serviceName">服務名稱，如為 null 則獲取所有服務</param>
        /// <returns>日誌文件列表</returns>
        Task<IEnumerable<string>> GetLogFilesAsync(string? serviceName = null);

        /// <summary>
        /// 從指定文件讀取日誌事件
        /// </summary>
        /// <param name="filePath">文件路徑</param>
        /// <param name="skip">跳過的記錄數</param>
        /// <param name="take">獲取的記錄數</param>
        /// <returns>日誌事件列表</returns>
        Task<IEnumerable<LogEvent>> ReadLogEventsFromFileAsync(string filePath, int skip = 0, int take = 100);

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
        Task<IEnumerable<LogEvent>> SearchLogEventsAsync(
            string? serviceName = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null,
            string? level = null,
            string? searchText = null,
            int skip = 0,
            int take = 100);

        /// <summary>
        /// 獲取日誌統計信息
        /// </summary>
        /// <param name="serviceName">服務名稱，如為 null 則獲取所有服務</param>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">結束時間</param>
        /// <returns>日誌統計信息</returns>
        Task<LogStatistics> GetLogStatisticsAsync(
            string? serviceName = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null);
    }

    /// <summary>
    /// 日誌統計信息
    /// </summary>
    public class LogStatistics
    {
        /// <summary>
        /// 按日誌級別統計的記錄數
        /// </summary>
        public Dictionary<string, int> CountByLevel { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// 按服務名稱統計的記錄數
        /// </summary>
        public Dictionary<string, int> CountByService { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// 按小時統計的記錄數
        /// </summary>
        public Dictionary<string, int> CountByHour { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// 最常見的錯誤消息
        /// </summary>
        public List<KeyValuePair<string, int>> MostCommonErrors { get; set; } = new List<KeyValuePair<string, int>>();

        /// <summary>
        /// 最慢的請求
        /// </summary>
        public List<LogEvent> SlowestRequests { get; set; } = new List<LogEvent>();
    }
}