using System;

namespace Shared.Messaging.Messages
{
    /// <summary>
    /// 訂單狀態更新消息
    /// </summary>
    public class OrderStatusChangedMessage : BaseMessage
    {
        /// <summary>
        /// 訂單ID
        /// </summary>
        public string OrderId { get; set; } = null!;

        /// <summary>
        /// 訂單編號
        /// </summary>
        public string OrderNumber { get; set; } = null!;

        /// <summary>
        /// 用戶ID
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// 舊狀態
        /// </summary>
        public string OldStatus { get; set; } = null!;

        /// <summary>
        /// 新狀態
        /// </summary>
        public string NewStatus { get; set; } = null!;

        /// <summary>
        /// 更新原因
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 狀態更新時間
        /// </summary>
        public DateTime ChangedAt { get; set; }
    }
}