import React, { useState, useEffect } from 'react';
import { Card, Button, Row, Col, Form, Spinner, Alert, Modal } from 'react-bootstrap';
import { getUserAddresses, addUserAddress, updateUserAddress, deleteUserAddress, setDefaultAddress } from '../../services/addressService';

/**
 * 用戶收貨地址管理組件
 * 允許用戶管理多個收貨地址，包括新增、編輯、刪除和設置默認地址
 */
const UserAddresses = () => {
  // 地址列表狀態
  const [addresses, setAddresses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // 地址表單狀態
  const [showAddressModal, setShowAddressModal] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [currentAddress, setCurrentAddress] = useState({
    id: null,
    name: '',
    phone: '',
    addressLine1: '',
    addressLine2: '',
    city: '',
    state: '',
    postalCode: '',
    country: '',
    isDefault: false,
    addressType: 'shipping'
  });
  
  // 表單提交狀態
  const [submitting, setSubmitting] = useState(false);
  const [formErrors, setFormErrors] = useState({});
  
  // 刪除確認狀態
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [addressToDelete, setAddressToDelete] = useState(null);
  const [deleting, setDeleting] = useState(false);
  
  // 載入用戶地址
  useEffect(() => {
    const fetchAddresses = async () => {
      try {
        setLoading(true);
        setError(null);
        
        // 呼叫API獲取用戶地址
        const addressList = await getUserAddresses();
        setAddresses(addressList);
      } catch (err) {
        console.error('獲取地址失敗:', err);
        setError('獲取地址失敗，請稍後再試');
      } finally {
        setLoading(false);
      }
    };
    
    fetchAddresses();
  }, []);
  
  // 打開新增地址模態框
  const openAddAddressModal = () => {
    setCurrentAddress({
      id: null,
      name: '',
      phone: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      state: '',
      postalCode: '',
      country: 'Taiwan',
      isDefault: addresses.length === 0, // 如果是第一個地址，則設為默認
      addressType: 'shipping'
    });
    setIsEditing(false);
    setFormErrors({});
    setShowAddressModal(true);
  };
  
  // 打開編輯地址模態框
  const openEditAddressModal = (address) => {
    setCurrentAddress({
      ...address
    });
    setIsEditing(true);
    setFormErrors({});
    setShowAddressModal(true);
  };
  
  // 關閉地址模態框
  const closeAddressModal = () => {
    setShowAddressModal(false);
  };
  
  // 處理地址表單變更
  const handleAddressChange = (e) => {
    const { name, value, type, checked } = e.target;
    setCurrentAddress(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
    
    // 清除對應欄位的錯誤
    if (formErrors[name]) {
      setFormErrors(prev => ({
        ...prev,
        [name]: null
      }));
    }
  };
  
  // 驗證地址表單
  const validateAddressForm = () => {
    const errors = {};
    
    if (!currentAddress.name.trim()) {
      errors.name = '請輸入收件人姓名';
    }
    
    if (!currentAddress.phone.trim()) {
      errors.phone = '請輸入聯絡電話';
    } else if (!/^[0-9]{8,10}$/.test(currentAddress.phone.replace(/[- ]/g, ''))) {
      errors.phone = '請輸入有效的電話號碼';
    }
    
    if (!currentAddress.addressLine1.trim()) {
      errors.addressLine1 = '請輸入地址';
    }
    
    if (!currentAddress.city.trim()) {
      errors.city = '請輸入城市';
    }
    
    if (!currentAddress.postalCode.trim()) {
      errors.postalCode = '請輸入郵遞區號';
    } else if (!/^[0-9]{3,6}$/.test(currentAddress.postalCode.replace(/[- ]/g, ''))) {
      errors.postalCode = '請輸入有效的郵遞區號';
    }
    
    if (!currentAddress.country.trim()) {
      errors.country = '請選擇國家';
    }
    
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };
  
  // 提交地址表單
  const handleAddressSubmit = async (e) => {
    e.preventDefault();
    
    // 驗證表單
    if (!validateAddressForm()) {
      return;
    }
    
    try {
      setSubmitting(true);
      
      if (isEditing) {
        // 更新現有地址
        const updatedAddress = await updateUserAddress(currentAddress.id, currentAddress);
        
        // 更新地址列表
        setAddresses(prevAddresses => 
          prevAddresses.map(addr => 
            addr.id === updatedAddress.id ? updatedAddress : addr
          )
        );
        
        // 如果設為默認地址，更新其他地址的默認狀態
        if (updatedAddress.isDefault) {
          setAddresses(prevAddresses => 
            prevAddresses.map(addr => 
              addr.id !== updatedAddress.id ? { ...addr, isDefault: false } : addr
            )
          );
        }
      } else {
        // 新增地址
        const newAddress = await addUserAddress(currentAddress);
        
        // 更新地址列表
        setAddresses(prevAddresses => [...prevAddresses, newAddress]);
        
        // 如果設為默認地址，更新其他地址的默認狀態
        if (newAddress.isDefault) {
          setAddresses(prevAddresses => 
            prevAddresses.map(addr => 
              addr.id !== newAddress.id ? { ...addr, isDefault: false } : addr
            )
          );
        }
      }
      
      // 關閉模態框
      closeAddressModal();
    } catch (err) {
      console.error('保存地址失敗:', err);
      alert('保存地址失敗，請稍後再試');
    } finally {
      setSubmitting(false);
    }
  };
  
  // 打開刪除確認模態框
  const openDeleteConfirmModal = (addressId) => {
    setAddressToDelete(addressId);
    setShowDeleteModal(true);
  };
  
  // 關閉刪除確認模態框
  const closeDeleteConfirmModal = () => {
    setShowDeleteModal(false);
    setAddressToDelete(null);
  };
  
  // 處理刪除地址
  const handleDeleteAddress = async () => {
    if (!addressToDelete) return;
    
    try {
      setDeleting(true);
      
      // 呼叫API刪除地址
      await deleteUserAddress(addressToDelete);
      
      // 更新地址列表
      setAddresses(prevAddresses => 
        prevAddresses.filter(addr => addr.id !== addressToDelete)
      );
      
      closeDeleteConfirmModal();
    } catch (err) {
      console.error('刪除地址失敗:', err);
      alert('刪除地址失敗，請稍後再試');
    } finally {
      setDeleting(false);
    }
  };
  
  // 設置默認地址
  const handleSetDefaultAddress = async (addressId) => {
    try {
      // 呼叫API設置默認地址
      await setDefaultAddress(addressId);
      
      // 更新地址列表
      setAddresses(prevAddresses => 
        prevAddresses.map(addr => ({
          ...addr,
          isDefault: addr.id === addressId
        }))
      );
    } catch (err) {
      console.error('設置默認地址失敗:', err);
      alert('設置默認地址失敗，請稍後再試');
    }
  };
  
  // 如果正在載入，顯示載入中
  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">載入中...</span>
        </Spinner>
        <p className="mt-2">載入地址資料中...</p>
      </div>
    );
  }
  
  // 如果發生錯誤，顯示錯誤訊息
  if (error) {
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
  
  return (
    <div className="user-addresses-container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h3>收貨地址管理</h3>
        <Button variant="primary" onClick={openAddAddressModal}>
          <i className="bi bi-plus-circle me-2"></i>新增地址
        </Button>
      </div>
      
      {/* 如果沒有地址，顯示提示訊息 */}
      {addresses.length === 0 ? (
        <div className="text-center py-5 bg-light rounded">
          <i className="bi bi-geo-alt" style={{ fontSize: '3rem', color: '#6c757d' }}></i>
          <h4 className="mt-3">您還沒有添加任何地址</h4>
          <p className="text-muted">添加收貨地址以便更快地完成結帳流程</p>
          <Button variant="primary" onClick={openAddAddressModal}>
            添加新地址
          </Button>
        </div>
      ) : (
        <Row>
          {addresses.map(address => (
            <Col md={6} key={address.id} className="mb-4">
              <Card className="h-100 address-card">
                {address.isDefault && (
                  <div className="default-address-badge">
                    <Badge bg="success">默認地址</Badge>
                  </div>
                )}
                
                <Card.Body>
                  <Card.Title>{address.name}</Card.Title>
                  <Card.Text>
                    <small className="text-muted d-block mb-2">電話: {address.phone}</small>
                    <span>
                      {address.postalCode} {address.city} {address.state}<br />
                      {address.addressLine1}<br />
                      {address.addressLine2 && `${address.addressLine2}`}
                    </span>
                  </Card.Text>
                </Card.Body>
                
                <Card.Footer className="bg-white">
                  <div className="d-flex justify-content-between">
                    <div>
                      <Button 
                        variant="outline-primary" 
                        size="sm" 
                        className="me-2"
                        onClick={() => openEditAddressModal(address)}
                      >
                        編輯
                      </Button>
                      <Button 
                        variant="outline-danger" 
                        size="sm"
                        onClick={() => openDeleteConfirmModal(address.id)}
                      >
                        刪除
                      </Button>
                    </div>
                    
                    {!address.isDefault && (
                      <Button 
                        variant="outline-success" 
                        size="sm"
                        onClick={() => handleSetDefaultAddress(address.id)}
                      >
                        設為默認
                      </Button>
                    )}
                  </div>
                </Card.Footer>
              </Card>
            </Col>
          ))}
        </Row>
      )}
      
      {/* 地址表單模態框 */}
      <Modal show={showAddressModal} onHide={closeAddressModal} size="lg">
        <Modal.Header closeButton>
          <Modal.Title>{isEditing ? '編輯地址' : '新增地址'}</Modal.Title>
        </Modal.Header>
        
        <Modal.Body>
          <Form onSubmit={handleAddressSubmit}>
            <Row>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>收件人姓名 <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="text"
                    name="name"
                    value={currentAddress.name}
                    onChange={handleAddressChange}
                    isInvalid={!!formErrors.name}
                    required
                  />
                  <Form.Control.Feedback type="invalid">
                    {formErrors.name}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
              
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>聯絡電話 <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="tel"
                    name="phone"
                    value={currentAddress.phone}
                    onChange={handleAddressChange}
                    isInvalid={!!formErrors.phone}
                    required
                  />
                  <Form.Control.Feedback type="invalid">
                    {formErrors.phone}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
            </Row>
            
            <Form.Group className="mb-3">
              <Form.Label>地址 <span className="text-danger">*</span></Form.Label>
              <Form.Control
                type="text"
                name="addressLine1"
                value={currentAddress.addressLine1}
                onChange={handleAddressChange}
                isInvalid={!!formErrors.addressLine1}
                required
                placeholder="街道門牌號碼"
              />
              <Form.Control.Feedback type="invalid">
                {formErrors.addressLine1}
              </Form.Control.Feedback>
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>地址第二行</Form.Label>
              <Form.Control
                type="text"
                name="addressLine2"
                value={currentAddress.addressLine2}
                onChange={handleAddressChange}
                placeholder="公寓、套房、單位等（選填）"
              />
            </Form.Group>
            
            <Row>
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>郵遞區號 <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="text"
                    name="postalCode"
                    value={currentAddress.postalCode}
                    onChange={handleAddressChange}
                    isInvalid={!!formErrors.postalCode}
                    required
                  />
                  <Form.Control.Feedback type="invalid">
                    {formErrors.postalCode}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
              
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>城市 <span className="text-danger">*</span></Form.Label>
                  <Form.Control
                    type="text"
                    name="city"
                    value={currentAddress.city}
                    onChange={handleAddressChange}
                    isInvalid={!!formErrors.city}
                    required
                  />
                  <Form.Control.Feedback type="invalid">
                    {formErrors.city}
                  </Form.Control.Feedback>
                </Form.Group>
              </Col>
              
              <Col md={4}>
                <Form.Group className="mb-3">
                  <Form.Label>州/省</Form.Label>
                  <Form.Control
                    type="text"
                    name="state"
                    value={currentAddress.state}
                    onChange={handleAddressChange}
                  />
                </Form.Group>
              </Col>
            </Row>
            
            <Form.Group className="mb-3">
              <Form.Label>國家 <span className="text-danger">*</span></Form.Label>
              <Form.Select
                name="country"
                value={currentAddress.country}
                onChange={handleAddressChange}
                isInvalid={!!formErrors.country}
                required
              >
                <option value="">請選擇國家</option>
                <option value="Taiwan">台灣</option>
                <option value="China">中國</option>
                <option value="Hong Kong">香港</option>
                <option value="Japan">日本</option>
                <option value="Korea">韓國</option>
                <option value="United States">美國</option>
                <option value="Canada">加拿大</option>
                <option value="Australia">澳洲</option>
                <option value="United Kingdom">英國</option>
                <option value="Singapore">新加坡</option>
                <option value="Malaysia">馬來西亞</option>
              </Form.Select>
              <Form.Control.Feedback type="invalid">
                {formErrors.country}
              </Form.Control.Feedback>
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>地址類型</Form.Label>
              <Form.Select
                name="addressType"
                value={currentAddress.addressType}
                onChange={handleAddressChange}
              >
                <option value="shipping">收貨地址</option>
                <option value="billing">帳單地址</option>
                <option value="both">收貨和帳單地址</option>
              </Form.Select>
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Check
                type="checkbox"
                id="isDefault"
                name="isDefault"
                label="設為默認地址"
                checked={currentAddress.isDefault}
                onChange={handleAddressChange}
              />
            </Form.Group>
          </Form>
        </Modal.Body>
        
        <Modal.Footer>
          <Button variant="secondary" onClick={closeAddressModal}>
            取消
          </Button>
          <Button 
            variant="primary" 
            onClick={handleAddressSubmit}
            disabled={submitting}
          >
            {submitting ? (
              <>
                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                <span className="ms-2">處理中...</span>
              </>
            ) : (isEditing ? '更新地址' : '保存地址')}
          </Button>
        </Modal.Footer>
      </Modal>
      
      {/* 刪除確認模態框 */}
      <Modal show={showDeleteModal} onHide={closeDeleteConfirmModal}>
        <Modal.Header closeButton>
          <Modal.Title>確認刪除</Modal.Title>
        </Modal.Header>
        
        <Modal.Body>
          <p>您確定要刪除此地址嗎？此操作無法撤銷。</p>
        </Modal.Body>
        
        <Modal.Footer>
          <Button variant="secondary" onClick={closeDeleteConfirmModal}>
            取消
          </Button>
          <Button 
            variant="danger" 
            onClick={handleDeleteAddress}
            disabled={deleting}
          >
            {deleting ? (
              <>
                <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                <span className="ms-2">處理中...</span>
              </>
            ) : '確認刪除'}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  );
};

export default UserAddresses;