/**
 * 頁尾組件
 * 顯示網站資訊、導航連結、聯繫方式和版權信息
 */
import React from 'react';
import { Link } from 'react-router-dom';
import './Footer.css';

const Footer = () => {
  // 獲取當前年份，用於版權聲明
  const currentYear = new Date().getFullYear();

  return (
    <footer className="footer">
      <div className="container footer-container">
        <div className="footer-section">
          <h3 className="footer-title">關於我們</h3>
          <p className="footer-text">
            我們是一家專注於提供優質商品和卓越購物體驗的電商平台。
            我們致力於為客戶提供最好的服務和最優惠的價格。
          </p>
        </div>

        <div className="footer-section">
          <h3 className="footer-title">快速連結</h3>
          <ul className="footer-links">
            <li><Link to="/">首頁</Link></li>
            <li><Link to="/products">所有商品</Link></li>
            <li><Link to="/categories">商品分類</Link></li>
            <li><Link to="/about">關於我們</Link></li>
            <li><Link to="/contact">聯繫我們</Link></li>
            <li><Link to="/faq">常見問題</Link></li>
          </ul>
        </div>

        <div className="footer-section">
          <h3 className="footer-title">客戶服務</h3>
          <ul className="footer-links">
            <li><Link to="/shipping">配送政策</Link></li>
            <li><Link to="/returns">退換貨政策</Link></li>
            <li><Link to="/privacy">隱私政策</Link></li>
            <li><Link to="/terms">使用條款</Link></li>
          </ul>
        </div>

        <div className="footer-section">
          <h3 className="footer-title">聯繫我們</h3>
          <address className="footer-contact">
            <p>電話：(02) 1234-5678</p>
            <p>電子郵件：support@example.com</p>
            <p>地址：台北市信義區信義路五段7號</p>
          </address>
          <div className="social-links">
            <a href="https://facebook.com" target="_blank" rel="noopener noreferrer" className="social-link">FB</a>
            <a href="https://instagram.com" target="_blank" rel="noopener noreferrer" className="social-link">IG</a>
            <a href="https://line.me" target="_blank" rel="noopener noreferrer" className="social-link">LINE</a>
          </div>
        </div>
      </div>

      <div className="footer-bottom">
        <div className="container">
          <p className="copyright">
            &copy; {currentYear} 電商平台. 版權所有.
          </p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;