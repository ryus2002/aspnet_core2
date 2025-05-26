import React, { useEffect } from 'react';
import { useParams, useLocation, useNavigate } from 'react-router-dom';
import { orderService } from '../services/orderService';
import './OrderResult.css';

/**
 * 訂單成功頁面
 * 顯示訂單支付成功的信息
 */
const OrderSuccess = () => {
  const { orderId } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const paymentResult = location.state?.paymentResult;
  
  // 獲取訂單詳情
  useEffect(() => {
    const fetchOrderDetails = async () => {
      try {
        // 獲取訂單詳情，可以在這裡添加額外的邏輯
        await orderService.getOrderByNumber(orderId);
      } catch (error) {
        console.error('獲取訂單詳情失敗:', error);
      }
    };

    fetchOrderDetails();
  }, [orderId]);

  const handleContinueShopping = () => {
    navigate('/products');
  };

  const handleViewOrder = () => {
    navigate(`/account/orders/${orderId}`);
  };

  return (
    <div className="order-result-container success">
      <div className="result-icon success">
        <i className="fa fa-check-circle"></i>
      </div>
      
      <h1>訂單支付成功！</h1>
      
      <div className="order-info">
        <p>感謝您的購買，您的訂單已成功付款。</p>
        <p className="order-number">訂單編號: <strong>{orderId}</strong></p>
        
        {paymentResult && (
          <div className="payment-details">
            <h3>支付詳情</h3>
            <div className="detail-item">
              <span className="label">交易編號:</span>
              <span className="value">{paymentResult.transactionId}</span>
            </div>
            <div className="detail-item">
              <span className="label">支付金額:</span>
              <span className="value">NT$ {paymentResult.amount?.toFixed(2)}</span>
            </div>
            <div className="detail-item">
              <span className="label">支付時間:</span>
              <span className="value">
                {paymentResult.completedAt 
                  ? new Date(paymentResult.completedAt).toLocaleString('zh-TW') 
                  : new Date().toLocaleString('zh-TW')}
              </span>
            </div>
          </div>
        )}
        
        <div className="next-steps">
          <p>我們已經發送訂單確認郵件到您的郵箱。您可以在「我的訂單」中查看訂單詳情。</p>
        </div>
      </div>
      
      <div className="result-actions">
        <button className="primary-button" onClick={handleContinueShopping}>
          繼續購物
        </button>
        <button className="secondary-button" onClick={handleViewOrder}>
          查看訂單
        </button>
      </div>
    </div>
  );
};

export default OrderSuccess;