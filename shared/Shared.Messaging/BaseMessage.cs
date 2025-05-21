using System;

namespace Shared.Messaging
{
    /// <summary>
    /// 所有消息的基類
    /// </summary>
    public abstract class BaseMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 消息創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// 消息類型
        /// </summary>
        public string MessageType { get; set; }
        
        /// <summary>
        /// 構造函數
        /// </summary>
        protected BaseMessage()
        {
            MessageType = GetType().Name;
        }
    }
}