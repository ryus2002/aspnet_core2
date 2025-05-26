/**
 * 商品服務
 * 處理商品列表、詳情、搜尋等相關API請求
 */
import axios from 'axios';

// API基礎URL
const API_URL = 'https://api-gateway/products';

/**
 * 設置請求頭部的認證令牌
 * @returns {object} 包含認證令牌的請求頭
 */
const authHeader = () => {
  const token = localStorage.getItem('token');
  return token ? { Authorization: `Bearer ${token}` } : {};
};

/**
 * 獲取商品列表
 * @param {object} params - 查詢參數
 * @param {number} params.page - 頁碼
 * @param {number} params.pageSize - 每頁數量
 * @param {string} params.categoryId - 分類ID
 * @param {number} params.minPrice - 最低價格
 * @param {number} params.maxPrice - 最高價格
 * @param {string} params.sortBy - 排序欄位
 * @param {string} params.sortDirection - 排序方向
 * @returns {Promise} 商品列表及分頁信息
 */
export const getProducts = async (params = {}) => {
  try {
    const response = await axios.get(API_URL, { params });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取商品列表失敗'
    );
  }
};

/**
 * 搜尋商品
 * @param {string} keyword - 搜尋關鍵字
 * @param {object} params - 其他查詢參數
 * @returns {Promise} 搜尋結果及分頁信息
 */
export const searchProducts = async (keyword, params = {}) => {
  try {
    const response = await axios.get(`${API_URL}/search`, { 
      params: { 
        keyword,
        ...params
      } 
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '搜尋商品失敗'
    );
  }
};

/**
 * 獲取商品詳情
 * @param {string} id - 商品ID
 * @returns {Promise} 商品詳細信息
 */
export const getProductById = async (id) => {
  try {
    const response = await axios.get(`${API_URL}/${id}`);
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取商品詳情失敗'
    );
  }
};

/**
 * 獲取商品庫存
 * @param {string} id - 商品ID
 * @param {string} variantId - 變體ID
 * @returns {Promise} 商品庫存信息
 */
export const getProductStock = async (id, variantId = null) => {
  try {
    let url = `${API_URL}/${id}/stock`;
    if (variantId) {
      url += `?variantId=${variantId}`;
    }
    const response = await axios.get(url);
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取商品庫存信息失敗'
    );
  }
};

/**
 * 獲取商品評論
 * @param {string} productId - 商品ID
 * @param {number} page - 頁碼
 * @param {number} pageSize - 每頁數量
 * @returns {Promise} 商品評論列表
 */
export const getProductReviews = async (productId, page = 1, pageSize = 10) => {
  try {
    const response = await axios.get(`${API_URL}/${productId}/reviews`, {
      params: { page, pageSize }
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取商品評論失敗'
    );
  }
};

/**
 * 獲取相關商品
 * @param {string} productId - 商品ID
 * @param {number} limit - 數量限制
 * @returns {Promise} 相關商品列表
 */
export const getRelatedProducts = async (productId, limit = 6) => {
  try {
    const response = await axios.get(`${API_URL}/${productId}/related`, {
      params: { limit }
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取相關商品失敗'
    );
  }
};

/**
 * 獲取商品分類
 * @param {boolean} includeInactive - 是否包含未啟用的分類
 * @returns {Promise} 分類列表
 */
export const getCategories = async (includeInactive = false) => {
  try {
    const response = await axios.get(`${API_URL}/categories`, {
      params: { includeInactive }
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取商品分類失敗'
    );
  }
};

/**
 * 獲取分類樹
 * @param {boolean} includeInactive - 是否包含未啟用的分類
 * @returns {Promise} 分類樹
 */
export const getCategoryTree = async (includeInactive = false) => {
  try {
    const response = await axios.get(`${API_URL}/categories/tree`, {
      params: { includeInactive }
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取分類樹失敗'
    );
  }
};