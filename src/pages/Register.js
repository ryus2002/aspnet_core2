/**
 * 註冊頁面
 * 提供用戶註冊功能
 */
import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Register.css';

const Register = () => {
  // 表單狀態
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    agreeTerms: false
  });
  
  // 表單錯誤狀態
  const [formErrors, setFormErrors] = useState({});
  
  // 提交和錯誤狀態
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState('');
  
  // 路由導航
  const navigate = useNavigate();
  
  // 認證上下文
  const { register, isAuthenticated } = useAuth();
  
  // 如果用戶已登入，重定向到首頁
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/');
    }
  }, [isAuthenticated, navigate]);

  /**
   * 處理表單輸入變更
   * @param {Event} e - 輸入變更事件
   */
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData({
      ...formData,
      [name]: type === 'checkbox' ? checked : value
    });
    
    // 清除該欄位的錯誤
    if (formErrors[name]) {
      setFormErrors({
        ...formErrors,
        [name]: ''
      });
    }
  };

  /**
   * 驗證表單
   * @returns {boolean} 表單是否有效
   */
  const validateForm = () => {
    const errors = {};
    
    // 驗證用戶名
    if (!formData.username.trim()) {
      errors.username = '請輸入用戶名';
    } else if (formData.username.length < 3) {
      errors.username = '用戶名至少需要3個字符';
    }
    
    // 驗證電子郵件
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!formData.email.trim()) {
      errors.email = '請輸入電子郵件';
    } else if (!emailRegex.test(formData.email)) {
      errors.email = '請輸入有效的電子郵件地址';
    }
    
    // 驗證密碼
    if (!formData.password) {
      errors.password = '請輸入密碼';
    } else if (formData.password.length < 6) {
      errors.password = '密碼至少需要6個字符';
    }
    
    // 驗證確認密碼
    if (!formData.confirmPassword) {
      errors.confirmPassword = '請確認密碼';
    } else if (formData.password !== formData.confirmPassword) {
      errors.confirmPassword = '兩次輸入的密碼不一致';
    }
    
    // 驗證同意條款
    if (!formData.agreeTerms) {
      errors.agreeTerms = '請同意服務條款和隱私政策';
    }
    
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  /**
   * 處理表單提交
   * @param {Event} e - 表單提交事件
   */
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // 驗證表單
    if (!validateForm()) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setSubmitError('');
      
      // 調用註冊方法
      await register({
        username: formData.username,
        email: formData.email,
        password: formData.password
      });
      
      // 註冊成功，導航到登入頁面
      navigate('/login', { state: { message: '註冊成功！請登入您的帳戶。' } });
    } catch (err) {
      console.error('註冊失敗:', err);
      setSubmitError(err.message || '註冊失敗，請稍後再試');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="register-page">
      <div className="register-container">
        <div className="register-header">
          <h1>創建帳戶</h1>
          <p>填寫以下信息以註冊</p>
        </div>
        
        {submitError && (
          <div className="error-message">
            {submitError}
          </div>
        )}
        
        <form className="register-form" onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">用戶名 <span className="required">*</span></label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="請輸入用戶名"
              disabled={isSubmitting}
              className={formErrors.username ? 'error' : ''}
            />
            {formErrors.username && (
              <div className="field-error">{formErrors.username}</div>
            )}
          </div>
          
          <div className="form-group">
            <label htmlFor="email">電子郵件 <span className="required">*</span></label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="請輸入電子郵件"
              disabled={isSubmitting}
              className={formErrors.email ? 'error' : ''}
            />
            {formErrors.email && (
              <div className="field-error">{formErrors.email}</div>
            )}
          </div>
          
          <div className="form-group">
            <label htmlFor="password">密碼 <span className="required">*</span></label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="請輸入密碼"
              disabled={isSubmitting}
              className={formErrors.password ? 'error' : ''}
            />
            {formErrors.password && (
              <div className="field-error">{formErrors.password}</div>
            )}
            <div className="password-strength">
              <div className={`strength-bar ${formData.password.length >= 6 ? 'medium' : ''} ${formData.password.length >= 8 ? 'strong' : ''}`}></div>
              <span className="strength-text">
                {formData.password.length === 0 && '請輸入密碼'}
                {formData.password.length > 0 && formData.password.length < 6 && '弱'}
                {formData.password.length >= 6 && formData.password.length < 8 && '中'}
                {formData.password.length >= 8 && '強'}
              </span>
            </div>
          </div>
          
          <div className="form-group">
            <label htmlFor="confirmPassword">確認密碼 <span className="required">*</span></label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="請再次輸入密碼"
              disabled={isSubmitting}
              className={formErrors.confirmPassword ? 'error' : ''}
            />
            {formErrors.confirmPassword && (
              <div className="field-error">{formErrors.confirmPassword}</div>
            )}
          </div>
          
          <div className="form-group checkbox-group">
            <input
              type="checkbox"
              id="agreeTerms"
              name="agreeTerms"
              checked={formData.agreeTerms}
              onChange={handleChange}
              disabled={isSubmitting}
            />
            <label htmlFor="agreeTerms">
              我已閱讀並同意 <Link to="/terms" target="_blank">服務條款</Link> 和 <Link to="/privacy" target="_blank">隱私政策</Link>
            </label>
            {formErrors.agreeTerms && (
              <div className="field-error">{formErrors.agreeTerms}</div>
            )}
          </div>
          
          <button
            type="submit"
            className="register-button"
            disabled={isSubmitting}
          >
            {isSubmitting ? '註冊中...' : '註冊'}
          </button>
        </form>
        
        <div className="register-divider">
          <span>或</span>
        </div>
        
        <div className="social-register">
          <button className="social-button google">
            使用 Google 註冊
          </button>
          <button className="social-button facebook">
            使用 Facebook 註冊
          </button>
        </div>
        
        <div className="login-link">
          已有帳戶？ <Link to="/login">立即登入</Link>
        </div>
      </div>
    </div>
  );
};

export default Register;