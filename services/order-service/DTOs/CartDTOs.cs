using System.ComponentModel.DataAnnotations;

namespace OrderService.DTOs
{
    /// <summary>
    /// 購物車請求基類
    /// </summary>
    public class CartRequestBase
    {
        /// <summary>
        /// 會話ID (未登入用戶)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SessionId { get; set; } = null!;
    }

    /// <summary>
    /// 創建購物車請求
    /// </summary>
    public class CreateCartRequest : CartRequestBase
    {
        /// <summary>
        /// 用戶ID (已登入用戶)
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// 額外元數據
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// 添加購物車項目請求
    /// </summary>
    public class AddCartItemRequest
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        [MaxLength(36)]
        public string ProductId { get; set; } = null!;
        
        /// <summary>
        /// 商品變體ID
        /// </summary>
        [MaxLength(36)]
        public string? VariantId { get; set; }
        
        /// <summary>
        /// 數量
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// 更新購物車項目請求
    /// </summary>
    public class UpdateCartItemRequest
    {
        /// <summary>
        /// 數量
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// 購物車項目響應
    /// </summary>
    public class CartItemResponse
    {
        /// <summary>
        /// 購物車項目ID
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
        /// 數量
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// 單價
        /// </summary>
        public decimal UnitPrice { get; set; }
        
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string Name { get; set; } = null!;
        
        /// <summary>
        /// 商品屬性
        /// </summary>
        public Dictionary<string, object>? Attributes { get; set; }
        
        /// <summary>
        /// 添加時間
        /// </summary>
        public DateTime AddedAt { get; set; }
        
        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 購物車響應
    /// </summary>
    public class CartResponse
    {
        /// <summary>
        /// 購物車ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// 用戶ID
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// 會話ID
        /// </summary>
        public string SessionId { get; set; } = null!;
        
        /// <summary>
        /// 狀態
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// 購物車項目
        /// </summary>
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
        
        /// <summary>
        /// 項目總數
        /// </summary>
        public int ItemsCount => Items.Sum(i => i.Quantity);
        
        /// <summary>
        /// 總金額
        /// </summary>
        public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);
        
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// 過期時間
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// 額外元數據
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }
    }
}