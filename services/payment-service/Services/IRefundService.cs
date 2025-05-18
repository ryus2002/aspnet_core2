using System.Collections.Generic;
using System.Threading.Tasks;
using PaymentService.Models;
using PaymentService.Models.DTOs;

namespace PaymentService.Services
{
        /// <summary>
    /// 退款服務接口
        /// </summary>
    public interface IRefundService
    {
        /// <summary>
        /// 創建退款請求
        /// </summary>
        /// <param name="request">退款請求</param>
        /// <param name="userId">用戶ID</param>
        /// <returns>退款記錄</returns>
        Task<Refund> CreateRefundRequestAsync(CreateRefundRequest request, string userId);
        
        /// <summary>
        /// 根據ID獲取退款
        /// </summary>
        /// <param name="id">退款ID</param>
        /// <returns>退款記錄</returns>
        Task<Refund?> GetRefundByIdAsync(string id);
        
        /// <summary>
        /// 獲取交易的所有退款
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <returns>退款列表</returns>
        Task<IEnumerable<Refund>> GetRefundsByTransactionIdAsync(string transactionId);
        
        /// <summary>
        /// 處理退款
        /// </summary>
        /// <param name="id">退款ID</param>
        /// <param name="isSuccess">是否成功</param>
        /// <param name="responseData">響應數據</param>
        /// <returns>更新後的退款記錄</returns>
        Task<Refund> ProcessRefundAsync(string id, bool isSuccess, string? responseData = null);
    }
}