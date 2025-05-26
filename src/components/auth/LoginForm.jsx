/**
 * 登入表單組件
 */
import React, { useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import './Auth.css';

const LoginForm = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [formError, setFormError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  
  const { login, error: authError, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  
  // 從location state獲取重定向路徑
  const from = location.state?.from?.pathname || '/';
  
  // 如果用戶已登入，重定向到之前的頁面或首頁
  useEffect(() => {
    if (isAuthenticated) {
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, from]);
  
  // 當認證錯誤變化時，更新表單錯誤
  useEffect(() => {
    if (authError) {
      setFormError(authError);
      setIsSubmitting(false);
    }
  }, [authError]);
  /**
   * 處理表單提交
   * @param {Event} e - 表單事件
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
    
    setFormError('');
    setIsSubmitting(true);
    
    try {
      // 調用登入API
      await login(username, password);
      
      // 如果選擇"記住我"，將用戶名保存到localStorage
      if (rememberMe) {
        localStorage.setItem('rememberedUsername', username);
      } else {
        localStorage.removeItem('rememberedUsername');
      }
      
      // 登入成功後重定向
      navigate(from, { replace: true });
    } catch (err) {
      // 錯誤處理在useEffect中進行
      console.error('登入失敗:', err);
    }
  };

  // 頁面加載時，檢查是否有保存的用戶名
  useEffect(() => {
    const savedUsername = localStorage.getItem('rememberedUsername');
    if (savedUsername) {
      setUsername(savedUsername);
      setRememberMe(true);
    }
  }, []);
  
  return (
    <div className="auth-form-container">
      <h2>登入</h2>
      {formError && <div className="auth-error">{formError}</div>}
      
      <form onSubmit={handleSubmit} className="auth-form">
        <div className="form-group">
          <label htmlFor="username">用戶名</label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            disabled={isSubmitting}
            placeholder="請輸入用戶名"
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="password">密碼</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            disabled={isSubmitting}
            placeholder="請輸入密碼"
          />
        </div>
        
        <div className="form-group checkbox">
          <input
            type="checkbox"
            id="rememberMe"
            checked={rememberMe}
            onChange={(e) => setRememberMe(e.target.checked)}
            disabled={isSubmitting}
          />
          <label htmlFor="rememberMe">記住我</label>
          </div>
        
        <button 
          type="submit" 
          className="auth-button" 
          disabled={isSubmitting}
        >
          {isSubmitting ? '登入中...' : '登入'}
        </button>
      </form>
      
      <div className="auth-links">
        <a href="/forgot-password">忘記密碼？</a>
        <span>還沒有帳號？<a href="/register">立即註冊</a></span>
    </div>
    </div>
  );
};

export default LoginForm;