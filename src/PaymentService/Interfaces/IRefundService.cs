public interface IRefundService
{
    /// <summary>
    /// 創建退款請求
    /// </summary>
    /// <param name="request">退款請求資料</param>
    /// <param name="userId">操作用戶ID</param>
    /// <returns>退款實體</returns>
    Task<Refund> CreateRefundAsync(CreateRefundRequest request, string userId);
    
    /// <summary>
    /// 根據ID獲取退款資訊
    /// </summary>
    /// <param name="id">退款ID</param>
    /// <returns>退款實體，如不存在則返回null</returns>
    Task<Refund?> GetRefundByIdAsync(string id);
    
    /// <summary>
    /// 根據交易ID獲取相關退款列表
    /// </summary>
    /// <param name="transactionId">支付交易ID</param>
    /// <returns>退款實體列表</returns>
    Task<IEnumerable<Refund>> GetRefundsByTransactionIdAsync(string transactionId);
    
    /// <summary>
    /// 更新退款狀態
    /// </summary>
    /// <param name="refundId">退款ID</param>
    /// <param name="newStatus">新狀態</param>
    /// <param name="reason">狀態變更原因</param>
    /// <returns>更新後的退款實體</returns>
    Task<Refund> UpdateRefundStatusAsync(string refundId, string newStatus, string reason);
    
    /// <summary>
    /// 處理退款結果
    /// </summary>
    /// <param name="id">退款ID</param>
    /// <param name="isSuccess">是否成功</param>
    /// <param name="responseData">支付提供商回應數據</param>
    /// <returns>處理後的退款實體</returns>
    Task<Refund> ProcessRefundAsync(string id, bool isSuccess, string? responseData = null);
}