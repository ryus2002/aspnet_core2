/**
 * 註冊表單組件
 */
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import './Auth.css';

const RegisterForm = () => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    agreeTerms: false
  });
  
  const [formErrors, setFormErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  
  const { register, error: authError } = useAuth();
  const navigate = useNavigate();
  
  // 當認證錯誤變化時，更新表單錯誤
  useEffect(() => {
    if (authError) {
      setFormErrors({ general: authError });
      setIsSubmitting(false);
    }
  }, [authError]);
  /**
   * 處理輸入變化
   * @param {Event} e - 輸入事件
   */
  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
    
    // 清除對應字段的錯誤
    if (formErrors[name]) {
      setFormErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[name];
        return newErrors;
      });
    }
  };

  /**
   * 驗證表單
   * @returns {boolean} 表單是否有效
   */
  const validateForm = () => {
    const errors = {};
    
    // 用戶名驗證
    if (!formData.username.trim()) {
      errors.username = '請輸入用戶名';
    } else if (formData.username.length < 3) {
      errors.username = '用戶名至少需要3個字符';
    }
    
    // 電子郵件驗證
    if (!formData.email.trim()) {
      errors.email = '請輸入電子郵件';
    } else if (!/\S+@\S+\.\S+/.test(formData.email)) {
      errors.email = '請輸入有效的電子郵件地址';
    }
    
    // 密碼驗證
    if (!formData.password) {
      errors.password = '請輸入密碼';
    } else if (formData.password.length < 8) {
      errors.password = '密碼至少需要8個字符';
    } else if (!/(?=.*\d)(?=.*[a-z])(?=.*[A-Z])/.test(formData.password)) {
      errors.password = '密碼需包含數字、小寫和大寫字母';
    }
    
    // 確認密碼驗證
    if (formData.password !== formData.confirmPassword) {
      errors.confirmPassword = '兩次輸入的密碼不一致';
    }
    
    // 姓名驗證（可選）
    if (formData.firstName && formData.firstName.length < 2) {
      errors.firstName = '名字至少需要2個字符';
    }
    
    if (formData.lastName && formData.lastName.length < 2) {
      errors.lastName = '姓氏至少需要2個字符';
    }
    
    // 條款同意驗證
    if (!formData.agreeTerms) {
      errors.agreeTerms = '請同意服務條款和隱私政策';
    }
    
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  /**
   * 處理表單提交
   * @param {Event} e - 表單事件
   */
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // 表單驗證
    if (!validateForm()) {
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      // 準備註冊數據
      const userData = {
        username: formData.username,
        email: formData.email,
        password: formData.password,
        firstName: formData.firstName,
        lastName: formData.lastName
};

      // 調用註冊API
      await register(userData);
      
      // 註冊成功
      setSuccessMessage('註冊成功！即將跳轉到登入頁面...');
      
      // 3秒後重定向到登入頁面
      setTimeout(() => {
        navigate('/login');
      }, 3000);
    } catch (err) {
      // 錯誤處理在useEffect中進行
      console.error('註冊失敗:', err);
    } finally {
      setIsSubmitting(false);
    }
  };
  
  return (
    <div className="auth-form-container">
      <h2>註冊帳號</h2>
      
      {successMessage && (
        <div className="auth-success">{successMessage}</div>
      )}
      
      {formErrors.general && (
        <div className="auth-error">{formErrors.general}</div>
      )}
      
      <form onSubmit={handleSubmit} className="auth-form">
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="firstName">名字</label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              disabled={isSubmitting}
              placeholder="請輸入名字"
            />
            {formErrors.firstName && (
              <span className="error-message">{formErrors.firstName}</span>
            )}
          </div>
          
          <div className="form-group">
            <label htmlFor="lastName">姓氏</label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              disabled={isSubmitting}
              placeholder="請輸入姓氏"
            />
            {formErrors.lastName && (
              <span className="error-message">{formErrors.lastName}</span>
            )}
          </div>
        </div>
        
        <div className="form-group">
          <label htmlFor="username">用戶名 *</label>
          <input
            type="text"
            id="username"
            name="username"
            value={formData.username}
            onChange={handleChange}
            disabled={isSubmitting}
            placeholder="請輸入用戶名"
            required
          />
          {formErrors.username && (
            <span className="error-message">{formErrors.username}</span>
          )}
        </div>
        
        <div className="form-group">
          <label htmlFor="email">電子郵件 *</label>
          <input
            type="email"
            id="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            disabled={isSubmitting}
            placeholder="請輸入電子郵件"
            required
          />
          {formErrors.email && (
            <span className="error-message">{formErrors.email}</span>
          )}
        </div>
        
        <div className="form-group">
          <label htmlFor="password">密碼 *</label>
          <input
            type="password"
            id="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            disabled={isSubmitting}
            placeholder="請輸入密碼"
            required
          />
          {formErrors.password && (
            <span className="error-message">{formErrors.password}</span>
          )}
        </div>
        
        <div className="form-group">
          <label htmlFor="confirmPassword">確認密碼 *</label>
          <input
            type="password"
            id="confirmPassword"
            name="confirmPassword"
            value={formData.confirmPassword}
            onChange={handleChange}
            disabled={isSubmitting}
            placeholder="請再次輸入密碼"
            required
          />
          {formErrors.confirmPassword && (
            <span className="error-message">{formErrors.confirmPassword}</span>
          )}
        </div>
        
        <div className="form-group checkbox">
          <input
            type="checkbox"
            id="agreeTerms"
            name="agreeTerms"
            checked={formData.agreeTerms}
            onChange={handleChange}
            disabled={isSubmitting}
            required
          />
          <label htmlFor="agreeTerms">
            我同意<a href="/terms" target="_blank">服務條款</a>和<a href="/privacy" target="_blank">隱私政策</a>
          </label>
          {formErrors.agreeTerms && (
            <span className="error-message">{formErrors.agreeTerms}</span>
          )}
        </div>
        
        <button 
          type="submit" 
          className="auth-button" 
          disabled={isSubmitting}
        >
          {isSubmitting ? '註冊中...' : '註冊'}
        </button>
      </form>
      
      <div className="auth-links">
        <span>已有帳號？<a href="/login">立即登入</a></span>
      </div>
    </div>
  );
};

export default RegisterForm;