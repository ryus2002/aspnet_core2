using System.Text.Json.Serialization;

namespace Shared.Messaging.Messages
{
    /// <summary>
    /// 所有消息的基類，定義共同的屬性
    /// </summary>
    public abstract class BaseMessage
    {
        /// <summary>
        /// 消息唯一識別符
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 消息創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 消息類型
        /// </summary>
        [JsonIgnore]
        public string MessageType => GetType().Name;

        /// <summary>
        /// 消息發送者
        /// </summary>
        public string Sender { get; set; } = "unknown";

        /// <summary>
        /// 消息版本
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// 相關ID，用於關聯請求和事件
        /// </summary>
        public string? CorrelationId { get; set; }
    }
}