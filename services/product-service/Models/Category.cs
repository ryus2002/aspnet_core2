using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductService.Models
{
    /// <summary>
    /// 商品分類模型，對應MongoDB中的categories集合
    /// </summary>
    public class Category
    {
        /// <summary>
        /// 分類唯一標識符
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        /// <summary>
        /// 分類名稱
        /// </summary>
        [BsonElement("name")]
        [BsonRequired]
        public string Name { get; set; } = null!;

        /// <summary>
        /// 分類描述
        /// </summary>
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// URL友好的標識符
        /// </summary>
        [BsonElement("slug")]
        public string Slug { get; set; } = null!;

        /// <summary>
        /// 父分類ID (頂級分類為null)
        /// </summary>
        [BsonElement("parentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentId { get; set; }

        /// <summary>
        /// 分類層級 (0為頂級)
        /// </summary>
        [BsonElement("level")]
        public int Level { get; set; }

        /// <summary>
        /// 從頂級到當前分類的路徑
        /// </summary>
        [BsonElement("path")]
        public string Path { get; set; } = "/";

        /// <summary>
        /// 分類狀態: active, inactive
        /// </summary>
        [BsonElement("status")]
        public string Status { get; set; } = "active";

        /// <summary>
        /// 排序順序
        /// </summary>
        [BsonElement("sortOrder")]
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否啟用
        /// </summary>
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 分類圖片
        /// </summary>
        [BsonElement("image")]
        public CategoryImage? Image { get; set; }

        /// <summary>
        /// 創建時間
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新時間
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// 分類圖片
    /// </summary>
    public class CategoryImage
    {
        /// <summary>
        /// 圖片URL
        /// </summary>
        [BsonElement("url")]
        public string Url { get; set; } = null!;

        /// <summary>
        /// 替代文字
        /// </summary>
        [BsonElement("alt")]
        public string Alt { get; set; } = string.Empty;
    }
}