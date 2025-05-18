public interface IPaymentService
{
    /// <summary>
    /// 創建支付交易
    /// </summary>
    /// <param name="request">支付請求資料</param>
    /// <returns>支付交易實體</returns>
    Task<PaymentTransaction> CreatePaymentAsync(CreatePaymentRequest request);
    
    /// <summary>
    /// 根據ID獲取支付交易
    /// </summary>
    /// <param name="id">支付交易ID</param>
    /// <returns>支付交易實體，如不存在則返回null</returns>
    Task<PaymentTransaction?> GetPaymentByIdAsync(string id);
    
    /// <summary>
    /// 根據訂單ID獲取支付交易
    /// </summary>
    /// <param name="orderId">訂單ID</param>
    /// <returns>支付交易實體，如不存在則返回null</returns>
    Task<PaymentTransaction?> GetPaymentByOrderIdAsync(string orderId);
    
    /// <summary>
    /// 更新支付交易狀態
    /// </summary>
    /// <param name="id">支付交易ID</param>
    /// <param name="newStatus">新狀態</param>
    /// <param name="reason">狀態變更原因</param>
    /// <returns>更新後的支付交易實體</returns>
    Task<PaymentTransaction> UpdatePaymentStatusAsync(string id, string newStatus, string reason);
    
    /// <summary>
    /// 處理支付回調
    /// </summary>
    /// <param name="providerId">支付提供商ID</param>
    /// <param name="callbackData">回調數據</param>
    /// <returns>處理結果</returns>
    Task<PaymentCallbackResult> ProcessPaymentCallbackAsync(string providerId, Dictionary<string, string> callbackData);
    
    /// <summary>
    /// 獲取用戶支付交易歷史
    /// </summary>
    /// <param name="userId">用戶ID</param>
    /// <param name="page">頁碼</param>
    /// <param name="pageSize">每頁大小</param>
    /// <returns>支付交易列表</returns>
    Task<PagedResult<PaymentTransaction>> GetUserPaymentHistoryAsync(string userId, int page = 1, int pageSize = 20);
}