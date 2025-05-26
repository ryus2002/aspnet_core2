import React from 'react';
import { useParams, useLocation, useNavigate } from 'react-router-dom';
import './OrderResult.css';

/**
 * 訂單失敗頁面
 * 顯示訂單支付失敗的信息
 */
const OrderFailed = () => {
  const { orderId } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const paymentResult = location.state?.paymentResult;

  const handleRetryPayment = () => {
    navigate(`/orders/${orderId}/payment`);
  };

  const handleContinueShopping = () => {
    navigate('/products');
  };

  return (
    <div className="order-result-container failed">
      <div className="result-icon failed">
        <i className="fa fa-times-circle"></i>
      </div>
      
      <h1>支付處理失敗</h1>
      
      <div className="order-info">
        <p>很抱歉，您的訂單支付處理失敗。</p>
        <p className="order-number">訂單編號: <strong>{orderId}</strong></p>
        
        {paymentResult && (
          <div className="payment-details">
            <h3>支付詳情</h3>
            <div className="detail-item">
              <span className="label">交易編號:</span>
              <span className="value">{paymentResult.transactionId}</span>
            </div>
            <div className="detail-item">
              <span className="label">失敗原因:</span>
              <span className="value error-text">{paymentResult.errorMessage || '支付處理失敗'}</span>
            </div>
          </div>
        )}
        
        <div className="next-steps">
          <p>您可以重新嘗試支付，或選擇其他支付方式。如果問題持續存在，請聯繫客戶服務。</p>
        </div>
      </div>
      
      <div className="result-actions">
        <button className="primary-button" onClick={handleRetryPayment}>
          重新支付
        </button>
        <button className="secondary-button" onClick={handleContinueShopping}>
          繼續購物
        </button>
      </div>
    </div>
  );
};

export default OrderFailed;