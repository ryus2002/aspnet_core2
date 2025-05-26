using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ProductService.Models
{
    /// <summary>
    /// 庫存預警狀態
    /// </summary>
    public enum AlertStatus
    {
        /// <summary>
        /// 已建立
        /// </summary>
        Created,

        /// <summary>
        /// 已通知
        /// </summary>
        Notified,

        /// <summary>
        /// 已解決
        /// </summary>
        Resolved,

        /// <summary>
        /// 已忽略
        /// </summary>
        Ignored
    }

    /// <summary>
    /// 庫存預警嚴重程度
    /// </summary>
    public enum AlertSeverity
    {
        /// <summary>
        /// 低
        /// </summary>
        Low,

        /// <summary>
        /// 中
        /// </summary>
        Medium,

        /// <summary>
        /// 高
        /// </summary>
        High,

        /// <summary>
        /// 緊急
        /// </summary>
        Critical
    }

    /// <summary>
    /// 庫存預警類型
    /// </summary>
    public enum AlertType
    {
        /// <summary>
        /// 低庫存
        /// </summary>
        LowStock,

        /// <summary>
        /// 缺貨
        /// </summary>
        OutOfStock,

        /// <summary>
        /// 庫存不一致
        /// </summary>
        InventoryDiscrepancy
    }

    /// <summary>
    /// 庫存預警模型
    /// </summary>
    public class InventoryAlert
    {
        /// <summary>
        /// 預警唯一標識符
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        /// <summary>
        /// 商品ID
        /// </summary>
        [BsonElement("productId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; } = null!;

        /// <summary>
        /// 商品名稱
        /// </summary>
        [BsonElement("productName")]
        public string ProductName { get; set; } = null!;

        /// <summary>
        /// 變體ID (如果適用)
        /// </summary>
        [BsonElement("variantId")]
        public string? VariantId { get; set; }

        /// <summary>
        /// 變體名稱 (如果適用)
        /// </summary>
        [BsonElement("variantName")]
        public string? VariantName { get; set; }

        /// <summary>
        /// 預警類型
        /// </summary>
        [BsonElement("alertType")]
        public AlertType AlertType { get; set; }

        /// <summary>
        /// 預警嚴重程度
        /// </summary>
        [BsonElement("severity")]
        public AlertSeverity Severity { get; set; }

        /// <summary>
        /// 預警狀態
        /// </summary>
        [BsonElement("status")]
        public AlertStatus Status { get; set; } = AlertStatus.Created;

        /// <summary>
        /// 當前庫存數量
        /// </summary>
        [BsonElement("currentStock")]
        public int CurrentStock { get; set; }

        /// <summary>
        /// 庫存閾值
        /// </summary>
        [BsonElement("threshold")]
        public int Threshold { get; set; }

        /// <summary>
        /// 預警訊息
        /// </summary>
        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 建議行動
        /// </summary>
        [BsonElement("suggestedAction")]
        public string SuggestedAction { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 通知時間
        /// </summary>
        [BsonElement("notifiedAt")]
        public DateTime? NotifiedAt { get; set; }

        /// <summary>
        /// 解決時間
        /// </summary>
        [BsonElement("resolvedAt")]
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// 處理者ID
        /// </summary>
        [BsonElement("resolvedBy")]
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// 解決備註
        /// </summary>
        [BsonElement("resolutionNotes")]
        public string? ResolutionNotes { get; set; }
    }
}