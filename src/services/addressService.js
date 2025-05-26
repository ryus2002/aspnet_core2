/**
 * 地址服務
 * 處理用戶地址相關的API請求
 */
import axios from 'axios';

// API基礎URL
const API_URL = 'https://api-gateway/addresses';

/**
 * 設置請求頭部的認證令牌
 * @returns {object} 包含認證令牌的請求頭
 */
const authHeader = () => {
  const token = localStorage.getItem('token');
  return token ? { Authorization: `Bearer ${token}` } : {};
};

/**
 * 獲取用戶的所有地址
 * @returns {Promise} 地址列表
 */
export const getUserAddresses = async () => {
  try {
    const response = await axios.get(API_URL, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取地址列表失敗'
    );
  }
};

/**
 * 獲取地址詳情
 * @param {string} addressId - 地址ID
 * @returns {Promise} 地址詳情
 */
export const getAddressById = async (addressId) => {
  try {
    const response = await axios.get(`${API_URL}/${addressId}`, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取地址詳情失敗'
    );
  }
};

/**
 * 新增地址
 * @param {object} addressData - 地址數據
 * @returns {Promise} 新增的地址
 */
export const addUserAddress = async (addressData) => {
  try {
    const response = await axios.post(API_URL, addressData, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '新增地址失敗'
    );
  }
};

/**
 * 更新地址
 * @param {string} addressId - 地址ID
 * @param {object} addressData - 更新的地址數據
 * @returns {Promise} 更新後的地址
 */
export const updateUserAddress = async (addressId, addressData) => {
  try {
    const response = await axios.put(`${API_URL}/${addressId}`, addressData, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '更新地址失敗'
    );
  }
};

/**
 * 刪除地址
 * @param {string} addressId - 地址ID
 * @returns {Promise} 刪除結果
 */
export const deleteUserAddress = async (addressId) => {
  try {
    const response = await axios.delete(`${API_URL}/${addressId}`, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '刪除地址失敗'
    );
  }
};

/**
 * 設置默認地址
 * @param {string} addressId - 地址ID
 * @returns {Promise} 設置結果
 */
export const setDefaultAddress = async (addressId) => {
  try {
    const response = await axios.patch(`${API_URL}/${addressId}/default`, {}, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '設置默認地址失敗'
    );
  }
};