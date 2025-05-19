using System.ComponentModel.DataAnnotations;
using ProductService.Models;

namespace ProductService.DTOs
{
    /// <summary>
    /// 創建商品請求
    /// </summary>
    public class CreateProductRequest
    {
        /// <summary>
        /// 商品名稱
        /// </summary>
        [Required(ErrorMessage = "商品名稱為必填")]
        [StringLength(100, ErrorMessage = "商品名稱不能超過100個字符")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// 商品描述
        /// </summary>
        [StringLength(2000, ErrorMessage = "商品描述不能超過2000個字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 正常價格
        /// </summary>
        [Required(ErrorMessage = "商品價格為必填")]
        [Range(0, 9999999, ErrorMessage = "商品價格必須大於或等於0")]
        public decimal Price { get; set; }

        /// <summary>
        /// 折扣價格
        /// </summary>
        [Range(0, 9999999, ErrorMessage = "折扣價格必須大於或等於0")]
        public decimal? DiscountPrice { get; set; }

        /// <summary>
        /// 貨幣單位
        /// </summary>
        [StringLength(3, ErrorMessage = "貨幣單位不能超過3個字符")]
        public string? Currency { get; set; }

        /// <summary>
        /// 商品圖片
        /// </summary>
        public List<ProductImageDto>? Images { get; set; }

        /// <summary>
        /// 商品分類ID
        /// </summary>
        [Required(ErrorMessage = "商品分類為必填")]
        public string CategoryId { get; set; } = null!;

        /// <summary>
        /// 商品屬性
        /// </summary>
        public Dictionary<string, object>? Attributes { get; set; }

        /// <summary>
        /// 商品變體
        /// </summary>
        public List<ProductVariantDto>? Variants { get; set; }

        /// <summary>
        /// 庫存數量
        /// </summary>
        [Required(ErrorMessage = "庫存數量為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "庫存數量必須大於或等於0")]
        public int StockQuantity { get; set; }

        /// <summary>
        /// 低庫存閾值
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "低庫存閾值必須大於或等於0")]
        public int? LowStockThreshold { get; set; }

        /// <summary>
        /// 商品標籤
        /// </summary>
        public List<string>? Tags { get; set; }

        /// <summary>
        /// 商品狀態
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 商品元數據
        /// </summary>
        public ProductMetadataDto? Metadata { get; set; }
    }

        /// <summary>
    /// 更新商品請求
    /// </summary>
    public class UpdateProductRequest
    {
        /// <summary>
        /// 商品名稱
        /// </summary>
        [StringLength(100, ErrorMessage = "商品名稱不能超過100個字符")]
        public string? Name { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        [StringLength(2000, ErrorMessage = "商品描述不能超過2000個字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 正常價格
        /// </summary>
        [Range(0, 9999999, ErrorMessage = "商品價格必須大於或等於0")]
        public decimal? Price { get; set; }

        /// <summary>
        /// 折扣價格
        /// </summary>
        [Range(0, 9999999, ErrorMessage = "折扣價格必須大於或等於0")]
        public decimal? DiscountPrice { get; set; }

        /// <summary>
        /// 貨幣單位
        /// </summary>
        [StringLength(3, ErrorMessage = "貨幣單位不能超過3個字符")]
        public string? Currency { get; set; }

        /// <summary>
        /// 商品圖片
        /// </summary>
        public List<ProductImageDto>? Images { get; set; }

        /// <summary>
        /// 商品分類ID
        /// </summary>
        public string? CategoryId { get; set; }

        /// <summary>
        /// 商品屬性
        /// </summary>
        public Dictionary<string, object>? Attributes { get; set; }

        /// <summary>
        /// 商品變體
        /// </summary>
        public List<ProductVariantDto>? Variants { get; set; }

        /// <summary>
        /// 商品狀態
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 商品標籤
        /// </summary>
        public List<string>? Tags { get; set; }

        /// <summary>
        /// 商品元數據
        /// </summary>
        public ProductMetadataDto? Metadata { get; set; }
    }

        /// <summary>
    /// 商品元數據DTO
        /// </summary>
    public class ProductMetadataDto
    {
        /// <summary>
        /// 搜尋關鍵字
        /// </summary>
        public List<string>? SearchKeywords { get; set; }
        /// <summary>
        /// SEO標題
        /// </summary>
        public string? SeoTitle { get; set; }

        /// <summary>
        /// SEO描述
        /// </summary>
        public string? SeoDescription { get; set; }
    }

    /// <summary>
    /// 商品圖片DTO
    /// </summary>
    public class ProductImageDto
    {
        /// <summary>
        /// 圖片URL
        /// </summary>
        [Required(ErrorMessage = "圖片URL為必填")]
        public string Url { get; set; } = null!;

        /// <summary>
        /// 替代文字
        /// </summary>
        public string? Alt { get; set; }
        /// <summary>
        /// 是否為主圖
        /// </summary>
        public bool IsMain { get; set; }
    }

        /// <summary>
    /// 商品變體DTO
        /// </summary>
    public class ProductVariantDto
    {
        /// <summary>
        /// 變體ID
        /// </summary>
        public string? VariantId { get; set; }

        /// <summary>
        /// 變體特定屬性
        /// </summary>
        public Dictionary<string, object>? Attributes { get; set; } = new Dictionary<string, object>();
        /// <summary>
        /// 變體價格
        /// </summary>
        [Required(ErrorMessage = "變體價格為必填")]
        [Range(0, 9999999, ErrorMessage = "變體價格必須大於或等於0")]
        public decimal Price { get; set; }

        /// <summary>
        /// 庫存單位
        /// </summary>
        public string? Sku { get; set; }

        /// <summary>
        /// 庫存數量
        /// </summary>
        [Required(ErrorMessage = "庫存數量為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "庫存數量必須大於或等於0")]
        public int Stock { get; set; }
    }

    /// <summary>
    /// 商品庫存更新請求
    /// </summary>
    public class UpdateStockRequest
    {
        /// <summary>
        /// 庫存數量
        /// </summary>
        [Required(ErrorMessage = "庫存數量為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "庫存數量必須大於或等於0")]
        public int Quantity { get; set; }

        /// <summary>
        /// 變體ID (如果適用)
        /// </summary>
        public string? VariantId { get; set; }

        /// <summary>
        /// 變動原因
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 相關單據ID (如訂單ID)
        /// </summary>
        public string? ReferenceId { get; set; }

        /// <summary>
        /// 操作用戶ID
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// 低庫存閾值
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "低庫存閾值必須大於或等於0")]
        public int? LowStockThreshold { get; set; }
    }

    /// <summary>
    /// 商品預留請求
    /// </summary>
    public class CreateReservationRequest
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        [Required(ErrorMessage = "商品ID為必填")]
        public string ProductId { get; set; } = null!;

        /// <summary>
        /// 變體ID (如果適用)
        /// </summary>
        public string? VariantId { get; set; }

        /// <summary>
        /// 預留數量
        /// </summary>
        [Required(ErrorMessage = "預留數量為必填")]
        [Range(1, int.MaxValue, ErrorMessage = "預留數量必須大於0")]
        public int Quantity { get; set; }

        /// <summary>
        /// 會話ID
        /// </summary>
        [Required(ErrorMessage = "會話ID為必填")]
        public string SessionId { get; set; } = null!;

        /// <summary>
        /// 用戶ID (可選)
        /// </summary>
        public string? UserId { get; set; }
        /// <summary>
        /// 預留過期分鐘數 (預設15分鐘)
        /// </summary>
        [Range(1, 1440, ErrorMessage = "預留過期時間必須在1-1440分鐘之間")]
        public int? ExpirationMinutes { get; set; } = 15;
    }
        /// <summary>
    /// 分頁響應
        /// </summary>
    /// <typeparam name="T">數據類型</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// 當前頁碼
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 總記錄數
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPrevious => Page > 1;

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNext => Page < TotalPages;

        /// <summary>
        /// 數據
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();
    }
}