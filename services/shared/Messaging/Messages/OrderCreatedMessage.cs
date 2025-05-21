namespace Shared.Messaging.Messages
{
    /// <summary>
    /// 訂單創建消息，當新訂單被創建時發送
    /// </summary>
    public class OrderCreatedMessage : BaseMessage
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
        /// 訂單總金額
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 訂單項目列表
        /// </summary>
        public List<OrderItemMessage> Items { get; set; } = new List<OrderItemMessage>();

        /// <summary>
        /// 訂單狀態
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// 訂單創建時間
        /// </summary>
        public DateTime OrderDate { get; set; }
    }

    /// <summary>
    /// 訂單項目消息
    /// </summary>
    public class OrderItemMessage
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ProductId { get; set; } = null!;

        /// <summary>
        /// 商品變體ID（如果有）
        /// </summary>
        public string? VariantId { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ProductName { get; set; } = null!;

        /// <summary>
        /// 商品數量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 商品單價
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 小計金額
        /// </summary>
        public decimal SubTotal { get; set; }
    }
}