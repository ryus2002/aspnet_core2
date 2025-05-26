/**
 * 訂單服務
 * 處理訂單相關的API請求
 */
import axios from 'axios';

// API基礎URL
const API_URL = 'https://api-gateway/orders';

/**
 * 設置請求頭部的認證令牌
 * @returns {object} 包含認證令牌的請求頭
 */
const authHeader = () => {
  const token = localStorage.getItem('token');
  return token ? { Authorization: `Bearer ${token}` } : {};
};

/**
 * 創建訂單
 * @param {object} orderData - 訂單數據
 * @returns {Promise} 創建的訂單
 */
export const createOrder = async (orderData) => {
  try {
    const response = await axios.post(API_URL, orderData, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '創建訂單失敗'
    );
  }
};

/**
 * 獲取訂單列表
 * @param {object} params - 查詢參數
 * @returns {Promise} 訂單列表
 */
export const getOrders = async (params = {}) => {
  try {
    const response = await axios.get(API_URL, {
      headers: authHeader(),
      params
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取訂單列表失敗'
    );
  }
};

/**
 * 獲取訂單詳情
 * @param {string} orderId - 訂單ID
 * @returns {Promise} 訂單詳情
 */
export const getOrderById = async (orderId) => {
  try {
    const response = await axios.get(`${API_URL}/${orderId}`, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取訂單詳情失敗'
    );
  }
};

/**
 * 取消訂單
 * @param {string} orderId - 訂單ID
 * @param {string} reason - 取消原因
 * @returns {Promise} 取消結果
 */
export const cancelOrder = async (orderId, reason) => {
  try {
    const response = await axios.post(`${API_URL}/${orderId}/cancel`, {
      reason
    }, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '取消訂單失敗'
    );
  }
};

/**
 * 獲取訂單狀態歷史
 * @param {string} orderId - 訂單ID
 * @returns {Promise} 訂單狀態歷史
 */
export const getOrderHistory = async (orderId) => {
  try {
    const response = await axios.get(`${API_URL}/${orderId}/history`, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取訂單歷史失敗'
    );
  }
};