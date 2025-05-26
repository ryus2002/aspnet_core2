/**
 * 購物車服務
 * 處理購物車相關的API請求
 */
import axios from 'axios';

// API基礎URL
const API_URL = 'https://api-gateway/cart';

/**
 * 設置請求頭部的認證令牌
 * @returns {object} 包含認證令牌的請求頭
 */
const authHeader = () => {
  const token = localStorage.getItem('token');
  return token ? { Authorization: `Bearer ${token}` } : {};
};

/**
 * 獲取購物車商品
 * @returns {Promise} 購物車商品列表
 */
export const getCartItems = async () => {
  try {
    const response = await axios.get(API_URL, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取購物車失敗'
    );
  }
};

/**
 * 添加商品到購物車
 * @param {object} item - 商品信息
 * @param {string} item.productId - 商品ID
 * @param {number} item.quantity - 數量
 * @param {string} item.variantId - 變體ID (可選)
 * @returns {Promise} 添加結果
 */
export const addToCart = async (item) => {
  try {
    const response = await axios.post(API_URL, item, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '添加商品到購物車失敗'
    );
  }
};

/**
 * 更新購物車商品數量
 * @param {string} itemId - 購物車項目ID
 * @param {number} quantity - 新數量
 * @returns {Promise} 更新結果
 */
export const updateCartItem = async (itemId, quantity) => {
  try {
    const response = await axios.put(`${API_URL}/${itemId}`, {
      quantity
    }, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '更新購物車商品失敗'
    );
  }
};

/**
 * 從購物車移除商品
 * @param {string} itemId - 購物車項目ID
 * @returns {Promise} 移除結果
 */
export const removeCartItem = async (itemId) => {
  try {
    const response = await axios.delete(`${API_URL}/${itemId}`, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '從購物車移除商品失敗'
    );
  }
};

/**
 * 清空購物車
 * @returns {Promise} 清空結果
 */
export const clearCart = async () => {
  try {
    const response = await axios.delete(API_URL, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '清空購物車失敗'
    );
  }
};

/**
 * 應用優惠碼
 * @param {string} code - 優惠碼
 * @returns {Promise} 應用結果
 */
export const applyPromoCode = async (code) => {
  try {
    const response = await axios.post(`${API_URL}/promo`, {
      code
    }, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '應用優惠碼失敗'
    );
  }
};