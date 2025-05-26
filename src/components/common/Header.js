/**
 * 頁頭組件
 * 顯示網站標題、導航菜單、搜尋欄、用戶狀態和購物車
 */
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useCart } from '../../contexts/CartContext';
import './Header.css';

const Header = () => {
  const { user, isAuthenticated, logout } = useAuth();
  const { cartCount } = useCart();
  const [searchKeyword, setSearchKeyword] = useState('');
  const [menuOpen, setMenuOpen] = useState(false);
  const navigate = useNavigate();

  /**
   * 處理搜尋表單提交
   * @param {Event} e - 表單提交事件
   */
  const handleSearch = (e) => {
    e.preventDefault();
    if (searchKeyword.trim()) {
      navigate(`/products?keyword=${encodeURIComponent(searchKeyword.trim())}`);
    }
  };

  /**
   * 處理用戶登出
   */
  const handleLogout = () => {
    logout();
    navigate('/');
  };

  /**
   * 切換移動端選單
   */
  const toggleMenu = () => {
    setMenuOpen(!menuOpen);
  };

  return (
    <header className="header">
      <div className="container header-container">
        {/* 網站標題 */}
        <div className="header-brand">
          <Link to="/" className="logo">電商平台</Link>
        </div>

        {/* 漢堡選單按鈕（移動端顯示） */}
        <button className="menu-toggle" onClick={toggleMenu}>
          <span className="menu-icon"></span>
        </button>

        {/* 導航選單 */}
        <nav className={`main-nav ${menuOpen ? 'active' : ''}`}>
          <ul className="nav-list">
            <li className="nav-item">
              <Link to="/" className="nav-link">首頁</Link>
            </li>
            <li className="nav-item">
              <Link to="/products" className="nav-link">所有商品</Link>
            </li>
            <li className="nav-item">
              <Link to="/categories" className="nav-link">商品分類</Link>
            </li>
            <li className="nav-item">
              <Link to="/about" className="nav-link">關於我們</Link>
            </li>
          </ul>
        </nav>

        {/* 搜尋欄 */}
        <div className="search-container">
          <form onSubmit={handleSearch}>
            <input
              type="text"
              className="search-input"
              placeholder="搜尋商品..."
              value={searchKeyword}
              onChange={(e) => setSearchKeyword(e.target.value)}
            />
            <button type="submit" className="search-button">
              <i className="search-icon">🔍</i>
            </button>
          </form>
        </div>

        {/* 用戶操作區 */}
        <div className="user-actions">
          {/* 購物車 */}
          <div className="cart-icon-container">
            <Link to="/cart" className="cart-icon">
              🛒
              {cartCount > 0 && (
                <span className="cart-badge">{cartCount}</span>
              )}
            </Link>
          </div>

          {/* 用戶選單 */}
          <div className="user-menu">
            {isAuthenticated ? (
              <div className="dropdown">
                <button className="dropdown-toggle">
                  {user?.name || '用戶'} ▼
                </button>
                <div className="dropdown-menu">
                  <Link to="/profile" className="dropdown-item">個人資料</Link>
                  <Link to="/orders" className="dropdown-item">我的訂單</Link>
                  <button onClick={handleLogout} className="dropdown-item">登出</button>
                </div>
              </div>
            ) : (
              <div className="auth-links">
                <Link to="/login" className="auth-link">登入</Link>
                <Link to="/register" className="auth-link">註冊</Link>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header;