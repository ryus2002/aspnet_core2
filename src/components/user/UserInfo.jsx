import React, { useState, useEffect } from 'react';
import { Form, Button, Row, Col, Alert, Spinner } from 'react-bootstrap';
import { getCurrentUser, updateUserProfile, changePassword } from '../../services/authService';

/**
 * 用戶資料管理組件
 * 允許用戶查看和編輯個人資料，以及修改密碼
 */
const UserInfo = () => {
  // 用戶資料狀態
  const [userInfo, setUserInfo] = useState({
    username: '',
    email: '',
    fullName: '',
    phoneNumber: '',
    gender: '',
    birthDate: '',
    bio: ''
  });
  
  // 密碼修改狀態
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  
  // 載入和提交狀態
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [passwordSubmitting, setPasswordSubmitting] = useState(false);
  
  // 提示訊息狀態
  const [alert, setAlert] = useState({ show: false, variant: '', message: '' });
  const [passwordAlert, setPasswordAlert] = useState({ show: false, variant: '', message: '' });
  
  // 編輯模式狀態
  const [editMode, setEditMode] = useState(false);
  
  // 載入用戶資料
  useEffect(() => {
    const fetchUserData = async () => {
      try {
        setLoading(true);
        const userData = await getCurrentUser();
        
        // 將API返回的數據映射到組件狀態
        setUserInfo({
          username: userData.username || '',
          email: userData.email || '',
          fullName: userData.fullName || '',
          phoneNumber: userData.phoneNumber || '',
          gender: userData.gender || '',
          birthDate: userData.birthDate ? userData.birthDate.substring(0, 10) : '',
          bio: userData.bio || ''
        });
      } catch (error) {
        console.error('獲取用戶資料失敗:', error);
        setAlert({
          show: true,
          variant: 'danger',
          message: '獲取用戶資料失敗，請稍後再試'
        });
      } finally {
        setLoading(false);
      }
    };
    
    fetchUserData();
  }, []);
  
  // 處理資料變更
  const handleChange = (e) => {
    const { name, value } = e.target;
    setUserInfo(prev => ({
      ...prev,
      [name]: value
    }));
  };
  
  // 處理密碼變更
  const handlePasswordChange = (e) => {
    const { name, value } = e.target;
    setPasswordData(prev => ({
      ...prev,
      [name]: value
    }));
  };
  
  // 提交資料更新
  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      setSubmitting(true);
      
      // 呼叫API更新用戶資料
      await updateUserProfile(userInfo);
      
      setAlert({
        show: true,
        variant: 'success',
        message: '個人資料更新成功'
      });
      
      // 退出編輯模式
      setEditMode(false);
    } catch (error) {
      console.error('更新用戶資料失敗:', error);
      setAlert({
        show: true,
        variant: 'danger',
        message: '更新用戶資料失敗，請稍後再試'
      });
    } finally {
      setSubmitting(false);
    }
  };
  
  // 提交密碼修改
  const handlePasswordSubmit = async (e) => {
    e.preventDefault();
    
    // 驗證新密碼和確認密碼是否匹配
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      setPasswordAlert({
        show: true,
        variant: 'danger',
        message: '新密碼與確認密碼不匹配'
      });
      return;
    }
    
    // 驗證新密碼長度
    if (passwordData.newPassword.length < 8) {
      setPasswordAlert({
        show: true,
        variant: 'danger',
        message: '新密碼長度必須至少為8個字符'
      });
      return;
    }
    
    try {
      setPasswordSubmitting(true);
      
      // 呼叫API修改密碼
      await changePassword(passwordData.currentPassword, passwordData.newPassword);
      
      // 清空密碼表單
      setPasswordData({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      });
      
      setPasswordAlert({
        show: true,
        variant: 'success',
        message: '密碼修改成功'
      });
    } catch (error) {
      console.error('修改密碼失敗:', error);
      setPasswordAlert({
        show: true,
        variant: 'danger',
        message: '修改密碼失敗，請確認當前密碼是否正確'
      });
    } finally {
      setPasswordSubmitting(false);
    }
  };
  
  // 切換編輯模式
  const toggleEditMode = () => {
    setEditMode(!editMode);
    // 清除之前的提示
    setAlert({ show: false, variant: '', message: '' });
  };
  
  // 取消編輯
  const handleCancel = () => {
    // 重新載入用戶資料
    getCurrentUser().then(userData => {
      setUserInfo({
        username: userData.username || '',
        email: userData.email || '',
        fullName: userData.fullName || '',
        phoneNumber: userData.phoneNumber || '',
        gender: userData.gender || '',
        birthDate: userData.birthDate ? userData.birthDate.substring(0, 10) : '',
        bio: userData.bio || ''
      });
    });
    
    // 退出編輯模式
    setEditMode(false);
    // 清除提示
    setAlert({ show: false, variant: '', message: '' });
  };
  
  // 如果正在載入，顯示載入中
  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">載入中...</span>
        </Spinner>
        <p className="mt-2">載入用戶資料中...</p>
      </div>
    );
  }
  
  return (
    <div className="user-info-container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h3>個人資料</h3>
        {!editMode && (
          <Button variant="outline-primary" onClick={toggleEditMode}>
            <i className="bi bi-pencil-fill me-2"></i>編輯資料
          </Button>
        )}
      </div>
      
      {alert.show && (
        <Alert variant={alert.variant} onClose={() => setAlert({ show: false, variant: '', message: '' })} dismissible>
          {alert.message}
        </Alert>
      )}
      
      {/* 個人資料表單 */}
      <Form onSubmit={handleSubmit}>
        <Row>
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>用戶名</Form.Label>
              <Form.Control
                type="text"
                name="username"
                value={userInfo.username}
                disabled
              />
              <Form.Text className="text-muted">
                用戶名不可修改
              </Form.Text>
            </Form.Group>
          </Col>
          
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>電子郵件</Form.Label>
              <Form.Control
                type="email"
                name="email"
                value={userInfo.email}
                disabled
              />
              <Form.Text className="text-muted">
                電子郵件不可修改
              </Form.Text>
            </Form.Group>
          </Col>
        </Row>
        
        <Row>
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>姓名</Form.Label>
              <Form.Control
                type="text"
                name="fullName"
                value={userInfo.fullName}
                onChange={handleChange}
                disabled={!editMode}
                required
              />
            </Form.Group>
          </Col>
          
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>電話號碼</Form.Label>
              <Form.Control
                type="tel"
                name="phoneNumber"
                value={userInfo.phoneNumber}
                onChange={handleChange}
                disabled={!editMode}
              />
            </Form.Group>
          </Col>
        </Row>
        
        <Row>
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>性別</Form.Label>
              <Form.Select
                name="gender"
                value={userInfo.gender}
                onChange={handleChange}
                disabled={!editMode}
              >
                <option value="">請選擇</option>
                <option value="male">男</option>
                <option value="female">女</option>
                <option value="other">其他</option>
                <option value="prefer_not_to_say">不願透露</option>
              </Form.Select>
            </Form.Group>
          </Col>
          
          <Col md={6}>
            <Form.Group className="mb-3">
              <Form.Label>出生日期</Form.Label>
              <Form.Control
                type="date"
                name="birthDate"
                value={userInfo.birthDate}
                onChange={handleChange}
                disabled={!editMode}
              />
            </Form.Group>
          </Col>
        </Row>
        
        <Form.Group className="mb-3">
          <Form.Label>個人簡介</Form.Label>
          <Form.Control
            as="textarea"
            name="bio"
            value={userInfo.bio}
            onChange={handleChange}
            disabled={!editMode}
            rows={3}
          />
        </Form.Group>
        
        {editMode && (
          <div className="d-flex justify-content-end gap-2 mb-4">
            <Button variant="outline-secondary" onClick={handleCancel}>
              取消
            </Button>
            <Button variant="primary" type="submit" disabled={submitting}>
              {submitting ? (
                <>
                  <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                  <span className="ms-2">儲存中...</span>
                </>
              ) : '儲存變更'}
            </Button>
          </div>
        )}
      </Form>
      
      <hr className="my-4" />
      
      {/* 密碼修改表單 */}
      <div className="password-change-section">
        <h3 className="mb-3">修改密碼</h3>
        
        {passwordAlert.show && (
          <Alert 
            variant={passwordAlert.variant} 
            onClose={() => setPasswordAlert({ show: false, variant: '', message: '' })} 
            dismissible
          >
            {passwordAlert.message}
          </Alert>
        )}
        
        <Form onSubmit={handlePasswordSubmit}>
          <Row>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>當前密碼</Form.Label>
                <Form.Control
                  type="password"
                  name="currentPassword"
                  value={passwordData.currentPassword}
                  onChange={handlePasswordChange}
                  required
                />
              </Form.Group>
            </Col>
          </Row>
          
          <Row>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>新密碼</Form.Label>
                <Form.Control
                  type="password"
                  name="newPassword"
                  value={passwordData.newPassword}
                  onChange={handlePasswordChange}
                  required
                  minLength={8}
                />
                <Form.Text className="text-muted">
                  密碼長度至少為8個字符
                </Form.Text>
              </Form.Group>
            </Col>
            
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>確認新密碼</Form.Label>
                <Form.Control
                  type="password"
                  name="confirmPassword"
                  value={passwordData.confirmPassword}
                  onChange={handlePasswordChange}
                  required
                  minLength={8}
                />
              </Form.Group>
            </Col>
          </Row>
          
          <div className="d-flex justify-content-end">
            <Button variant="primary" type="submit" disabled={passwordSubmitting}>
              {passwordSubmitting ? (
                <>
                  <Spinner as="span" animation="border" size="sm" role="status" aria-hidden="true" />
                  <span className="ms-2">處理中...</span>
                </>
              ) : '更新密碼'}
            </Button>
          </div>
        </Form>
      </div>
    </div>
  );
};

export default UserInfo;