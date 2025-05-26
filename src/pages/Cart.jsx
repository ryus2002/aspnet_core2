import React, { useState, useEffect } from 'react';
import { Table, Button, InputNumber, Empty, Steps, Card, Input, Row, Col, Divider, message, Spin } from 'antd';
import { ShoppingOutlined, DeleteOutlined, RightOutlined, LeftOutlined, ShoppingCartOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import { Link, useNavigate } from 'react-router-dom';
import './Cart.css';

const { Step } = Steps;

/**
 * 購物車頁面元件
 * 
 * 顯示購物車內容，提供商品數量調整、移除商品、應用優惠碼等功能
 * 
 * @returns {JSX.Element} 購物車頁面
 */
const Cart = () => {
  // 購物車商品
  const [cartItems, setCartItems] = useState([]);
  // 載入狀態
  const [loading, setLoading] = useState(true);
  // 優惠碼
  const [promoCode, setPromoCode] = useState('');
  // 折扣金額
  const [discount, setDiscount] = useState(0);
  // 優惠碼應用中狀態
  const [applyingPromo, setApplyingPromo] = useState(false);
  // 導航
  const navigate = useNavigate();

  /**
   * 元件載入時獲取購物車資料
   */
  useEffect(() => {
    fetchCartItems();
  }, []);

  /**
   * 從 API 獲取購物車資料
   */
  const fetchCartItems = async () => {
    try {
      setLoading(true);
      
      // 模擬 API 呼叫
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // 模擬購物車資料
      const mockCartItems = [
        {
          id: 1,
          productId: 101,
          name: '高品質藍牙耳機',
          variant: '黑色',
          price: 1299,
          quantity: 1,
          imageUrl: 'https://picsum.photos/200/200?random=1',
          inStock: true,
          stockQuantity: 10
        },
        {
          id: 2,
          productId: 102,
          name: '超輕量筆記型電腦',
          variant: '15吋 / i7 / 16GB',
          price: 32999,
          quantity: 1,
          imageUrl: 'https://picsum.photos/200/200?random=2',
          inStock: true,
          stockQuantity: 5
        },
        {
          id: 3,
          productId: 103,
          name: '智慧手錶',
          variant: '運動版',
          price: 4999,
          quantity: 2,
          imageUrl: 'https://picsum.photos/200/200?random=3',
          inStock: true,
          stockQuantity: 8
        }
      ];
      
      setCartItems(mockCartItems);
    } catch (error) {
      console.error('獲取購物車失敗:', error);
      message.error('無法載入購物車，請稍後再試');
    } finally {
      setLoading(false);
    }
  };

  /**
   * 處理數量變更
   * 
   * @param {number} itemId - 購物車項目 ID
   * @param {number} quantity - 新數量
   */
  const handleQuantityChange = async (itemId, quantity) => {
    try {
      // 模擬 API 呼叫
      await new Promise(resolve => setTimeout(resolve, 300));
      
      setCartItems(cartItems.map(item => 
        item.id === itemId ? { ...item, quantity } : item
      ));
      
      message.success('購物車已更新');
    } catch (error) {
      message.error('更新數量失敗，請稍後再試');
    }
  };

  /**
   * 處理移除商品
   * 
   * @param {number} itemId - 購物車項目 ID
   */
  const handleRemoveItem = async (itemId) => {
    try {
      // 模擬 API 呼叫
      await new Promise(resolve => setTimeout(resolve, 300));
      
      setCartItems(cartItems.filter(item => item.id !== itemId));
      message.success('商品已從購物車移除');
    } catch (error) {
      message.error('移除商品失敗，請稍後再試');
    }
  };

  /**
   * 處理應用優惠碼
   */
  const handleApplyPromoCode = async () => {
    if (!promoCode.trim()) {
      message.warning('請輸入優惠碼');
      return;
    }

    try {
      setApplyingPromo(true);
      
      // 模擬 API 呼叫
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      // 模擬優惠碼驗證
      if (promoCode.toUpperCase() === 'DISCOUNT20') {
        const discountAmount = Math.round(calculateSubtotal() * 0.2);
        setDiscount(discountAmount);
        message.success(`優惠碼套用成功，已折抵 NT$ ${discountAmount.toLocaleString()}`);
      } else if (promoCode.toUpperCase() === 'NEWYEAR') {
        const discountAmount = 500;
        setDiscount(discountAmount);
        message.success(`優惠碼套用成功，已折抵 NT$ ${discountAmount.toLocaleString()}`);
      } else {
        message.error('優惠碼無效或已過期');
      }
    } catch (error) {
      message.error('套用優惠碼失敗，請稍後再試');
    } finally {
      setApplyingPromo(false);
    }
  };

  /**
   * 處理結帳
   */
  const handleCheckout = () => {
    navigate('/checkout');
  };

  /**
   * 計算小計金額
   * 
   * @returns {number} 小計金額
   */
  const calculateSubtotal = () => {
    return cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  };

  /**
   * 計算總計金額
   * 
   * @returns {number} 總計金額
   */
  const calculateTotal = () => {
    return Math.max(0, calculateSubtotal() - discount);
  };

  /**
   * 表格欄位定義
   */
  const columns = [
    {
      title: '商品',
      dataIndex: 'product',
      key: 'product',
      render: (_, record) => (
        <div className="cart-product">
          <img src={record.imageUrl} alt={record.name} className="cart-product-image" />
          <div className="cart-product-info">
            <Link to={`/products/${record.productId}`} className="cart-product-name">
              {record.name}
            </Link>
            {record.variant && <div className="cart-product-variant">款式: {record.variant}</div>}
            {!record.inStock && <div className="out-of-stock-warning">
              <ExclamationCircleOutlined /> 缺貨
            </div>}
          </div>
        </div>
      ),
    },
    {
      title: '單價',
      dataIndex: 'price',
      key: 'price',
      render: (price) => <span className="cart-price">NT$ {price.toLocaleString()}</span>,
    },
    {
      title: '數量',
      key: 'quantity',
      render: (_, record) => (
        <InputNumber
          min={1}
          max={record.inStock ? record.stockQuantity : record.quantity}
          value={record.quantity}
          onChange={(value) => handleQuantityChange(record.id, value)}
          disabled={!record.inStock}
          className="cart-quantity-input"
        />
      ),
    },
    {
      title: '小計',
      key: 'subtotal',
      render: (_, record) => (
        <span className="cart-subtotal">
          NT$ {(record.price * record.quantity).toLocaleString()}
        </span>
      ),
    },
    {
      title: '',
      key: 'action',
      render: (_, record) => (
        <Button
          type="text"
          danger
          icon={<DeleteOutlined />}
          onClick={() => handleRemoveItem(record.id)}
          className="cart-remove-btn"
        >
          移除
        </Button>
      ),
    },
  ];

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
      </div>
    );
  }

  if (cartItems.length === 0) {
    return (
      <div className="empty-cart-container">
        <Empty
          image={Empty.PRESENTED_IMAGE_SIMPLE}
          description="您的購物車是空的"
          className="empty-cart"
        >
          <Button type="primary" icon={<ShoppingOutlined />} size="large" className="shop-now-btn">
            <Link to="/products">去購物</Link>
          </Button>
        </Empty>
      </div>
    );
  }

  return (
    <div className="cart-container">
      <h1 className="page-title">購物車</h1>
      
      <Steps current={0} className="checkout-steps">
        <Step title="購物車" icon={<ShoppingCartOutlined />} />
        <Step title="結帳" />
        <Step title="完成" />
      </Steps>
      
      <div className="cart-content">
        <Table
          columns={columns}
          dataSource={cartItems}
          pagination={false}
          rowKey="id"
          locale={{ emptyText: '購物車是空的' }}
          className="cart-table"
        />
        
        <Row gutter={24} className="cart-summary-row">
          <Col xs={24} md={16}>
            <Card title="優惠碼" className="promo-code-card">
              <div className="promo-code-input">
                <Input
                  placeholder="輸入優惠碼"
                  value={promoCode}
                  onChange={(e) => setPromoCode(e.target.value)}
                  className="promo-input"
                />
                <Button
                  type="primary"
                  onClick={handleApplyPromoCode}
                  loading={applyingPromo}
                  className="apply-promo-btn"
                >
                  套用
                </Button>
              </div>
              <div className="promo-tips">
                <p>輸入優惠碼 DISCOUNT20 可享有 8 折優惠</p>
                <p>輸入優惠碼 NEWYEAR 可折抵 NT$ 500</p>
              </div>
            </Card>
            
            <div className="continue-shopping-container">
              <Button type="link" className="continue-shopping-btn">
                <Link to="/products">
                  <LeftOutlined /> 繼續購物
                </Link>
              </Button>
            </div>
          </Col>
          
          <Col xs={24} md={8}>
            <Card title="訂單摘要" className="order-summary-card">
              <div className="summary-item">
                <span>小計</span>
                <span>NT$ {calculateSubtotal().toLocaleString()}</span>
              </div>
              
              {discount > 0 && (
                <div className="summary-item discount">
                  <span>折扣</span>
                  <span>-NT$ {discount.toLocaleString()}</span>
                </div>
              )}
              
              <Divider className="summary-divider" />
              
              <div className="summary-item total">
                <span>總計</span>
                <span>NT$ {calculateTotal().toLocaleString()}</span>
              </div>
              
              <div className="cart-buttons">
                <Button
                  type="primary"
                  size="large"
                  block
                  onClick={handleCheckout}
                  className="checkout-button"
                >
                  前往結帳 <RightOutlined />
                </Button>
              </div>
              
              <div className="payment-methods">
                <p>支援付款方式：</p>
                <div className="payment-icons">
                  <i className="payment-icon credit-card"></i>
                  <i className="payment-icon paypal"></i>
                  <i className="payment-icon apple-pay"></i>
                  <i className="payment-icon line-pay"></i>
                </div>
              </div>
            </Card>
          </Col>
        </Row>
      </div>
    </div>
  );
};

export default Cart;