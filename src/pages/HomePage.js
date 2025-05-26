/**
 * 首頁
 * 網站的主頁，展示熱門商品、促銷信息等
 */
import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getProducts } from '../services/productService';
import './HomePage.css';

const HomePage = () => {
  // 熱門商品狀態
  const [featuredProducts, setFeaturedProducts] = useState([]);
  const [newArrivals, setNewArrivals] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // 載入商品數據
  useEffect(() => {
    fetchProducts();
  }, []);

  /**
   * 獲取商品數據
   */
  const fetchProducts = async () => {
    try {
      setLoading(true);
      setError('');
      
      // 獲取熱門商品
      const featuredResponse = await getProducts({
        pageSize: 4,
        sortBy: 'popularity',
        sortDirection: 'desc'
      });
      
      setFeaturedProducts(featuredResponse.items || []);
      
      // 獲取新品
      const newArrivalsResponse = await getProducts({
        pageSize: 8,
        sortBy: 'createdAt',
        sortDirection: 'desc'
      });
      
      setNewArrivals(newArrivalsResponse.items || []);
    } catch (err) {
      console.error('獲取商品失敗:', err);
      setError('無法載入商品，請稍後再試');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="home-page">
      {/* 英雄區塊 */}
      <section className="hero-section">
        <div className="container">
          <div className="hero-content">
            <h1 className="hero-title">探索最新商品</h1>
            <p className="hero-subtitle">發現優質商品，享受購物樂趣</p>
            <Link to="/products" className="hero-button">
              立即購物
            </Link>
          </div>
        </div>
      </section>
      
      {/* 特色分類 */}
      <section className="categories-section">
        <div className="container">
          <h2 className="section-title">熱門分類</h2>
          
          <div className="categories-grid">
            <div className="category-card">
              <Link to="/products?category=electronics" className="category-link">
                <div className="category-image">
                  <img src="https://via.placeholder.com/300x200?text=電子產品" alt="電子產品" />
                </div>
                <h3 className="category-name">電子產品</h3>
              </Link>
            </div>
            
            <div className="category-card">
              <Link to="/products?category=clothing" className="category-link">
                <div className="category-image">
                  <img src="https://via.placeholder.com/300x200?text=服飾" alt="服飾" />
                </div>
                <h3 className="category-name">服飾</h3>
              </Link>
            </div>
            
            <div className="category-card">
              <Link to="/products?category=home" className="category-link">
                <div className="category-image">
                  <img src="https://via.placeholder.com/300x200?text=家居" alt="家居" />
                </div>
                <h3 className="category-name">家居</h3>
              </Link>
            </div>
            
            <div className="category-card">
              <Link to="/products?category=beauty" className="category-link">
                <div className="category-image">
                  <img src="https://via.placeholder.com/300x200?text=美妝" alt="美妝" />
                </div>
                <h3 className="category-name">美妝</h3>
              </Link>
            </div>
          </div>
        </div>
      </section>
      
      {/* 熱門商品 */}
      <section className="featured-section">
        <div className="container">
          <h2 className="section-title">熱門商品</h2>
          
          {loading ? (
            <div className="loading-container">
              <div className="loading-spinner"></div>
              <p>載入商品中...</p>
            </div>
          ) : error ? (
            <div className="error-message">
              {error}
            </div>
          ) : (
            <div className="featured-grid">
              {featuredProducts.map(product => (
                <div key={product.id} className="product-card">
                  <Link to={`/products/${product.id}`} className="product-link">
                    <div className="product-image">
                      <img
                        src={product.imageUrl || product.images[0] || 'https://via.placeholder.com/300'}
                        alt={product.name}
                      />
                    </div>
                    <div className="product-info">
                      <h3 className="product-name">{product.name}</h3>
                      <div className="product-price">
                        {product.discountPrice ? (
                          <>
                            <span className="original-price">NT$ {product.price.toLocaleString()}</span>
                            <span className="discount-price">NT$ {product.discountPrice.toLocaleString()}</span>
                          </>
                        ) : (
                          <span className="current-price">NT$ {product.price.toLocaleString()}</span>
                        )}
                      </div>
                    </div>
                  </Link>
                </div>
              ))}
            </div>
          )}
          
          <div className="view-all-container">
            <Link to="/products" className="view-all-button">
              查看所有商品
            </Link>
          </div>
        </div>
      </section>
      
      {/* 促銷橫幅 */}
      <section className="promo-banner">
        <div className="container">
          <div className="banner-content">
            <h2 className="banner-title">限時優惠</h2>
            <p className="banner-text">使用優惠碼 DISCOUNT10 可享9折優惠</p>
            <Link to="/products" className="banner-button">
              立即選購
            </Link>
          </div>
        </div>
      </section>
      
      {/* 新品上架 */}
      <section className="new-arrivals-section">
        <div className="container">
          <h2 className="section-title">新品上架</h2>
          
          {loading ? (
            <div className="loading-container">
              <div className="loading-spinner"></div>
              <p>載入商品中...</p>
            </div>
          ) : error ? (
            <div className="error-message">
              {error}
            </div>
          ) : (
            <div className="new-arrivals-grid">
              {newArrivals.map(product => (
                <div key={product.id} className="product-card">
                  <Link to={`/products/${product.id}`} className="product-link">
                    <div className="product-image">
                      <img
                        src={product.imageUrl || product.images[0] || 'https://via.placeholder.com/300'}
                        alt={product.name}
                      />
                      <div className="product-badge">新品</div>
                    </div>
                    <div className="product-info">
                      <h3 className="product-name">{product.name}</h3>
                      <div className="product-price">
                        {product.discountPrice ? (
                          <>
                            <span className="original-price">NT$ {product.price.toLocaleString()}</span>
                            <span className="discount-price">NT$ {product.discountPrice.toLocaleString()}</span>
                          </>
                        ) : (
                          <span className="current-price">NT$ {product.price.toLocaleString()}</span>
                        )}
                      </div>
                    </div>
                  </Link>
                </div>
              ))}
            </div>
          )}
        </div>
      </section>
      
      {/* 服務特色 */}
      <section className="features-section">
        <div className="container">
          <div className="features-grid">
            <div className="feature-card">
              <div className="feature-icon">🚚</div>
              <h3 className="feature-title">免費配送</h3>
              <p className="feature-text">訂單滿NT$ 1,000即可享受免費配送</p>
            </div>
            
            <div className="feature-card">
              <div className="feature-icon">🔄</div>
              <h3 className="feature-title">輕鬆退換</h3>
              <p className="feature-text">7天內不滿意可退換貨</p>
            </div>
            
            <div className="feature-card">
              <div className="feature-icon">🔒</div>
              <h3 className="feature-title">安全支付</h3>
              <p className="feature-text">多種安全支付方式</p>
            </div>
            
            <div className="feature-card">
              <div className="feature-icon">💬</div>
              <h3 className="feature-title">專業客服</h3>
              <p className="feature-text">週一至週五 9:00-18:00 在線服務</p>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;