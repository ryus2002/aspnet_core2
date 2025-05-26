import React, { useState, useEffect } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { paymentService } from '../services/paymentService';
import './PaymentMethodSelect.css';

/**
 * 支付方式選擇頁面
 * 用於顯示可用的支付方式並讓用戶進行選擇
 */
const PaymentMethodSelect = () => {
  const [paymentMethods, setPaymentMethods] = useState([]);
  const [selectedMethod, setSelectedMethod] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();
  const { orderId } = useParams();
  const location = useLocation();
  const orderDetails = location.state?.orderDetails || {};

  // 載入支付方式
  useEffect(() => {
    const fetchPaymentMethods = async () => {
      try {
        setLoading(true);
        // 使用模擬支付API獲取支付方式
        const methods = await paymentService.getPaymentMethods();
        setPaymentMethods(methods);
        // 如果有支付方式，默認選擇第一個
        if (methods.length > 0) {
          setSelectedMethod(methods[0].id);
        }
      } catch (err) {
        console.error('獲取支付方式失敗:', err);
        setError('無法載入支付方式，請稍後再試。');
      } finally {
        setLoading(false);
      }
    };

    fetchPaymentMethods();
  }, []);

  /**
   * 處理支付方式選擇
   * @param {string} methodId - 支付方式ID
   */
  const handleMethodSelect = (methodId) => {
    setSelectedMethod(methodId);
  };

  /**
   * 處理支付提交
   * 創建支付交易並跳轉到支付頁面
   */
  const handleSubmit = async () => {
    if (!selectedMethod) {
      setError('請選擇一種支付方式');
      return;
    }

    try {
      setLoading(true);
      // 創建支付交易
      const paymentData = {
        orderId: orderId,
        paymentMethodId: selectedMethod,
        amount: orderDetails.totalAmount,
        currency: 'TWD',
        description: `訂單 ${orderId} 的支付`,
        successUrl: `/orders/${orderId}/success`,
        failureUrl: `/orders/${orderId}/failed`
      };

      const transaction = await paymentService.createPayment(paymentData);
      
      // 跳轉到模擬支付頁面
      navigate(`/payment/${transaction.id}/process`, { 
        state: { 
          transaction,
          orderDetails
        }
      });
    } catch (err) {
      console.error('創建支付交易失敗:', err);
      setError('無法處理支付請求，請稍後再試。');
      setLoading(false);
    }
  };

  if (loading && paymentMethods.length === 0) {
    return (
      <div className="payment-method-container">
        <div className="loading-spinner">載入中...</div>
      </div>
    );
  }

  return (
    <div className="payment-method-container">
      <h1>選擇支付方式</h1>
      
      {error && <div className="error-message">{error}</div>}
      
      <div className="order-summary">
        <h2>訂單摘要</h2>
        <p><strong>訂單編號:</strong> {orderId}</p>
        <p><strong>總金額:</strong> NT$ {orderDetails.totalAmount?.toFixed(2)}</p>
      </div>

      <div className="payment-methods">
        <h2>可用的支付方式</h2>
        {paymentMethods.length === 0 ? (
          <p>目前沒有可用的支付方式</p>
        ) : (
          <div className="method-list">
            {paymentMethods.map((method) => (
              <div 
                key={method.id} 
                className={`method-item ${selectedMethod === method.id ? 'selected' : ''}`}
                onClick={() => handleMethodSelect(method.id)}
              >
                <div className="method-icon">
                  {method.iconUrl ? (
                    <img src={method.iconUrl} alt={method.name} />
                  ) : (
                    <div className="default-icon">{method.name.charAt(0)}</div>
                  )}
                </div>
                <div className="method-info">
                  <h3>{method.name}</h3>
                  <p>{method.description}</p>
                </div>
                <div className="method-select">
                  <input
                    type="radio"
                    name="paymentMethod"
                    checked={selectedMethod === method.id}
                    onChange={() => handleMethodSelect(method.id)}
                  />
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="payment-actions">
        <button 
          className="back-button"
          onClick={() => navigate(-1)}
        >
          返回
        </button>
        <button 
          className="proceed-button"
          onClick={handleSubmit}
          disabled={!selectedMethod || loading}
        >
          {loading ? '處理中...' : '繼續支付'}
        </button>
      </div>
    </div>
  );
};

export default PaymentMethodSelect;