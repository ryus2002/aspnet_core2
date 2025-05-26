/**
 * 應用程式入口點
 * 負責渲染React應用到DOM
 */
import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import './index.css';
import App from './App';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { CartProvider } from './contexts/CartContext';
import { setupHttpInterceptors } from './utils/httpInterceptor';

// 初始化HTTP攔截器的組件
const HttpInterceptorInitializer = ({ children }) => {
  const { refreshToken, logout } = useAuth();
  
  React.useEffect(() => {
    setupHttpInterceptors(refreshToken, logout);
  }, [refreshToken, logout]);
  
  return children;
};

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <HttpInterceptorInitializer>
        <CartProvider>
          <App />
        </CartProvider>
        </HttpInterceptorInitializer>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>
);