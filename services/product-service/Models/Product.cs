using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ProductService.Models
{
    /// <summary>
    /// 商品模型，對應MongoDB中的products集合
    /// </summary>
    public class Product
    {
        /// <summary>
        /// 商品唯一標識符
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        /// <summary>
        /// 商品名稱
        /// </summary>
        [BsonElement("name")]
        [BsonRequired]
        public string Name { get; set; } = null!;

        /// <summary>
        /// 商品描述
        /// </summary>
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 商品價格資訊
        /// </summary>
        [BsonElement("price")]
        public PriceInfo Price { get; set; } = new PriceInfo();

        /// <summary>
        /// 商品圖片列表
        /// </summary>
        [BsonElement("images")]
        public List<ProductImage> Images { get; set; } = new List<ProductImage>();

        /// <summary>
        /// 商品分類ID
        /// </summary>
        [BsonElement("categoryId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; } = null!;

        /// <summary>
        /// 商品屬性，使用動態字典存儲不同商品的特殊屬性
        /// </summary>
        [BsonElement("attributes")]
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 商品變體列表
        /// </summary>
        [BsonElement("variants")]
        public List<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        /// <summary>
        /// 商品庫存資訊
        /// </summary>
        [BsonElement("stock")]
        public StockInfo Stock { get; set; } = new StockInfo();

        /// <summary>
        /// 商品狀態: active, inactive, outOfStock
        /// </summary>
        [BsonElement("status")]
        public string Status { get; set; } = "active";

        /// <summary>
        /// 商品標籤
        /// </summary>
        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// 商品評分資訊
        /// </summary>
        [BsonElement("ratings")]
        public RatingInfo Ratings { get; set; } = new RatingInfo();

        /// <summary>
        /// 商品創建時間
        /// </summary>
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 商品更新時間
        /// </summary>
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 商品元數據
        /// </summary>
        [BsonElement("metadata")]
        public ProductMetadata Metadata { get; set; } = new ProductMetadata();
    }

    /// <summary>
    /// 商品價格資訊
    /// </summary>
    public class PriceInfo
    {
        /// <summary>
        /// 正常價格
        /// </summary>
        [BsonElement("regular")]
        public decimal Regular { get; set; }

        /// <summary>
        /// 折扣價格，如果沒有折扣則為null
        /// </summary>
        [BsonElement("discount")]
        public decimal? Discount { get; set; }

        /// <summary>
        /// 貨幣單位，預設為TWD
        /// </summary>
        [BsonElement("currency")]
        public string Currency { get; set; } = "TWD";
    }

    /// <summary>
    /// 商品圖片
    /// </summary>
    public class ProductImage
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

        /// <summary>
        /// 是否為主圖
        /// </summary>
        [BsonElement("isMain")]
        public bool IsMain { get; set; }
    }

    /// <summary>
    /// 商品變體
    /// </summary>
    public class ProductVariant
    {
        /// <summary>
        /// 變體ID
        /// </summary>
        [BsonElement("variantId")]
        public string VariantId { get; set; } = null!;

        /// <summary>
        /// 變體名稱
        /// </summary>
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 變體特定屬性
        /// </summary>
        [BsonElement("attributes")]
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 變體價格
        /// </summary>
        [BsonElement("price")]
        public decimal Price { get; set; }

        /// <summary>
        /// 庫存單位
        /// </summary>
        [BsonElement("sku")]
        public string Sku { get; set; } = null!;

        /// <summary>
        /// 庫存信息
        /// </summary>
        [BsonElement("stock")]
        public StockInfo Stock { get; set; } = new StockInfo();
    }

    /// <summary>
    /// 庫存資訊
    /// </summary>
    public class StockInfo
    {
        /// <summary>
        /// 庫存數量
        /// </summary>
        [BsonElement("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// 已預留數量
        /// </summary>
        [BsonElement("reserved")]
        public int Reserved { get; set; }

        /// <summary>
        /// 可用數量
        /// </summary>
        [BsonElement("available")]
        public int Available { get; set; }

        /// <summary>
        /// 低庫存閾值
        /// </summary>
        [BsonElement("lowStockThreshold")]
        public int LowStockThreshold { get; set; } = 5;
    }

    /// <summary>
    /// 評分資訊
    /// </summary>
    public class RatingInfo
    {
        /// <summary>
        /// 平均評分
        /// </summary>
        [BsonElement("average")]
        public double Average { get; set; }

        /// <summary>
        /// 評分數量
        /// </summary>
        [BsonElement("count")]
        public int Count { get; set; }
    }

    /// <summary>
    /// 商品元數據
    /// </summary>
    public class ProductMetadata
    {
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        [BsonElement("searchKeywords")]
        public List<string> SearchKeywords { get; set; } = new List<string>();

        /// <summary>
        /// SEO標題
        /// </summary>
        [BsonElement("seoTitle")]
        public string SeoTitle { get; set; } = string.Empty;

        /// <summary>
        /// SEO描述
        /// </summary>
        [BsonElement("seoDescription")]
        public string SeoDescription { get; set; } = string.Empty;
    }
}