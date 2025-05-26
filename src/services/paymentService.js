import { httpClient } from '../utils/httpInterceptor';

/**
 * 支付服務
 * 處理與支付相關的API請求
 */
class PaymentService {
  /**
   * 獲取支付方式列表
   * @returns {Promise<Array>} 支付方式列表
   */
  async getPaymentMethods() {
    try {
      const response = await httpClient.get('/api/mock-payments/methods');
    return response.data;
  } catch (error) {
      console.error('獲取支付方式失敗:', error);
      throw error;
  }
  }
/**
   * 創建支付交易
   * @param {Object} paymentData - 支付數據
   * @returns {Promise<Object>} 創建的支付交易
 */
  async createPayment(paymentData) {
  try {
      const response = await httpClient.post('/api/payments', paymentData);
    return response.data;
  } catch (error) {
      console.error('創建支付交易失敗:', error);
      throw error;
  }
  }
/**
   * 獲取支付交易詳情
   * @param {string} transactionId - 交易ID
   * @returns {Promise<Object>} 支付交易詳情
 */
  async getPayment(transactionId) {
  try {
      const response = await httpClient.get(`/api/payments/${transactionId}`);
    return response.data;
  } catch (error) {
      console.error('獲取支付交易失敗:', error);
      throw error;
  }
  }
/**
   * 獲取訂單的支付交易
   * @param {string} orderId - 訂單ID
   * @returns {Promise<Object>} 支付交易詳情
 */
  async getPaymentByOrderId(orderId) {
  try {
      const response = await httpClient.get(`/api/payments/order/${orderId}`);
    return response.data;
  } catch (error) {
      console.error('獲取訂單支付交易失敗:', error);
      throw error;
  }
  }
/**
   * 處理模擬支付
   * @param {string} transactionId - 交易ID
   * @param {boolean} shouldSucceed - 是否成功
   * @param {number} delaySeconds - 延遲秒數
   * @returns {Promise<Object>} 處理結果
 */
  async processPayment(transactionId, shouldSucceed = true, delaySeconds = 3) {
  try {
      const response = await httpClient.post(
        `/api/mock-payments/${transactionId}/process?shouldSucceed=${shouldSucceed}&delaySeconds=${delaySeconds}`
      );
    return response.data;
  } catch (error) {
      console.error('處理支付失敗:', error);
      throw error;
  }
  }
/**
   * 獲取模擬支付頁面
   * @param {string} transactionId - 交易ID
   * @returns {Promise<string>} 支付頁面HTML
 */
  async getPaymentPage(transactionId) {
  try {
      const response = await httpClient.get(
        `/api/mock-payments/${transactionId}/page`,
        { responseType: 'text' }
      );
    return response.data;
  } catch (error) {
      console.error('獲取支付頁面失敗:', error);
      throw error;
  }
  }

  /**
   * 取消支付
   * @param {string} transactionId - 交易ID
   * @param {string} reason - 取消原因
   * @returns {Promise<Object>} 取消結果
   */
  async cancelPayment(transactionId, reason) {
    try {
      const response = await httpClient.post(`/api/payments/${transactionId}/cancel`, { reason });
      return response.data;
    } catch (error) {
      console.error('取消支付失敗:', error);
      throw error;
    }
  }
}

export const paymentService = new PaymentService();
