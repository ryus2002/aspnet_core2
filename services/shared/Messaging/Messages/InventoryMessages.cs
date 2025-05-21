namespace Shared.Messaging.Messages
{
    /// <summary>
    /// 庫存更新消息，當商品庫存發生變化時發送
    /// </summary>
    public class InventoryUpdatedMessage : BaseMessage
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
        /// 新的庫存數量
        /// </summary>
        public int NewQuantity { get; set; }

        /// <summary>
        /// 變化數量（正數表示增加，負數表示減少）
        /// </summary>
        public int QuantityChange { get; set; }

        /// <summary>
        /// 庫存變化原因
        /// </summary>
        public string Reason { get; set; } = null!;

        /// <summary>
        /// 相關單據ID（如訂單ID）
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// 操作用戶ID
        /// </summary>
        public string? UserId { get; set; }
    }

    /// <summary>
    /// 庫存不足消息，當庫存不足以滿足訂單時發送
    /// </summary>
    public class InventoryLowMessage : BaseMessage
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
        /// 當前庫存數量
        /// </summary>
        public int CurrentQuantity { get; set; }

        /// <summary>
        /// 庫存閾值
        /// </summary>
        public int Threshold { get; set; }

        /// <summary>
        /// 是否完全缺貨
        /// </summary>
        public bool IsOutOfStock => CurrentQuantity <= 0;
    }

    /// <summary>
    /// 庫存預留消息，當庫存被預留時發送
    /// </summary>
    public class InventoryReservedMessage : BaseMessage
    {
        /// <summary>
        /// 預留ID
        /// </summary>
        public string ReservationId { get; set; } = null!;

        /// <summary>
        /// 會話ID或用戶ID
        /// </summary>
        public string OwnerId { get; set; } = null!;

        /// <summary>
        /// 預留類型（Session或User）
        /// </summary>
        public string OwnerType { get; set; } = null!;

        /// <summary>
        /// 預留項目列表
        /// </summary>
        public List<ReservationItemMessage> Items { get; set; } = new List<ReservationItemMessage>();

        /// <summary>
        /// 預留過期時間
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// 預留項目消息
    /// </summary>
    public class ReservationItemMessage
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
        /// 預留數量
        /// </summary>
        public int Quantity { get; set; }
    }
}