using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductService.Models
{
    /// <summary>
    /// 庫存變動記錄模型，對應MongoDB中的inventory_changes集合
    /// </summary>
    public class InventoryChange
    {
        /// <summary>
        /// 記錄唯一標識符
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
        /// 變體ID (如果適用)
        /// </summary>
        [BsonElement("variantId")]
        public string? VariantId { get; set; }

        /// <summary>
        /// 變動類型 (increment, decrement, adjustment)
        /// </summary>
        [BsonElement("type")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// 變動數量
        /// </summary>
        [BsonElement("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// 變動原因 (order, return, adjustment, etc.)
        /// </summary>
        [BsonElement("reason")]
        public string Reason { get; set; } = null!;

        /// <summary>
        /// 相關單據ID (如訂單ID)
        /// </summary>
        [BsonElement("referenceId")]
        public string? ReferenceId { get; set; }

        /// <summary>
        /// 變動前數量
        /// </summary>
        [BsonElement("previousQuantity")]
        public int PreviousQuantity { get; set; }

        /// <summary>
        /// 變動後數量
        /// </summary>
        [BsonElement("newQuantity")]
        public int NewQuantity { get; set; }

        /// <summary>
        /// 變動時間
        /// </summary>
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 操作用戶ID
        /// </summary>
        [BsonElement("userId")]
        public string? UserId { get; set; }
    }
}