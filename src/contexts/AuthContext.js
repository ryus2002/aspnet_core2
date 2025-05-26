/**
 * 認證上下文
 * 管理用戶認證狀態，提供登入、登出功能
 */
import React, { createContext, useState, useEffect, useContext, useCallback } from 'react';
import { 
  login as apiLogin, 
  register as apiRegister, 
  refreshToken as apiRefreshToken,
  getCurrentUser as apiGetCurrentUser,
  logout as apiLogout,
  setupAxiosInterceptors
} from '../services/authService';
// 創建認證上下文
const AuthContext = createContext();

// 認證提供者組件
export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [tokenRefreshTimer, setTokenRefreshTimer] = useState(null);

  // 設置Axios攔截器
  useEffect(() => {
    setupAxiosInterceptors();
  }, []);

  /**
   * 解析JWT令牌
   * @param {string} token - JWT令牌
   * @returns {object} 解析後的用戶數據
   */
  const parseJwt = useCallback((token) => {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (e) {
      console.error('解析JWT失敗:', e);
      return null;
    }
  }, []);
  /**
   * 檢查令牌是否過期
   * @param {object} tokenData - 解析後的令牌數據
   * @returns {boolean} 是否過期
   */
  const isTokenExpired = useCallback((tokenData) => {
    if (!tokenData || !tokenData.exp) return true;
    const currentTime = Math.floor(Date.now() / 1000);
    return tokenData.exp < currentTime;
  }, []);
  /**
   * 設置令牌刷新定時器
   * @param {string} token - JWT令牌
   */
  const setupTokenRefresh = useCallback((token) => {
    // 清除現有定時器
    if (tokenRefreshTimer) {
      clearTimeout(tokenRefreshTimer);
    }
    
    const tokenData = parseJwt(token);
    if (tokenData && tokenData.exp) {
      // 計算令牌過期時間（提前5分鐘刷新）
      const expiresIn = (tokenData.exp * 1000) - Date.now() - (5 * 60 * 1000);
      
      if (expiresIn > 0) {
        // 設置定時器在令牌即將過期時刷新
        const timer = setTimeout(async () => {
          const refreshTokenValue = localStorage.getItem('refreshToken');
          if (refreshTokenValue) {
            try {
              const response = await apiRefreshToken(refreshTokenValue);
              localStorage.setItem('token', response.token);
              localStorage.setItem('refreshToken', response.refreshToken);
              localStorage.setItem('tokenExpires', response.expires);
              
              // 更新用戶數據
              setUser(parseJwt(response.token));
              
              // 設置下一次刷新
              setupTokenRefresh(response.token);
    } catch (err) {
              console.error('自動刷新令牌失敗:', err);
              logout();
    }
          }
        }, expiresIn);
        
        setTokenRefreshTimer(timer);
      }
    }
  }, [parseJwt, tokenRefreshTimer]);

  // 在組件掛載時檢查是否已登入
  useEffect(() => {
    const checkAuthStatus = async () => {
      const token = localStorage.getItem('token');
      const refreshTokenValue = localStorage.getItem('refreshToken');
      
      if (token) {
        try {
          // 解析JWT獲取用戶信息
          const userData = parseJwt(token);
          
          // 檢查token是否過期
          if (isTokenExpired(userData)) {
            // 如果過期且有refreshToken，嘗試刷新
            if (refreshTokenValue) {
              try {
                const response = await apiRefreshToken(refreshTokenValue);
                localStorage.setItem('token', response.token);
                localStorage.setItem('refreshToken', response.refreshToken);
                localStorage.setItem('tokenExpires', response.expires);
                
                const newUserData = parseJwt(response.token);
                setUser(newUserData);
                
                // 設置令牌刷新定時器
                setupTokenRefresh(response.token);
              } catch (err) {
                // 刷新失敗，清除登入狀態
                logout();
              }
            } else {
              // 沒有refreshToken，清除登入狀態
              logout();
            }
          } else {
            // token有效，設置用戶信息
            setUser(userData);
            
            // 設置令牌刷新定時器
            setupTokenRefresh(token);
            
            // 獲取完整的用戶信息
            try {
              const userInfo = await apiGetCurrentUser();
              // 合併JWT中的基本信息和API返回的詳細信息
              setUser(prevUser => ({
                ...prevUser,
                ...userInfo
              }));
            } catch (err) {
              console.error('獲取用戶詳細信息失敗:', err);
            }
          }
        } catch (err) {
          console.error('認證狀態檢查失敗:', err);
          logout();
        }
      }
      
      setLoading(false);
  };

    checkAuthStatus();
    
    // 組件卸載時清除定時器
    return () => {
      if (tokenRefreshTimer) {
        clearTimeout(tokenRefreshTimer);
      }
  };
  }, [parseJwt, isTokenExpired, setupTokenRefresh]);

  /**
   * 用戶登入
   * @param {string} username - 用戶名
   * @param {string} password - 密碼
   * @returns {Promise} 登入結果
   */
  const login = async (username, password) => {
    try {
      setError(null);
      const response = await apiLogin(username, password);
      
      const userData = parseJwt(response.token);
      setUser(userData);
      
      // 設置令牌刷新定時器
      setupTokenRefresh(response.token);
      
      return response;
    } catch (err) {
      setError(err.message || '登入失敗，請檢查您的憑證');
      throw err;
    }
  };

  /**
   * 用戶註冊
   * @param {object} userData - 用戶註冊數據
   * @returns {Promise} 註冊結果
   */
  const register = async (userData) => {
    try {
      setError(null);
      const response = await apiRegister(userData);
      return response;
    } catch (err) {
      setError(err.message || '註冊失敗，請稍後再試');
      throw err;
    }
  };

  /**
   * 用戶登出
   */
  const logout = () => {
    // 調用API登出
    apiLogout();
    
    // 清除本地狀態
    setUser(null);
    
    // 清除令牌刷新定時器
    if (tokenRefreshTimer) {
      clearTimeout(tokenRefreshTimer);
      setTokenRefreshTimer(null);
    }
};

  /**
   * 檢查用戶是否有特定角色
   * @param {string|string[]} roles - 要檢查的角色或角色列表
   * @returns {boolean} 用戶是否擁有指定角色
   */
  const hasRole = useCallback((roles) => {
    if (!user || !user.role) return false;
    
    if (Array.isArray(roles)) {
      return roles.some(role => user.role.includes(role));
    }
    
    return user.role.includes(roles);
  }, [user]);

  // 提供認證相關的狀態和方法
  const value = {
    user,
    loading,
    error,
    login,
    register,
    logout,
    hasRole,
    isAuthenticated: !!user,
    refreshToken: async () => {
      const refreshTokenValue = localStorage.getItem('refreshToken');
      if (refreshTokenValue) {
        const response = await apiRefreshToken(refreshTokenValue);
        setUser(parseJwt(response.token));
        setupTokenRefresh(response.token);
        return response;
      }
      throw new Error('沒有可用的刷新令牌');
    }
};

  return (
    <AuthContext.Provider value={value}></AuthContext.Provider>
      {children}
    </AuthContext.Provider>
  );
};

// 自定義Hook，方便在組件中使用認證上下文
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth必須在AuthProvider內部使用');
  }
  return context;
};