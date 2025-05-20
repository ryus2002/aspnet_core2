using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs
{
    /// <summary>
    /// 創建訂單請求
    /// </summary>
    public class CreateOrderRequest
    {
        /// <summary>
        /// 購物車ID或會話ID (二選一必填)
        /// </summary>
        public int? CartId { get; set; }
        
        /// <summary>
        /// 會話ID (與CartId二選一)
        /// </summary>
        public string? SessionId { get; set; }
        
        /// <summary>
        /// 配送地址ID (可選，如果已有地址)
        /// </summary>
        public int? ShippingAddressId { get; set; }
        
        /// <summary>
        /// 帳單地址ID (可選，如果已有地址)
        /// </summary>
        public int? BillingAddressId { get; set; }
        
        /// <summary>
        /// 配送地址 (如果沒有現有地址ID)
        /// </summary>
        public AddressDTO? ShippingAddress { get; set; }
        
        /// <summary>
        /// 帳單地址 (如果沒有現有地址ID)
        /// </summary>
        public AddressDTO? BillingAddress { get; set; }
        
        /// <summary>
        /// 配送方式
        /// </summary>
        [MaxLength(50)]
        public string? ShippingMethod { get; set; }
        
        /// <summary>
        /// 訂單備註
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// 額外元數據
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
    
    /// <summary>
    /// 地址DTO
    /// </summary>
    public class AddressDTO
    {
        /// <summary>
        /// 收件人/帳單人姓名
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// 電話
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = null!;
        
        /// <summary>
        /// 地址行1
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string AddressLine1 { get; set; } = null!;
        
        /// <summary>
        /// 地址行2
        /// </summary>
        [MaxLength(255)]
        public string? AddressLine2 { get; set; }
        
        /// <summary>
        /// 城市
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string City { get; set; } = null!;
        
        /// <summary>
        /// 州/省
        /// </summary>
        [MaxLength(100)]
        public string? State { get; set; }
        
        /// <summary>
        /// 郵政編碼
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = null!;
        
        /// <summary>
        /// 國家
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Country { get; set; } = null!;
        
        /// <summary>
        /// 是否保存為默認地址
        /// </summary>
        public bool SaveAsDefault { get; set; } = false;
        
        /// <summary>
        /// 地址類型: shipping, billing, both
        /// </summary>
        [MaxLength(20)]
        public string AddressType { get; set; } = "both";
    }
    
    /// <summary>
    /// 更新訂單狀態請求
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// 新狀態
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// 狀態變更說明
        /// </summary>
        public string? Comment { get; set; }
    }
    
    /// <summary>
    /// 取消訂單請求
    /// </summary>
    public class CancelOrderRequest
    {
        /// <summary>
        /// 取消原因
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Reason { get; set; } = null!;
    }
    
    /// <summary>
    /// 訂單項目響應
    /// </summary>
    public class OrderItemResponse
    {
        /// <summary>
        /// 訂單項目ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ProductId { get; set; } = null!;
        
        /// <summary>
        /// 商品變體ID
        /// </summary>
        public string? VariantId { get; set; }
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// 數量
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// 單價
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// 總價
        /// </summary>
        public decimal TotalPrice { get; set; }
        
        /// <summary>
        /// 商品屬性
        /// </summary>
        public Dictionary<string, object>? Attributes { get; set; }
        
        /// <summary>
        /// 庫存單位
        /// </summary>
        public string? SKU { get; set; }
        
        /// <summary>
        /// 商品圖片URL
        /// </summary>
        public string? ImageUrl { get; set; }
    }
    
    /// <summary>
    /// 訂單狀態歷史響應
    /// </summary>
    public class OrderStatusHistoryResponse
    {
        /// <summary>
        /// 歷史記錄ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 訂單狀態
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// 狀態變更說明
        /// </summary>
        public string? Comment { get; set; }
        
        /// <summary>
        /// 操作人ID
        /// </summary>
        public string? ChangedBy { get; set; }
        
        /// <summary>
        /// 變更時間
        /// </summary>
        public DateTime ChangedAt { get; set; }
    }
    
    /// <summary>
    /// 訂單響應
    /// </summary>
    public class OrderResponse
    {
        /// <summary>
        /// 訂單ID
        /// </summary>
        public string Id { get; set; } = null!;
        
        /// <summary>
        /// 訂單編號
        /// </summary>
        public string OrderNumber { get; set; } = null!;
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// 訂單狀態
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// 訂單總金額
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// 訂單項目數量
        /// </summary>
        public int ItemsCount { get; set; }
        
        /// <summary>
        /// 配送地址
        /// </summary>
        public AddressDTO? ShippingAddress { get; set; }
        
        /// <summary>
        /// 帳單地址
        /// </summary>
        public AddressDTO? BillingAddress { get; set; }
        
        /// <summary>
        /// 支付ID
        /// </summary>
        public string? PaymentId { get; set; }
        
        /// <summary>
        /// 配送方式
        /// </summary>
        public string? ShippingMethod { get; set; }
        
        /// <summary>
        /// 配送費用
        /// </summary>
        public decimal ShippingFee { get; set; }
        
        /// <summary>
        /// 稅額
        /// </summary>
        public decimal TaxAmount { get; set; }
        
        /// <summary>
        /// 折扣金額
        /// </summary>
        public decimal DiscountAmount { get; set; }
        
        /// <summary>
        /// 訂單備註
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// 訂單完成時間
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// 訂單取消時間
        /// </summary>
        public DateTime? CancelledAt { get; set; }
        
        /// <summary>
        /// 取消原因
        /// </summary>
        public string? CancellationReason { get; set; }
        
        /// <summary>
        /// 訂單項目
        /// </summary>
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
        
        /// <summary>
        /// 訂單狀態歷史
        /// </summary>
        public List<OrderStatusHistoryResponse>? StatusHistory { get; set; }
        
        /// <summary>
        /// 額外元數據
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
    
    /// <summary>
    /// 分頁響應
    /// </summary>
    /// <typeparam name="T">數據類型</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// 數據
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();
        
        /// <summary>
        /// 總記錄數
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// 頁碼
        /// </summary>
        public int Page { get; set; }
        
        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; }
        
        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        
        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPrevious => Page > 1;
        
        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNext => Page < TotalPages;
    }
}