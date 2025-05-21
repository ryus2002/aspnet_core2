using System;

namespace Shared.Messaging.Messages
{
    /// <summary>
    /// 消息基類，所有消息都應該繼承此類
    /// </summary>
    public abstract class BaseMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 消息創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 消息發送者
        /// </summary>
        public string Sender { get; set; } = "unknown";

        /// <summary>
        /// 消息類型
        /// </summary>
        public string MessageType => GetType().Name;
    }
}