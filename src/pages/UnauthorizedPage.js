import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './UnauthorizedPage.css';

const UnauthorizedPage = () => {
  const navigate = useNavigate();
  
  return (
    <div className="unauthorized-container">
      <div className="unauthorized-content">
        <h1>訪問被拒絕</h1>
        <div className="unauthorized-icon">🔒</div>
        <p>很抱歉，您沒有訪問此頁面的權限。</p>
        <p>請確認您的帳號擁有所需的權限，或聯繫系統管理員尋求幫助。</p>
        
        <div className="unauthorized-actions">
          <button 
            className="back-button"
            onClick={() => navigate(-1)}
          >
            返回上一頁
          </button>
          <Link to="/" className="home-link">
            返回首頁
          </Link>
        </div>
      </div>
    </div>
  );
};

export default UnauthorizedPage;