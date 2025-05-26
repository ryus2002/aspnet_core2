/**
 * 認證服務
 * 處理用戶註冊、登入、登出等認證相關API請求
 */
import axios from 'axios';

// API基礎URL
const API_URL = 'https://api-gateway/auth';

/**
 * 設置請求頭部的認證令牌
 * @returns {object} 包含認證令牌的請求頭
 */
const authHeader = () => {
  const token = localStorage.getItem('token');
  return token ? { Authorization: `Bearer ${token}` } : {};
};

/**
 * 用戶登入
 * @param {string} username - 用戶名
 * @param {string} password - 密碼
 * @returns {Promise} 登入結果，包含令牌和用戶信息
 */
export const login = async (username, password) => {
  try {
    const response = await axios.post(`${API_URL}/login`, {
      username,
      password
    });
    
    // 存儲令牌到本地存儲
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('refreshToken', response.data.refreshToken);
      localStorage.setItem('tokenExpires', response.data.expires);
      
      // 設置全局默認請求頭
      axios.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
    }
    
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '登入失敗，請檢查您的憑證'
    );
  }
};

/**
 * 用戶註冊
 * @param {object} userData - 用戶註冊數據
 * @returns {Promise} 註冊結果
 */
export const register = async (userData) => {
  try {
    const response = await axios.post(`${API_URL}/register`, userData);
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '註冊失敗，請稍後再試'
    );
  }
};

/**
 * 刷新認證令牌
 * @param {string} refreshToken - 刷新令牌
 * @returns {Promise} 新的認證令牌
 */
export const refreshToken = async (refreshToken) => {
  try {
    const response = await axios.post(`${API_URL}/refresh-token`, {
      refreshToken
    });
    
    // 更新本地存儲的令牌
    if (response.data.token) {
      localStorage.setItem('token', response.data.token);
      localStorage.setItem('refreshToken', response.data.refreshToken);
      localStorage.setItem('tokenExpires', response.data.expires);
      
      // 更新全局默認請求頭
      axios.defaults.headers.common['Authorization'] = `Bearer ${response.data.token}`;
    }
    return response.data;
  } catch (error) {
    // 刷新令牌失敗，清除所有令牌
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpires');
    delete axios.defaults.headers.common['Authorization'];
    throw new Error('無法刷新令牌，請重新登入');
  }
};

/**
 * 獲取當前用戶信息
 * @returns {Promise} 用戶信息
 */
export const getCurrentUser = async () => {
  try {
    const response = await axios.get(`${API_URL}/me`, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '獲取用戶信息失敗'
    );
  }
};

/**
 * 更新用戶信息
 * @param {object} userData - 更新的用戶數據
 * @returns {Promise} 更新結果
 */
export const updateUserProfile = async (userData) => {
  try {
    const response = await axios.put(`${API_URL}/profile`, userData, {
      headers: authHeader()
    });
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '更新用戶信息失敗'
    );
  }
};

/**
 * 修改密碼
 * @param {string} oldPassword - 舊密碼
 * @param {string} newPassword - 新密碼
 * @returns {Promise} 修改結果
 */
export const changePassword = async (oldPassword, newPassword) => {
  try {
    const response = await axios.post(
      `${API_URL}/change-password`,
      { oldPassword, newPassword },
      { headers: authHeader() }
    );
    return response.data;
  } catch (error) {
    throw new Error(
      error.response?.data?.message || '修改密碼失敗'
    );
  }
};

/**
 * 用戶登出
 */
export const logout = () => {
  // 清除本地存儲的令牌
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  localStorage.removeItem('tokenExpires');
  
  // 清除全局請求頭中的認證信息
  delete axios.defaults.headers.common['Authorization'];
};

/**
 * 設置請求攔截器，自動處理令牌過期
 */
export const setupAxiosInterceptors = () => {
  // 請求攔截器
  axios.interceptors.request.use(
    async (config) => {
      // 檢查是否有令牌
      const token = localStorage.getItem('token');
      const tokenExpires = localStorage.getItem('tokenExpires');
      const refreshTokenValue = localStorage.getItem('refreshToken');
      
      if (token) {
        // 檢查令牌是否即將過期（提前5分鐘刷新）
        const isExpiring = tokenExpires && new Date(tokenExpires) < new Date(Date.now() + 5 * 60 * 1000);
        
        if (isExpiring && refreshTokenValue) {
          try {
            // 嘗試刷新令牌
            const response = await refreshToken(refreshTokenValue);
            config.headers.Authorization = `Bearer ${response.token}`;
          } catch (error) {
            console.error('令牌刷新失敗:', error);
            // 刷新失敗，不設置認證頭
          }
        } else {
          // 令牌有效，設置認證頭
          config.headers.Authorization = `Bearer ${token}`;
        }
      }
      
      return config;
    },
    (error) => Promise.reject(error)
  );
  
  // 響應攔截器
  axios.interceptors.response.use(
    (response) => response,
    async (error) => {
      const originalRequest = error.config;
      
      // 如果是401錯誤且未嘗試過刷新令牌
      if (error.response?.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;
        
        const refreshTokenValue = localStorage.getItem('refreshToken');
        if (refreshTokenValue) {
          try {
            // 嘗試刷新令牌
            const response = await refreshToken(refreshTokenValue);
            
            // 更新原始請求的認證頭
            originalRequest.headers.Authorization = `Bearer ${response.token}`;
            
            // 重新發送原始請求
            return axios(originalRequest);
          } catch (refreshError) {
            // 刷新失敗，重定向到登入頁面
            window.location.href = '/login';
            return Promise.reject(refreshError);
          }
        }
      }
      
      return Promise.reject(error);
    }
  );
};