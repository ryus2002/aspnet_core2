/**
 * 404頁面
 * 當用戶訪問不存在的頁面時顯示
 */
import React from 'react';
import { Link } from 'react-router-dom';
import './NotFound.css';

const NotFound = () => {
  return (
    <div className="not-found-page">
      <div className="container">
        <div className="not-found-content">
          <div className="not-found-code">404</div>
          <h1 className="not-found-title">頁面不存在</h1>
          <p className="not-found-message">
            很抱歉，您請求的頁面不存在或已被移除。
          </p>
          <div className="not-found-actions">
            <Link to="/" className="home-button">
              返回首頁
            </Link>
            <Link to="/products" className="products-button">
              瀏覽商品
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default NotFound;