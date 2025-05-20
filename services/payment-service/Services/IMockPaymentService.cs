using System.Threading.Tasks;

namespace PaymentService.Services
{
    /// <summary>
    /// 模擬支付服務接口
    /// </summary>
    public interface IMockPaymentService
    {
        /// <summary>
        /// 模擬支付處理
        /// </summary>
        /// <param name="transactionId">交易ID</param>
        /// <param name="shouldSucceed">是否成功</param>
        /// <param name="delaySeconds">延遲秒數</param>
        /// <returns>處理結果</returns>
        Task<MockPaymentResult> ProcessPaymentAsync(string transactionId, bool shouldSucceed = true, int delaySeconds = 3);

        /// <summary>
        /// 模擬退款處理
        /// </summary>
        /// <param name="refundId">退款ID</param>
        /// <param name="shouldSucceed">是否成功</param>
        /// <param name="delaySeconds">延遲秒數</param>
        /// <returns>處理結果</returns>
        Task<MockRefundResult> ProcessRefundAsync(string refundId, bool shouldSucceed = true, int delaySeconds = 3);
    }
}