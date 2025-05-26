/**
 * 登入頁面
 * 提供用戶登入功能
 */
import React, { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Login.css';

const Login = () => {
  // 表單狀態
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  
  // 錯誤和加載狀態
  const [formError, setFormError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  // 路由相關
  const navigate = useNavigate();
  const location = useLocation();
  
  // 認證上下文
  const { login, isAuthenticated, error } = useAuth();
  
  // 如果用戶已登入，重定向到首頁或之前嘗試訪問的頁面
  useEffect(() => {
    if (isAuthenticated) {
      const from = location.state?.from?.pathname || '/';
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, location]);
  
  // 當認證錯誤變更時，更新表單錯誤
  useEffect(() => {
    if (error) {
      setFormError(error);
    }
  }, [error]);

  /**
   * 處理表單提交
   * @param {Event} e - 表單提交事件
   */
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // 表單驗證
    if (!username.trim()) {
      setFormError('請輸入用戶名');
      return;
    }
    
    if (!password) {
      setFormError('請輸入密碼');
      return;
    }
    
    try {
      setFormError('');
      setIsSubmitting(true);
      
      // 調用登入方法
      await login(username, password);
      
      // 如果設置了記住我，保存用戶名
      if (rememberMe) {
        localStorage.setItem('rememberedUsername', username);
      } else {
        localStorage.removeItem('rememberedUsername');
      }
    } catch (err) {
      console.error('登入失敗:', err);
      setFormError(err.message || '登入失敗，請檢查您的憑證');
    } finally {
      setIsSubmitting(false);
    }
  };

  // 從本地存儲加載記住的用戶名
  useEffect(() => {
    const savedUsername = localStorage.getItem('rememberedUsername');
    if (savedUsername) {
      setUsername(savedUsername);
      setRememberMe(true);
    }
  }, []);

  return (
    <div className="login-page">
      <div className="login-container">
        <div className="login-header">
          <h1>歡迎回來</h1>
          <p>請登入您的帳戶</p>
        </div>
        
        {formError && (
          <div className="error-message">
            {formError}
          </div>
        )}
        
        <form className="login-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">用戶名</label>
            <input
              type="text"
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="請輸入用戶名"
              disabled={isSubmitting}
            />
          </div>
          
          <div className="form-group">
            <label htmlFor="password">密碼</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="請輸入密碼"
              disabled={isSubmitting}
            />
          </div>
          
          <div className="form-options">
            <div className="remember-me">
              <input
                type="checkbox"
                id="rememberMe"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                disabled={isSubmitting}
              />
              <label htmlFor="rememberMe">記住我</label>
            </div>
            
            <Link to="/forgot-password" className="forgot-password">
              忘記密碼？
            </Link>
          </div>
          
          <button
            type="submit"
            className="login-button"
            disabled={isSubmitting}
          >
            {isSubmitting ? '登入中...' : '登入'}
          </button>
        </form>
        
        <div className="login-divider">
          <span>或</span>
        </div>
        
        <div className="social-login">
          <button className="social-button google">
            使用 Google 登入
          </button>
          <button className="social-button facebook">
            使用 Facebook 登入
          </button>
        </div>
        
        <div className="register-link">
          還沒有帳戶？ <Link to="/register">立即註冊</Link>
        </div>
      </div>
    </div>
  );
};

export default Login;