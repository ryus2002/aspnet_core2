/**
 * 受保護路由組件
 * 用於限制未登入用戶訪問特定頁面，並支持基於角色的訪問控制
 */
import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

/**
 * 受保護路由組件
 * @param {object} props - 組件屬性
 * @param {React.ReactNode} props.children - 子組件
 * @param {string|string[]} [props.requiredRoles] - 訪問所需角色
 * @param {string} [props.redirectPath="/login"] - 未授權時重定向路徑
 * @returns {React.ReactNode} 渲染內容
 */
const ProtectedRoute = ({ 
  children, 
  requiredRoles, 
  redirectPath = "/login" 
}) => {
  const { isAuthenticated, loading, hasRole } = useAuth();
  const location = useLocation();

  // 如果認證狀態正在加載中，顯示加載指示器
  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>載入中...</p>
      </div>
    );
  }

  // 如果用戶未登入，重定向到登入頁面，並保存當前位置
  if (!isAuthenticated) {
    return <Navigate to={redirectPath} state={{ from: location }} replace />;
  }

  // 如果指定了所需角色，檢查用戶是否擁有該角色
  if (requiredRoles && !hasRole(requiredRoles)) {
    // 用戶已登入但沒有所需角色，重定向到未授權頁面
    return <Navigate to="/unauthorized" state={{ from: location }} replace />;
  }
  // 用戶已登入且擁有所需角色（如果有指定），渲染子組件
  return children;
};

export default ProtectedRoute;