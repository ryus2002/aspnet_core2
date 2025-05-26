using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductService.Models
{
    /// <summary>
    /// 商品預留模型，對應MongoDB中的reservations集合
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// 預留唯一標識符
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        /// <summary>
        /// 擁有者ID (如用戶ID或購物車ID)
        /// </summary>
        [BsonElement("ownerId")]
        public string OwnerId { get; set; } = null!;

        /// <summary>
        /// 擁有者類型 (user, cart, order等)
        /// </summary>
        [BsonElement("ownerType")]
        public string OwnerType { get; set; } = null!;

        /// <summary>
        /// 預留項目列表
        /// </summary>
        [BsonElement("items")]
        public List<ReservationItem> Items { get; set; } = new List<ReservationItem>();

        /// <summary>
        /// 會話ID
        /// </summary>
        [BsonElement("sessionId")]
        public string SessionId { get; set; } = null!;

        /// <summary>
        /// 用戶ID (可選)
        /// </summary>
        [BsonElement("userId")]
        public string? UserId { get; set; }

        /// <summary>
        /// 預留過期時間
        /// </summary>
        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 預留狀態 (active, used, expired, cancelled)
        /// </summary>
        [BsonElement("status")]
        public string Status { get; set; } = "active";

        /// <summary>
        /// 關聯單據ID (如訂單ID)
        /// </summary>
        [BsonElement("referenceId")]
        public string? ReferenceId { get; set; }

        /// <summary>
        /// 創建時間
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 預留項目
    /// </summary>
    public class ReservationItem
    {
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
        /// 預留數量
        /// </summary>
        [BsonElement("quantity")]
        public int Quantity { get; set; }
    }
}