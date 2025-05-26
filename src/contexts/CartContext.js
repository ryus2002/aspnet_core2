/**
 * 購物車上下文
 * 管理購物車狀態，提供添加、更新、移除商品功能
 */
import React, { createContext, useState, useEffect, useContext } from 'react';
import { getCartItems, addToCart as apiAddToCart, updateCartItem as apiUpdateCartItem, removeCartItem as apiRemoveCartItem } from '../services/cartService';
import { useAuth } from './AuthContext';

// 創建購物車上下文
const CartContext = createContext();

// 購物車提供者組件
export const CartProvider = ({ children }) => {
  const [cartItems, setCartItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [cartTotal, setCartTotal] = useState(0);
  const [cartCount, setCartCount] = useState(0);
  const { isAuthenticated } = useAuth();

  // 在組件掛載時或認證狀態變更時獲取購物車數據
  useEffect(() => {
    if (isAuthenticated) {
      fetchCartItems();
    } else {
      // 從本地存儲加載匿名購物車
      loadLocalCart();
    }
  }, [isAuthenticated]);

  /**
   * 從API獲取購物車數據
   */
  const fetchCartItems = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getCartItems();
      setCartItems(response.items || []);
      calculateCartSummary(response.items || []);
    } catch (err) {
      console.error('獲取購物車數據失敗:', err);
      setError('無法載入購物車，請稍後再試');
    } finally {
      setLoading(false);
    }
  };

  /**
   * 從本地存儲加載匿名購物車
   */
  const loadLocalCart = () => {
    try {
      const localCart = localStorage.getItem('cart');
      if (localCart) {
        const parsedCart = JSON.parse(localCart);
        setCartItems(parsedCart);
        calculateCartSummary(parsedCart);
      } else {
        setCartItems([]);
        calculateCartSummary([]);
      }
    } catch (err) {
      console.error('加載本地購物車失敗:', err);
      setCartItems([]);
      calculateCartSummary([]);
    }
  };

  /**
   * 保存購物車到本地存儲
   * @param {Array} items - 購物車商品列表
   */
  const saveLocalCart = (items) => {
    try {
      localStorage.setItem('cart', JSON.stringify(items));
    } catch (err) {
      console.error('保存本地購物車失敗:', err);
    }
  };

  /**
   * 計算購物車總價和商品數量
   * @param {Array} items - 購物車商品列表
   */
  const calculateCartSummary = (items) => {
    const total = items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    const count = items.reduce((sum, item) => sum + item.quantity, 0);
    setCartTotal(total);
    setCartCount(count);
  };

  /**
   * 添加商品到購物車
   * @param {object} product - 商品信息
   * @param {number} quantity - 數量
   * @returns {Promise} 添加結果
   */
  const addToCart = async (product, quantity = 1) => {
    try {
      setError(null);
      
      if (isAuthenticated) {
        // 已登入用戶，調用API
        const response = await apiAddToCart({
          productId: product.id,
          quantity: quantity,
          variantId: product.variantId
        });
        
        await fetchCartItems(); // 重新獲取購物車數據
        return response;
      } else {
        // 匿名用戶，使用本地存儲
        const existingItemIndex = cartItems.findIndex(
          item => item.productId === product.id && 
                 (item.variantId === product.variantId || 
                 (!item.variantId && !product.variantId))
        );
        
        let updatedCart;
        
        if (existingItemIndex >= 0) {
          // 商品已存在，更新數量
          updatedCart = [...cartItems];
          updatedCart[existingItemIndex].quantity += quantity;
        } else {
          // 添加新商品
          const newItem = {
            id: Date.now().toString(), // 臨時ID
            productId: product.id,
            name: product.name,
            price: product.price,
            imageUrl: product.imageUrl,
            quantity: quantity,
            variantId: product.variantId,
            variant: product.variant
          };
          updatedCart = [...cartItems, newItem];
        }
        
        setCartItems(updatedCart);
        calculateCartSummary(updatedCart);
        saveLocalCart(updatedCart);
        
        return { success: true };
      }
    } catch (err) {
      setError(err.message || '添加商品失敗，請稍後再試');
      throw err;
    }
  };

  /**
   * 更新購物車商品數量
   * @param {string} itemId - 購物車項目ID
   * @param {number} quantity - 新數量
   * @returns {Promise} 更新結果
   */
  const updateCartItem = async (itemId, quantity) => {
    try {
      setError(null);
      
      if (isAuthenticated) {
        // 已登入用戶，調用API
        const response = await apiUpdateCartItem(itemId, quantity);
        await fetchCartItems(); // 重新獲取購物車數據
        return response;
      } else {
        // 匿名用戶，使用本地存儲
        const updatedCart = cartItems.map(item => 
          item.id === itemId ? { ...item, quantity } : item
        );
        
        setCartItems(updatedCart);
        calculateCartSummary(updatedCart);
        saveLocalCart(updatedCart);
        
        return { success: true };
      }
    } catch (err) {
      setError(err.message || '更新商品數量失敗，請稍後再試');
      throw err;
    }
  };

  /**
   * 從購物車移除商品
   * @param {string} itemId - 購物車項目ID
   * @returns {Promise} 移除結果
   */
  const removeCartItem = async (itemId) => {
    try {
      setError(null);
      
      if (isAuthenticated) {
        // 已登入用戶，調用API
        const response = await apiRemoveCartItem(itemId);
        await fetchCartItems(); // 重新獲取購物車數據
        return response;
      } else {
        // 匿名用戶，使用本地存儲
        const updatedCart = cartItems.filter(item => item.id !== itemId);
        
        setCartItems(updatedCart);
        calculateCartSummary(updatedCart);
        saveLocalCart(updatedCart);
        
        return { success: true };
      }
    } catch (err) {
      setError(err.message || '移除商品失敗，請稍後再試');
      throw err;
    }
  };

  /**
   * 清空購物車
   * @returns {Promise} 清空結果
   */
  const clearCart = async () => {
    try {
      setError(null);
      
      if (isAuthenticated) {
        // 已登入用戶，這裡需要實現清空購物車的API
        // 暫時使用本地方法
        setCartItems([]);
        calculateCartSummary([]);
      } else {
        // 匿名用戶，使用本地存儲
        setCartItems([]);
        calculateCartSummary([]);
        saveLocalCart([]);
      }
      
      return { success: true };
    } catch (err) {
      setError(err.message || '清空購物車失敗，請稍後再試');
      throw err;
    }
  };

  // 提供購物車相關的狀態和方法
  const value = {
    cartItems,
    cartTotal,
    cartCount,
    loading,
    error,
    addToCart,
    updateCartItem,
    removeCartItem,
    clearCart,
    refreshCart: fetchCartItems
  };

  return (
    <CartContext.Provider value={value}>
      {children}
    </CartContext.Provider>
  );
};

// 自定義Hook，方便在組件中使用購物車上下文
export const useCart = () => {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart必須在CartProvider內部使用');
  }
  return context;
};