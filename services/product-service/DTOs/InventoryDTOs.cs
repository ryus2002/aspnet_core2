namespace ProductService.DTOs
{
    /// <summary>
    /// 庫存回滾請求
    /// </summary>
    public class RollbackInventoryRequest
    {
        /// <summary>
        /// 訂單ID
        /// </summary>
        public string OrderId { get; set; } = null!;
        
        /// <summary>
        /// 回滾項目
        /// </summary>
        public List<RollbackInventoryItem> Items { get; set; } = new List<RollbackInventoryItem>();
    }
    
    /// <summary>
    /// 庫存回滾項目
    /// </summary>
    public class RollbackInventoryItem
    {
        /// <summary>
        /// 產品ID
        /// </summary>
        public string ProductId { get; set; } = null!;
        
        /// <summary>
        /// 變體ID
        /// </summary>
        public string? VariantId { get; set; }
        
        /// <summary>
        /// 數量
        /// </summary>
        public int Quantity { get; set; }
    }
    
    /// <summary>
    /// 確認預留請求
    /// </summary>
    public class ConfirmReservationRequest
    {
        /// <summary>
        /// 參考ID
        /// </summary>
        public string ReferenceId { get; set; } = null!;
    }
    
    /// <summary>
    /// 預留響應
    /// </summary>
    public class ReservationResponse
    {
        /// <summary>
        /// 預留ID
        /// </summary>
        public string Id { get; set; } = null!;
        
        /// <summary>
        /// 擁有者ID
        /// </summary>
        public string OwnerId { get; set; } = null!;
        
        /// <summary>
        /// 擁有者類型
        /// </summary>
        public string OwnerType { get; set; } = null!;
        
        /// <summary>
        /// 預留狀態
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// 預留項目
        /// </summary>
        public List<ReservationItemResponse>? Items { get; set; }
        
        /// <summary>
        /// 過期時間
        /// </summary>
        public DateTime ExpiresAt { get; set; }
        
        /// <summary>
        /// 創建時間
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
    
    /// <summary>
    /// 預留項目響應
    /// </summary>
    public class ReservationItemResponse
    {
        /// <summary>
        /// 產品ID
        /// </summary>
        public string ProductId { get; set; } = null!;
        
        /// <summary>
        /// 變體ID
        /// </summary>
        public string? VariantId { get; set; }
        
        /// <summary>
        /// 數量
        /// </summary>
        public int Quantity { get; set; }
    }
}