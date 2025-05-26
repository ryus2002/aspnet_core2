/**
 * HTTP攔截器
 * 處理請求和響應攔截，自動添加認證令牌和處理錯誤
 */
import axios from 'axios';

/**
 * 設置HTTP攔截器
 * @param {function} refreshTokenFn - 刷新令牌的函數
 * @param {function} logoutFn - 登出函數
 */
export const setupHttpInterceptors = (refreshTokenFn, logoutFn) => {
  // 請求攔截器
  axios.interceptors.request.use(
    (config) => {
      // 從本地存儲獲取令牌
      const token = localStorage.getItem('token');
      
      // 如果有令牌，添加到請求頭
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
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
      
      // 如果是401錯誤（未授權）且未嘗試過刷新令牌
      if (error.response?.status === 401 && !originalRequest._retry) {
        originalRequest._retry = true;
        
        try {
          // 嘗試刷新令牌
          const response = await refreshTokenFn();
          
          // 更新原始請求的認證頭
          originalRequest.headers.Authorization = `Bearer ${response.token}`;
          
          // 重新發送原始請求
          return axios(originalRequest);
        } catch (refreshError) {
          // 刷新令牌失敗，執行登出
          logoutFn();
          
          // 重定向到登入頁面
          window.location.href = '/login';
          
          return Promise.reject(refreshError);
        }
      }
      
      // 處理其他錯誤
      if (error.response?.status === 403) {
        // 權限不足
        window.location.href = '/unauthorized';
      } else if (error.response?.status === 404) {
        // 資源不存在
        window.location.href = '/404';
      } else if (error.response?.status >= 500) {
        // 服務器錯誤
        console.error('服務器錯誤:', error);
        // 可以顯示全局錯誤通知
      }
      
      return Promise.reject(error);
    }
  );
};