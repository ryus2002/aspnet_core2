import React, { useState, useEffect } from 'react';
import { Form, Input, Button, Radio, Steps, Card, Row, Col, Divider, message, Spin, Select } from 'antd';
import { ShoppingCartOutlined, CreditCardOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import './Checkout.css';

const { Step } = Steps;
const { Option } = Select;

/**
 * 結帳頁面元件
 * 
 * 提供配送地址填寫、支付方式選擇等功能
 * 
 * @returns {JSX.Element} 結帳頁面
 */
const Checkout = () => {
  // 載入狀態
  const [loading, setLoading] = useState(true);
  // 購物車商品
  const [cartItems, setCartItems] = useState([]);
  // 配送方式
  const [shippingMethod, setShippingMethod] = useState('standard');
  // 支付方式
  const [paymentMethod, setPaymentMethod] = useState('credit_card');
  // 地址表單
  const [form] = Form.useForm();
  // 訂單處理中
  const [processing, setProcessing] = useState(false);
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
          imageUrl: 'https://picsum.photos/200/200?random=1'
        },
        {
          id: 2,
          productId: 102,
          name: '超輕量筆記型電腦',
          variant: '15吋 / i7 / 16GB',
          price: 32999,
          quantity: 1,
          imageUrl: 'https://picsum.photos/200/200?random=2'
        },
        {
          id: 3,
          productId: 103,
          name: '智慧手錶',
          variant: '運動版',
          price: 4999,
          quantity: 2,
          imageUrl: 'https://picsum.photos/200/200?random=3'
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
   * 計算小計金額
   * 
   * @returns {number} 小計金額
   */
  const calculateSubtotal = () => {
    return cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  };

  /**
   * 計算運費
   * 
   * @returns {number} 運費
   */
  const calculateShippingFee = () => {
    const subtotal = calculateSubtotal();
    
    if (subtotal >= 20000) {
      return 0; // 免運費
    }
    
    return shippingMethod === 'standard' ? 100 : 200;
  };

  /**
   * 計算總計金額
   * 
   * @returns {number} 總計金額
   */
  const calculateTotal = () => {
    return calculateSubtotal() + calculateShippingFee();
  };

  /**
   * 處理表單提交
   * 
   * @param {object} values - 表單值
   */
  const handleSubmit = async (values) => {
    try {
      setProcessing(true);
      
      // 模擬 API 呼叫
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      console.log('訂單資料:', {
        shippingAddress: values,
        shippingMethod,
        paymentMethod,
        cartItems,
        subtotal: calculateSubtotal(),
        shippingFee: calculateShippingFee(),
        total: calculateTotal()
      });
      
      // 模擬訂單創建成功
      navigate('/order-success', { 
        state: { 
          orderNumber: 'ORD' + Date.now().toString().substring(3),
          total: calculateTotal()
        } 
      });
      
    } catch (error) {
      console.error('訂單處理失敗:', error);
      message.error('訂單處理失敗，請稍後再試');
    } finally {
      setProcessing(false);
    }
  };

  if (loading) {
    return (
      <div className="loading-container">
        <Spin size="large" />
      </div>
    );
  }

  return (
    <div className="checkout-container">
      <h1 className="page-title">結帳</h1>
      
      <Steps current={1} className="checkout-steps">
        <Step title="購物車" icon={<ShoppingCartOutlined />} />
        <Step title="結帳" icon={<CreditCardOutlined />} />
        <Step title="完成" icon={<CheckCircleOutlined />} />
      </Steps>
      
      <div className="checkout-content">
        <Row gutter={24}>
          <Col xs={24} md={16}>
            <Card title="配送資訊" className="checkout-card">
              <Form
                form={form}
                layout="vertical"
                onFinish={handleSubmit}
                initialValues={{
                  country: 'TW'
                }}
              >
                <Row gutter={16}>
                  <Col span={12}>
                    <Form.Item
                      name="name"
                      label="收件人姓名"
                      rules={[{ required: true, message: '請輸入收件人姓名' }]}
                    >
                      <Input placeholder="請輸入收件人姓名" />
                    </Form.Item>
                  </Col>
                  <Col span={12}>
                    <Form.Item
                      name="phone"
                      label="聯絡電話"
                      rules={[{ required: true, message: '請輸入聯絡電話' }]}
                    >
                      <Input placeholder="請輸入聯絡電話" />
                    </Form.Item>
                  </Col>
                </Row>
                
                <Form.Item
                  name="email"
                  label="電子郵件"
                  rules={[
                    { required: true, message: '請輸入電子郵件' },
                    { type: 'email', message: '請輸入有效的電子郵件' }
                  ]}
                >
                  <Input placeholder="請輸入電子郵件" />
                </Form.Item>
                
                <Form.Item
                  name="country"
                  label="國家/地區"
                  rules={[{ required: true, message: '請選擇國家/地區' }]}
                >
                  <Select>
                    <Option value="TW">台灣</Option>
                    <Option value="HK">香港</Option>
                    <Option value="JP">日本</Option>
                    <Option value="SG">新加坡</Option>
                  </Select>
                </Form.Item>
                
                <Row gutter={16}>
                  <Col span={12}>
                    <Form.Item
                      name="city"
                      label="縣市"
                      rules={[{ required: true, message: '請輸入縣市' }]}
                    >
                      <Input placeholder="請輸入縣市" />
                    </Form.Item>
                  </Col>
                  <Col span={12}>
                    <Form.Item
                      name="postalCode"
                      label="郵遞區號"
                      rules={[{ required: true, message: '請輸入郵遞區號' }]}
                    >
                      <Input placeholder="請輸入郵遞區號" />
                    </Form.Item>
                  </Col>
                </Row>
                
                <Form.Item
                  name="address"
                  label="詳細地址"
                  rules={[{ required: true, message: '請輸入詳細地址' }]}
                >
                  <Input placeholder="請輸入詳細地址" />
                </Form.Item>
                
                <Form.Item
                  name="notes"
                  label="訂單備註"
                >
                  <Input.TextArea placeholder="有什麼需要告訴我們的嗎？" rows={3} />
                </Form.Item>
                
                <Divider />
                
                <h3>配送方式</h3>
                <Radio.Group
                  value={shippingMethod}
                  onChange={(e) => setShippingMethod(e.target.value)}
                  className="shipping-method-group"
                >
                  <Radio value="standard" className="shipping-method-option">
                    <div className="shipping-method-info">
                      <div className="shipping-method-name">標準配送</div>
                      <div className="shipping-method-description">3-5 個工作天</div>
                    </div>
                    <div className="shipping-method-price">
                      {calculateSubtotal() >= 20000 ? '免運費' : 'NT$ 100'}
                    </div>
                  </Radio>
                  <Radio value="express" className="shipping-method-option">
                    <div className="shipping-method-info">
                      <div className="shipping-method-name">快速配送</div>
                      <div className="shipping-method-description">1-2 個工作天</div>
                    </div>
                    <div className="shipping-method-price">
                      {calculateSubtotal() >= 20000 ? '免運費' : 'NT$ 200'}
                    </div>
                  </Radio>
                </Radio.Group>
                
                <Divider />
                
                <h3>支付方式</h3>
                <Radio.Group
                  value={paymentMethod}
                  onChange={(e) => setPaymentMethod(e.target.value)}
                  className="payment-method-group"
                >
                  <Radio value="credit_card" className="payment-method-option">
                    <div className="payment-method-info">
                      <div className="payment-method-name">信用卡</div>
                      <div className="payment-method-description">支援 Visa、Mastercard、JCB</div>
                    </div>
                    <div className="payment-method-icon credit-card-icon"></div>
                  </Radio>
                  <Radio value="line_pay" className="payment-method-option">
                    <div className="payment-method-info">
                      <div className="payment-method-name">LINE Pay</div>
                      <div className="payment-method-description">使用 LINE Pay 電子錢包付款</div>
                    </div>
                    <div className="payment-method-icon line-pay-icon"></div>
                  </Radio>
                </Radio.Group>
                
                <div className="checkout-buttons">
                  <Button
                    type="default"
                    size="large"
                    onClick={() => navigate('/cart')}
                    className="back-to-cart-button"
                  >
                    返回購物車
                  </Button>
                  <Button
                    type="primary"
                    size="large"
                    htmlType="submit"
                    loading={processing}
                    className="place-order-button"
                  >
                    確認下單
                  </Button>
                </div>
              </Form>
            </Card>
          </Col>
          
          <Col xs={24} md={8}>
            <Card title="訂單摘要" className="order-summary-card">
              <div className="order-items">
                {cartItems.map(item => (
                  <div key={item.id} className="order-item">
                    <div className="order-item-image">
                      <img src={item.imageUrl} alt={item.name} />
                      <span className="order-item-quantity">{item.quantity}</span>
                    </div>
                    <div className="order-item-details">
                      <div className="order-item-name">{item.name}</div>
                      <div className="order-item-variant">{item.variant}</div>
                    </div>
                    <div className="order-item-price">
                      NT$ {(item.price * item.quantity).toLocaleString()}
                    </div>
                  </div>
                ))}
              </div>
              
              <Divider className="summary-divider" />
              
              <div className="summary-item">
                <span>小計</span>
                <span>NT$ {calculateSubtotal().toLocaleString()}</span>
              </div>
              
              <div className="summary-item">
                <span>運費</span>
                <span>
                  {calculateShippingFee() === 0
                    ? '免運費'
                    : `NT$ ${calculateShippingFee().toLocaleString()}`}
                </span>
              </div>
              
              <Divider className="summary-divider" />
              
              <div className="summary-item total">
                <span>總計</span>
                <span>NT$ {calculateTotal().toLocaleString()}</span>
              </div>
            </Card>
          </Col>
        </Row>
      </div>
    </div>
  );
};

export default Checkout;