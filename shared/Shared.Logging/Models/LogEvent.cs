using System.Text.Json.Serialization;

namespace Shared.Logging.Models
{
    /// <summary>
    /// 日誌事件模型
    /// </summary>
    public class LogEvent
    {
        /// <summary>
        /// 事件發生時間
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// 日誌級別
        /// </summary>
        [JsonPropertyName("level")]
        public string Level { get; set; } = "Information";

        /// <summary>
        /// 消息模板
        /// </summary>
        [JsonPropertyName("messageTemplate")]
        public string MessageTemplate { get; set; } = string.Empty;

        /// <summary>
        /// 渲染後的消息
        /// </summary>
        [JsonPropertyName("renderedMessage")]
        public string RenderedMessage { get; set; } = string.Empty;

        /// <summary>
        /// 異常信息
        /// </summary>
        [JsonPropertyName("exception")]
        public string? Exception { get; set; }

        /// <summary>
        /// 日誌屬性
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}