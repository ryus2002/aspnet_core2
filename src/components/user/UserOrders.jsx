import React, { useState, useEffect } from 'react';
import { Card, Badge, Button, Row, Col, Spinner, Alert, Modal, Table } from 'react-bootstrap';
import { getOrders, getOrderById, cancelOrder, getOrderHistory } from '../../services/orderService';
import { formatDate, formatCurrency } from '../../utils/formatUtils';

/**
 * 用戶訂單管理組件
 * 顯示用戶的所有訂單及其詳情
 */
const UserOrders = () => {
  // 訂單列表狀態
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // 訂單詳情狀態
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [orderDetails, setOrderDetails] = useState(null);
  const [orderHistory, setOrderHistory] = useState([]);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [showDetails, setShowDetails] = useState(false);
  
  // 取消訂單狀態
  const [cancelReason, setCancelReason] = useState('');
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancellingOrder, setCancellingOrder] = useState(false);
  
  // 分頁狀態
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [hasMore, setHasMore] = useState(false);
  
  // 載入訂單列表
  useEffect(() => {
    const fetchOrders = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // 呼叫API獲取訂單列表
        const response = await getOrders({ page, limit: 10 });
        
        setOrders(response.data);
        setTotalPages(response.totalPages || 1);
        setHasMore(page < (response.totalPages || 1));
      } catch (err) {
        console.error('獲取訂單列表失敗:', err);
        setError('獲取訂單列表失敗，請稍後再試');
      } finally {
        setLoading(false);
      }
    };
    
    fetchOrders();
  }, [page]);
  
  // 載入更多訂單
  const loadMoreOrders = () => {
    if (hasMore) {
      setPage(prevPage => prevPage + 1);
    }
  };
  
  // 查看訂單詳情
  const viewOrderDetails = async (orderId) => {
    try {
      setDetailsLoading(true);
      setSelectedOrder(orderId);
      
      // 並行獲取訂單詳情和訂單歷史
      const [orderData, historyData] = await Promise.all([
        getOrderById(orderId),
        getOrderHistory(orderId)
      ]);
      
      setOrderDetails(orderData);
      setOrderHistory(historyData);
      setShowDetails(true);
    } catch (err) {
      console.error('獲取訂單詳情失敗:', err);
      alert('獲取訂單詳情失敗，請稍後再試');
    } finally {
      setDetailsLoading(false);
    }
  };
  
  // 關閉訂單詳情模態框
  const closeDetails = () => {
    setShowDetails(false);
    setSelectedOrder(null);
    setOrderDetails(null);
    setOrderHistory([]);
  };
  
  // 打開取消訂單模態框
  const openCancelModal = (orderId) => {
    setSelectedOrder(orderId);
    setShowCancelModal(true);
  };
  
  // 關閉取消訂單模態框
  const closeCancelModal = () => {
    setShowCancelModal(false);
    setSelectedOrder(null);
    setCancelReason('');
  };
  
  // 處理取消訂單
  const handleCancelOrder = async () => {
    if (!selectedOrder) return;
    
    try {
      setCancellingOrder(true);
      
      // 呼叫API取消訂單
      await cancelOrder(selectedOrder, cancelReason);
      
      // 更新訂單列表
      setOrders(prevOrders => 
        prevOrders.map(order => 
          order.id === selectedOrder 
            ? { ...order, status: 'cancelled', cancelledAt: new Date().toISOString() } 
            : order
        )
      );
      
      closeCancelModal();
      alert('訂單已成功取消');
    } catch (err) {
      console.error('取消訂單失敗:', err);
      alert('取消訂單失敗，請稍後再試');
    } finally {
      setCancellingOrder(false);
    }
  };
  
  // 獲取訂單狀態標籤顏色
  const getStatusBadgeVariant = (status) => {
    switch (status) {
      case 'pending': return 'warning';
      case 'paid': return 'info';
      case 'processing': return 'primary';
      case 'shipped': return 'info';
      case 'delivered': return 'success';
      case 'cancelled': return 'danger';
      case 'refunded': return 'secondary';
      default: return 'secondary';
    }
  };
  
  // 獲取訂單狀態中文名稱
  const getStatusText = (status) => {
    switch (status) {
      case 'pending': return '待付款';
      case 'paid': return '已付款';
      case 'processing': return '處理中';
      case 'shipped': return '已出貨';
      case 'delivered': return '已送達';
      case 'cancelled': return '已取消';
      case 'refunded': return '已退款';
      default: return '未知狀態';
    }
  };
  
  // 如果正在載入，顯示載入中
  if (loading && page === 1) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">載入中...</span>
        </Spinner>
        <p className="mt-2">載入訂單資料中...</p>
      </div>
    );
  }
  
  // 如果發生錯誤，顯示錯誤訊息
  if (error && orders.length === 0) {
    return (
      <Alert variant="danger">
        <Alert.Heading>載入失敗</Alert.Heading>
        <p>{error}</p>
        <Button variant="outline-danger" onClick={() => window.location.reload()}>
          重新載入
        </Button>
      </Alert>
    );
  }
  
  // 如果沒有訂單，顯示提示訊息
  if (orders.length === 0 && !loading) {
    return (
      <div className="text-center py-5">
        <i className="bi bi-bag-x" style={{ fontSize: '3rem', color: '#6c757d' }}></i>
        <h4 className="mt-3">您還沒有任何訂單</h4>
        <p className="text-muted">開始購物，建立您的第一筆訂單吧！</p>
        <Button variant="primary" href="/products">
          瀏覽商品
        </Button>
      </div>
    );
  }
  
  return (
    <div className="user-orders-container">
      <h3 className="mb-4">我的訂單</h3>
      
      {/* 訂單列表 */}
      {orders.map(order => (
        <Card key={order.id} className="mb-3 order-card">
          <Card.Header className="d-flex justify-content-between align-items-center">
            <div>
              <span className="fw-bold">訂單編號: </span>
              <span>{order.orderNumber}</span>
            </div>
            <Badge bg={getStatusBadgeVariant(order.status)}>
              {getStatusText(order.status)}
            </Badge>
          </Card.Header>
          
          <Card.Body>
            <Row>
              <Col md={6}>
                <p className="mb-1">
                  <span className="text-muted">下單日期: </span>
                  {formatDate(order.createdAt)}
                </p>
                <p className="mb-1">
                  <span className="text-muted">商品數量: </span>
                  {order.itemsCount} 件
                </p>
              </Col>
              
              <Col md={6}>
                <p className="mb-1">
                  <span className="text-muted">訂單金額: </span>
                  <span className="fw-bold">{formatCurrency(order.totalAmount)}</span>
                </p>
                <p className="mb-1">
                  <span className="text-muted">配送方式: </span>
                  {order.shippingMethod || '標準配送'}
                </p>
              </Col>
            </Row>
          </Card.Body>
          
          <Card.Footer className="bg-white d-flex justify-content-end">
            <Button 
              variant="outline-primary" 
              size="sm" 
              className="me-2"
              onClick={() => viewOrderDetails(order.id)}
            >
              查看詳情
            </Button>
            
            {/* 只有待付款或已付款的訂單可以取消 */}
            {['pending', 'paid'].includes(order.status) && (
              <Button 
                variant="outline-danger" 
                size="sm"
                onClick={() => openCancelModal(order.id)}
              >
                取消訂單
              </Button>
            )}
          </Card.Footer>
        </Card>
      ))}
      
      {/* 載入更多按鈕 */}
      {hasMore && (
        <div className="text-center mt-3 mb-4">
          <Button 
            variant="outline-primary" 
            onClick={loadMoreOrders}
            disabled={loading}
          >
            {loading ? (
              <>
                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                <span className="ms-2">載入中...</span>
              </>
            ) : '載入更多訂單'}
          </Button>
        </div>
      )}
      
      {/* 訂單詳情模態框 */}
      <Modal show={showDetails} onHide={closeDetails} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>訂單詳情</Modal.Title>
        </Modal.Header>
        
        <Modal.Body>
          {detailsLoading ? (
            <div className="text-center py-4">
              <Spinner animation="border" role="status">
                <span className="visually-hidden">載入中...</span>
              </Spinner>
              <p className="mt-2">載入訂單詳情中...</p>
            </div>
          ) : orderDetails ? (
            <>
              <div className="order-info mb-4">
                <Row>
                  <Col md={6}>
                    <p className="mb-1">
                      <span className="fw-bold">訂單編號: </span>
                      {orderDetails.orderNumber}
                    </p>
                    <p className="mb-1">
                      <span className="fw-bold">下單日期: </span>
                      {formatDate(orderDetails.createdAt)}
                    </p>
                    <p className="mb-1">
                      <span className="fw-bold">訂單狀態: </span>
                      <Badge bg={getStatusBadgeVariant(orderDetails.status)}>
                        {getStatusText(orderDetails.status)}
                      </Badge>
                    </p>
                  </Col>
                  
                  <Col md={6}>
                    <p className="mb-1">
                      <span className="fw-bold">配送方式: </span>
                      {orderDetails.shippingMethod || '標準配送'}
                    </p>
                    <p className="mb-1">
                      <span className="fw-bold">配送費用: </span>
                      {formatCurrency(orderDetails.shippingFee)}
                    </p>
                    {orderDetails.completedAt && (
                      <p className="mb-1">
                        <span className="fw-bold">完成日期: </span>
                        {formatDate(orderDetails.completedAt)}
                      </p>
                    )}
                  </Col>
                </Row>
              </div>
              
              <h5 className="mb-3">訂單項目</h5>
              <Table responsive>
                <thead>
                  <tr>
                    <th>商品</th>
                    <th>單價</th>
                    <th>數量</th>
                    <th className="text-end">小計</th>
                  </tr>
                </thead>
                <tbody>
                  {orderDetails.items && orderDetails.items.map(item => (
                    <tr key={item.id}>
                      <td>
                        <div className="d-flex align-items-center">
                          {item.imageUrl && (
                            <img 
                              src={item.imageUrl} 
                              alt={item.name} 
                              style={{ width: '50px', height: '50px', objectFit: 'cover', marginRight: '10px' }}
                            />
                          )}
                          <div>
                            <div>{item.name}</div>
                            {item.attributes && Object.entries(item.attributes).map(([key, value]) => (
                              <small key={key} className="text-muted d-block">
                                {key}: {value}
                              </small>
                            ))}
                          </div>
                        </div>
                      </td>
                      <td>{formatCurrency(item.unitPrice)}</td>
                      <td>{item.quantity}</td>
                      <td className="text-end">{formatCurrency(item.totalPrice)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr>
                    <td colSpan="3" className="text-end fw-bold">商品小計:</td>
                    <td className="text-end">{formatCurrency(orderDetails.totalAmount - orderDetails.shippingFee - orderDetails.taxAmount)}</td>
                  </tr>
                  {orderDetails.taxAmount > 0 && (
                    <tr>
                      <td colSpan="3" className="text-end fw-bold">稅額:</td>
                      <td className="text-end">{formatCurrency(orderDetails.taxAmount)}</td>
                    </tr>
                  )}
                  <tr>
                    <td colSpan="3" className="text-end fw-bold">運費:</td>
                    <td className="text-end">{formatCurrency(orderDetails.shippingFee)}</td>
                  </tr>
                  {orderDetails.discountAmount > 0 && (
                    <tr>
                      <td colSpan="3" className="text-end fw-bold">折扣:</td>
                      <td className="text-end">-{formatCurrency(orderDetails.discountAmount)}</td>
                    </tr>
                  )}
                  <tr>
                    <td colSpan="3" className="text-end fw-bold">訂單總計:</td>
                    <td className="text-end fw-bold">{formatCurrency(orderDetails.totalAmount)}</td>
                  </tr>
                </tfoot>
              </Table>
              
              {orderDetails.shippingAddress && (
                <>
                  <h5 className="mb-3 mt-4">配送資訊</h5>
                  <Card>
                    <Card.Body>
                      <p className="mb-1">
                        <span className="fw-bold">收件人: </span>
                        {orderDetails.shippingAddress.name}
                      </p>
                      <p className="mb-1">
                        <span className="fw-bold">電話: </span>
                        {orderDetails.shippingAddress.phone}
                      </p>
                      <p className="mb-1">
                        <span className="fw-bold">地址: </span>
                        {`${orderDetails.shippingAddress.postalCode} ${orderDetails.shippingAddress.city} ${orderDetails.shippingAddress.addressLine1} ${orderDetails.shippingAddress.addressLine2 || ''}`}
                      </p>
                    </Card.Body>
                  </Card>
                </>
              )}
              
              {orderHistory.length > 0 && (
                <>
                  <h5 className="mb-3 mt-4">訂單狀態歷史</h5>
                  <Table responsive>
                    <thead>
                      <tr>
                        <th>日期時間</th>
                        <th>狀態</th>
                        <th>備註</th>
                      </tr>
                    </thead>
                    <tbody>
                      {orderHistory.map(history => (
                        <tr key={history.id}>
                          <td>{formatDate(history.changedAt, true)}</td>
                          <td>
                            <Badge bg={getStatusBadgeVariant(history.status)}>
                              {getStatusText(history.status)}
                            </Badge>
                          </td>
                          <td>{history.comment || '-'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </Table>
                </>
              )}
            </>
          ) : (
            <Alert variant="warning">
              無法載入訂單詳情，請稍後再試
            </Alert>
          )}
        </Modal.Body>
        
        <Modal.Footer>
          <Button variant="secondary" onClick={closeDetails}>
            關閉
          </Button>
          
          {/* 只有待付款或已付款的訂單可以取消 */}
          {orderDetails && ['pending', 'paid'].includes(orderDetails.status) && (
            <Button 
              variant="danger" 
              onClick={() => {
                closeDetails();
                openCancelModal(orderDetails.id);
              }}
            >
              取消訂單
            </Button>
          )}
        </Modal.Footer>
      </Modal>
      
      {/* 取消訂單確認模態框 */}
      <Modal show={showCancelModal} onHide={closeCancelModal}>
        <Modal.Header closeButton>
          <Modal.Title>取消訂單</Modal.Title>
        </Modal.Header>
        
        <Modal.Body>
          <p>您確定要取消此訂單嗎？此操作無法撤銷。</p>
          <div className="form-group">
            <label htmlFor="cancelReason">取消原因 (選填):</label>
            <textarea
              id="cancelReason"
              className="form-control mt-2"
              rows="3"
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              placeholder="請輸入取消原因..."
            ></textarea>
          </div>
        </Modal.Body>
        
        <Modal.Footer>
          <Button variant="secondary" onClick={closeCancelModal}>
            返回
          </Button>
          <Button 
            variant="danger" 
            onClick={handleCancelOrder}
            disabled={cancellingOrder}
          >
            {cancellingOrder ? (
              <>
                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                <span className="ms-2">處理中...</span>
              </>
            ) : '確認取消'}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
};

export default UserOrders;