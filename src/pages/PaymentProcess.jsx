import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { paymentService } from '../services/paymentService';
import './PaymentProcess.css';

/**
 * 模擬支付處理頁面
 * 用於模擬支付流程並顯示支付結果
 */
const PaymentProcess = () => {
  const [transaction, setTransaction] = useState(null);
  const [paymentMethod, setPaymentMethod] = useState(null);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState(false);
  const [error, setError] = useState(null);
  const [result, setResult] = useState(null);
  const { transactionId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const initialTransaction = location.state?.transaction;
  const orderDetails = location.state?.orderDetails || {};

  // 載入交易信息
  useEffect(() => {
    const fetchTransaction = async () => {
      try {
        setLoading(true);
        // 如果有初始交易數據，直接使用
        if (initialTransaction) {
          setTransaction(initialTransaction);
          setPaymentMethod(initialTransaction.paymentMethod);
          setLoading(false);
          return;
        }

        // 否則從API獲取交易數據
        const transactionData = await paymentService.getPayment(transactionId);
        setTransaction(transactionData);
        setPaymentMethod(transactionData.paymentMethod);
      } catch (err) {
        console.error('獲取交易信息失敗:', err);
        setError('無法載入交易信息，請返回重試。');
      } finally {
        setLoading(false);
      }
    };

    fetchTransaction();
  }, [transactionId, initialTransaction]);

  /**
   * 處理支付提交
   * @param {boolean} success - 是否模擬成功支付
   */
  const handlePayment = async (success) => {
    try {
      setProcessing(true);
      setResult(null);

      // 調用模擬支付處理API
      const response = await paymentService.processPayment(transactionId, success);
      
      setResult({
        success: response.success,
        message: response.message,
        status: response.transactionStatus
      });

      // 3秒後跳轉到結果頁面
      setTimeout(() => {
        if (success) {
          navigate(`/orders/${transaction.orderId}/success`, {
            state: { paymentResult: response }
          });
        } else {
          navigate(`/orders/${transaction.orderId}/failed`, {
            state: { paymentResult: response }
          });
        }
      }, 3000);
    } catch (err) {
      console.error('處理支付失敗:', err);
      setError('處理支付請求時出錯，請稍後再試。');
    } finally {
      setProcessing(false);
    }
  };

  if (loading) {
    return (
      <div className="payment-process-container">
        <div className="loading-spinner">載入中...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="payment-process-container">
        <div className="error-message">{error}</div>
        <button className="back-button" onClick={() => navigate(-1)}>返回</button>
      </div>
    );
  }

  if (!transaction) {
    return (
      <div className="payment-process-container">
        <div className="error-message">找不到交易信息</div>
        <button className="back-button" onClick={() => navigate(-1)}>返回</button>
      </div>
    );
  }

  return (
    <div className="payment-process-container">
      <h1>模擬支付頁面</h1>
      
      <div className="payment-info">
        <div className="payment-details">
          <h2>交易詳情</h2>
          <div className="detail-item">
            <span className="label">訂單編號:</span>
            <span className="value">{transaction.orderId}</span>
          </div>
          <div className="detail-item">
            <span className="label">金額:</span>
            <span className="value">NT$ {transaction.amount?.toFixed(2)}</span>
          </div>
          <div className="detail-item">
            <span className="label">支付方式:</span>
            <span className="value">{paymentMethod?.name || '未知'}</span>
          </div>
          <div className="detail-item">
            <span className="label">交易狀態:</span>
            <span className="value status">{result?.status || transaction.status}</span>
          </div>
        </div>

        {paymentMethod && (
          <div className="payment-method-display">
            {paymentMethod.iconUrl ? (
              <img src={paymentMethod.iconUrl} alt={paymentMethod.name} />
            ) : (
              <div className="default-icon">{paymentMethod.name.charAt(0)}</div>
            )}
            <h3>{paymentMethod.name}</h3>
          </div>
        )}
      </div>

      {result ? (
        <div className={`payment-result ${result.success ? 'success' : 'error'}`}>
          <h3>{result.success ? '支付處理成功' : '支付處理失敗'}</h3>
          <p>{result.message}</p>
          <p className="redirect-message">即將跳轉到結果頁面...</p>
        </div>
      ) : (
        <div className="payment-actions">
          <p className="instruction">這是模擬支付環境，請選擇模擬結果：</p>
          <div className="action-buttons">
            <button 
              className="success-button"
              onClick={() => handlePayment(true)}
              disabled={processing}
            >
              {processing ? '處理中...' : '模擬支付成功'}
            </button>
            <button 
              className="fail-button"
              onClick={() => handlePayment(false)}
              disabled={processing}
            >
              {processing ? '處理中...' : '模擬支付失敗'}
            </button>
          </div>
        </div>
      )}

      <div className="payment-footer">
        <button 
          className="cancel-button"
          onClick={() => navigate(-1)}
          disabled={processing || result}
        >
          取消並返回
        </button>
      </div>
    </div>
  );
};

export default PaymentProcess;