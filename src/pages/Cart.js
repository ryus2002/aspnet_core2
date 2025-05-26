/**
 * 購物車頁面
 * 顯示購物車中的商品，並提供數量調整、移除商品和結帳功能
 */
import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useCart } from '../contexts/CartContext';
import { useAuth } from '../contexts/AuthContext';
import './Cart.css';

const Cart = () => {
  // 購物車上下文
  const { cartItems, cartTotal, loading, error, updateCartItem, removeCartItem, clearCart } = useCart();
  
  // 認證上下文
  const { isAuthenticated } = useAuth();
  
  // 優惠碼狀態
  const [promoCode, setPromoCode] = useState('');
  const [discount, setDiscount] = useState(0);
  const [applyingPromo, setApplyingPromo] = useState(false);
  const [promoError, setPromoError] = useState('');
  
  // 路由導航
  const navigate = useNavigate();

  /**
   * 處理數量變更
   * @param {string} itemId - 購物車項目ID
   * @param {number} quantity - 新數量
   */
  const handleQuantityChange = async (itemId, quantity) => {
    if (quantity < 1) return;
    
    try {
      await updateCartItem(itemId, quantity);
    } catch (err) {
      console.error('更新數量失敗:', err);
      alert('更新數量失敗，請稍後再試');
    }
  };

  /**
   * 處理移除商品
   * @param {string} itemId - 購物車項目ID
   */
  const handleRemoveItem = async (itemId) => {
    try {
      await removeCartItem(itemId);
    } catch (err) {
      console.error('移除商品失敗:', err);
      alert('移除商品失敗，請稍後再試');
    }
  };

  /**
   * 處理清空購物車
   */
  const handleClearCart = async () => {
    if (window.confirm('確定要清空購物車嗎？')) {
      try {
        await clearCart();
      } catch (err) {
        console.error('清空購物車失敗:', err);
        alert('清空購物車失敗，請稍後再試');
      }
    }
  };

  /**
   * 處理應用優惠碼
   */
  const handleApplyPromoCode = async () => {
    if (!promoCode.trim()) {
      setPromoError('請輸入優惠碼');
      return;
    }
    
    try {
      setApplyingPromo(true);
      setPromoError('');
      
      // 這裡應該調用API來驗證優惠碼
      // 暫時使用模擬數據
      setTimeout(() => {
        if (promoCode.toUpperCase() === 'DISCOUNT10') {
          const discountAmount = cartTotal * 0.1;
          setDiscount(discountAmount);
          alert(`優惠碼套用成功，已折抵 NT$ ${discountAmount.toLocaleString()}`);
        } else {
          setPromoError('無效的優惠碼');
        }
        setApplyingPromo(false);
      }, 1000);
    } catch (err) {
      console.error('應用優惠碼失敗:', err);
      setPromoError('應用優惠碼失敗，請稍後再試');
      setApplyingPromo(false);
    }
  };

  /**
   * 處理結帳
   */
  const handleCheckout = () => {
    if (!isAuthenticated) {
      // 如果用戶未登入，導航到登入頁面，並設置重定向回購物車
      navigate('/login', { state: { from: { pathname: '/cart' } } });
      return;
    }
    
    // 導航到結帳頁面
    navigate('/checkout');
  };

  /**
   * 計算總金額
   * @returns {number} 總金額
   */
  const calculateTotal = () => {
    return cartTotal - discount;
  };

  // 載入中狀態
  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>載入購物車中...</p>
      </div>
    );
  }

  // 購物車為空
  if (cartItems.length === 0) {
    return (
      <div className="cart-page">
        <div className="container">
          <h1 className="page-title">購物車</h1>
          
          <div className="empty-cart">
            <div className="empty-cart-icon">🛒</div>
            <h2>您的購物車是空的</h2>
            <p>看起來您還沒有將任何商品加入購物車</p>
            <Link to="/products" className="continue-shopping-button">
              繼續購物
            </Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="cart-page">
      <div className="container">
        <h1 className="page-title">購物車</h1>
        
        {error && (
          <div className="error-message">
            {error}
          </div>
        )}
        
        <div className="cart-content">
          <div className="cart-items">
            <div className="cart-header">
              <div className="cart-header-product">商品</div>
              <div className="cart-header-price">單價</div>
              <div className="cart-header-quantity">數量</div>
              <div className="cart-header-subtotal">小計</div>
              <div className="cart-header-action">操作</div>
            </div>
            
            {cartItems.map(item => (
              <div key={item.id} className="cart-item">
                <div className="cart-item-product">
                  <div className="cart-item-image">
                    <Link to={`/products/${item.productId}`}>
                      <img src={item.imageUrl || 'https://via.placeholder.com/80'} alt={item.name} />
                    </Link>
                  </div>
                  <div className="cart-item-details">
                    <Link to={`/products/${item.productId}`} className="cart-item-name">
                      {item.name}
                    </Link>
                    {item.variant && (
                      <div className="cart-item-variant">
                        款式: {item.variant}
                      </div>
                    )}
                    {!item.inStock && (
                      <div className="out-of-stock-warning">
                        缺貨
                      </div>
                    )}
                  </div>
                </div>
                
                <div className="cart-item-price">
                  NT$ {item.price.toLocaleString()}
                </div>
                
                <div className="cart-item-quantity">
                  <div className="quantity-controls">
                    <button
                      className="quantity-button minus"
                      onClick={() => handleQuantityChange(item.id, item.quantity - 1)}
                      disabled={item.quantity <= 1}
                    >
                      -
                    </button>
                    <input
                      type="number"
                      min="1"
                      value={item.quantity}
                      onChange={(e) => handleQuantityChange(item.id, parseInt(e.target.value) || 1)}
                      className="quantity-input"
                    />
                    <button
                      className="quantity-button plus"
                      onClick={() => handleQuantityChange(item.id, item.quantity + 1)}
                    >
                      +
                    </button>
                  </div>
                </div>
                
                <div className="cart-item-subtotal">
                  NT$ {(item.price * item.quantity).toLocaleString()}
                </div>
                
                <div className="cart-item-action">
                  <button
                    className="remove-item-button"
                    onClick={() => handleRemoveItem(item.id)}
                  >
                    移除
                  </button>
                </div>
              </div>
            ))}
            
            <div className="cart-actions">
              <Link to="/products" className="continue-shopping-link">
                ← 繼續購物
              </Link>
              <button className="clear-cart-button" onClick={handleClearCart}>
                清空購物車
              </button>
            </div>
          </div>
          
          <div className="cart-summary">
            <h2 className="summary-title">訂單摘要</h2>
            
            <div className="promo-code-section">
              <h3>優惠碼</h3>
              <div className="promo-code-input">
                <input
                  type="text"
                  placeholder="輸入優惠碼"
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value)}
                  disabled={applyingPromo}
                />
                <button
                  className="apply-promo-button"
                  onClick={handleApplyPromoCode}
                  disabled={applyingPromo}
                >
                  {applyingPromo ? '套用中...' : '套用'}
                </button>
              </div>
              {promoError && (
                <div className="promo-error">
                  {promoError}
                </div>
              )}
            </div>
            
            <div className="summary-details">
              <div className="summary-row">
                <span>小計</span>
                <span>NT$ {cartTotal.toLocaleString()}</span>
              </div>
              
              {discount > 0 && (
                <div className="summary-row discount">
                  <span>折扣</span>
                  <span>-NT$ {discount.toLocaleString()}</span>
                </div>
              )}
              
              <div className="summary-row shipping">
                <span>運費</span>
                <span>免費</span>
              </div>
              
              <div className="summary-row total">
                <span>總計</span>
                <span>NT$ {calculateTotal().toLocaleString()}</span>
              </div>
            </div>
            
            <button
              className="checkout-button"
              onClick={handleCheckout}
            >
              前往結帳
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Cart;