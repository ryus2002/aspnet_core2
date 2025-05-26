import React, { useState } from 'react';
import { Container, Row, Col, Nav, Tab } from 'react-bootstrap';
import UserInfo from '../components/user/UserInfo';
import UserOrders from '../components/user/UserOrders';
import UserAddresses from '../components/user/UserAddresses';
import './UserProfile.css';

/**
 * 用戶個人中心頁面
 * 整合用戶資料、訂單管理和地址管理功能
 */
const UserProfile = () => {
  // 預設選中的標籤
  const [activeTab, setActiveTab] = useState('profile');

  return (
    <Container className="user-profile-container my-5">
      <h2 className="text-center mb-4">個人中心</h2>
      
      <Row>
        {/* 左側導航欄 */}
        <Col md={3}>
          <div className="profile-sidebar">
            <div className="user-avatar-container text-center mb-4">
              <div className="user-avatar">
                {/* 用戶頭像，如果沒有則顯示默認頭像 */}
                <img 
                  src="/images/default-avatar.png" 
                  alt="用戶頭像" 
                  className="img-fluid rounded-circle"
                  onError={(e) => {
                    e.target.onerror = null;
                    e.target.src = 'https://via.placeholder.com/150';
                  }}
                />
              </div>
            </div>
            
            <Nav variant="pills" className="flex-column profile-nav" activeKey={activeTab} onSelect={(key) => setActiveTab(key)}>
              <Nav.Item>
                <Nav.Link eventKey="profile" className="profile-nav-link">
                  <i className="bi bi-person-fill me-2"></i>個人資料
                </Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="orders" className="profile-nav-link">
                  <i className="bi bi-bag-fill me-2"></i>我的訂單
                </Nav.Link>
              </Nav.Item>
              <Nav.Item>
                <Nav.Link eventKey="addresses" className="profile-nav-link">
                  <i className="bi bi-geo-alt-fill me-2"></i>收貨地址
                </Nav.Link>
              </Nav.Item>
            </Nav>
          </div>
        </Col>
        
        {/* 右側內容區域 */}
        <Col md={9}>
          <div className="profile-content">
            <Tab.Content>
              <Tab.Pane eventKey="profile" active={activeTab === 'profile'}>
                <UserInfo />
              </Tab.Pane>
              <Tab.Pane eventKey="orders" active={activeTab === 'orders'}>
                <UserOrders />
              </Tab.Pane>
              <Tab.Pane eventKey="addresses" active={activeTab === 'addresses'}>
                <UserAddresses />
              </Tab.Pane>
            </Tab.Content>
          </div>
        </Col>
      </Row>
    </Container>
  );
};

export default UserProfile;