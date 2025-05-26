import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { CartProvider } from './contexts/CartContext';
import ProtectedRoute from './components/common/ProtectedRoute';

// 頁面組件
import HomePage from './pages/HomePage';
import ProductList from './pages/ProductList';
import ProductDetail from './pages/ProductDetail';
import Cart from './pages/Cart';
import Checkout from './pages/Checkout';
import PaymentMethodSelect from './pages/PaymentMethodSelect';
import PaymentProcess from './pages/PaymentProcess';
import OrderSuccess from './pages/OrderSuccess';
import OrderFailed from './pages/OrderFailed';
import Login from './pages/Login';
import Register from './pages/Register';
import NotFound from './pages/NotFound';
import UnauthorizedPage from './pages/UnauthorizedPage';
import UserProfile from './pages/UserProfile'; // 新增用戶個人中心頁面

// 通用組件
import Header from './components/common/Header';
import Footer from './components/common/Footer';

import './App.css';

function App() {
  return (
    <Router>
      <AuthProvider>
        <CartProvider>
        <div className="app">
          <Header />
          <main className="main-content">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/products" element={<ProductList />} />
              <Route path="/products/:productId" element={<ProductDetail />} />
              <Route path="/cart" element={<Cart />} />
              
              {/* 需要登入的路由 */}
              <Route path="/checkout" element={
                <ProtectedRoute>
                  <Checkout />
                </ProtectedRoute>
              } />
              
              {/* 用戶個人中心路由 */}
              <Route path="/profile" element={
                <ProtectedRoute>
                  <UserProfile />
                </ProtectedRoute>
              } />
              
              {/* 支付相關路由 */}
              <Route path="/orders/:orderId/payment" element={
                <ProtectedRoute>
                  <PaymentMethodSelect />
                </ProtectedRoute>
              } />
              <Route path="/payment/:transactionId/process" element={
                <ProtectedRoute>
                  <PaymentProcess />
                </ProtectedRoute>
              } />
              <Route path="/orders/:orderId/success" element={
                <ProtectedRoute>
                  <OrderSuccess />
                </ProtectedRoute>
              } />
              <Route path="/orders/:orderId/failed" element={
                <ProtectedRoute>
                  <OrderFailed />
                </ProtectedRoute>
              } />
              
              {/* 認證相關路由 */}
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/unauthorized" element={<UnauthorizedPage />} />
              
              {/* 404頁面 */}
              <Route path="*" element={<NotFound />} />
            </Routes>
          </main>
          <Footer />
        </div>
        </CartProvider>
      </AuthProvider>
    </Router>
  );
}

export default App;