const express = require('express');
const cors = require('cors');
const morgan = require('morgan');
const { createProxyMiddleware } = require('http-proxy-middleware');
const jwt = require('jsonwebtoken');

const app = express();
const PORT = process.env.PORT || 3000;
const JWT_SECRET = process.env.JWT_SECRET || 'your-secret-key';

// 服務地址
const AUTH_SERVICE_URL = process.env.AUTH_SERVICE_URL || 'http://localhost:3001';
const PRODUCT_SERVICE_URL = process.env.PRODUCT_SERVICE_URL || 'http://localhost:3002';
const ORDER_SERVICE_URL = process.env.ORDER_SERVICE_URL || 'http://localhost:3003';
const PAYMENT_SERVICE_URL = process.env.PAYMENT_SERVICE_URL || 'http://localhost:3004';

// 中間件
app.use(cors());
app.use(morgan('dev'));
app.use(express.json());

// JWT 驗證中間件
const authenticateJWT = (req, res, next) => {
  const authHeader = req.headers.authorization;
  
  if (authHeader) {
    const token = authHeader.split(' ')[1];
    
    jwt.verify(token, JWT_SECRET, (err, user) => {
      if (err) {
        return res.status(403).json({ message: '令牌無效或已過期' });
      }
      
      req.user = user;
      next();
    });
  } else {
    res.status(401).json({ message: '未提供認證令牌' });
  }
};

// 公開路由
const publicRoutes = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/products',
  '/api/categories'
];

// 檢查是否為公開路由
const isPublicRoute = (req) => {
  return publicRoutes.some(route => {
    if (route.endsWith('*')) {
      return req.path.startsWith(route.slice(0, -1));
    }
    return req.path === route || req.path.startsWith(`${route}/`);
  });
};

// 路由中間件
app.use((req, res, next) => {
  if (isPublicRoute(req)) {
    return next();
  }
  
  authenticateJWT(req, res, next);
});

// 代理配置
const proxyOptions = {
  changeOrigin: true,
  pathRewrite: {
    '^/api/auth': '/api',
    '^/api/products': '/api/products',
    '^/api/orders': '/api/orders',
    '^/api/payments': '/api/payments'
  }
};

// 代理路由
app.use('/api/auth', createProxyMiddleware({
  ...proxyOptions,
  target: AUTH_SERVICE_URL
}));

app.use('/api/products', createProxyMiddleware({
  ...proxyOptions,
  target: PRODUCT_SERVICE_URL
}));

app.use('/api/categories', createProxyMiddleware({
  ...proxyOptions,
  target: PRODUCT_SERVICE_URL
}));

app.use('/api/orders', createProxyMiddleware({
  ...proxyOptions,
  target: ORDER_SERVICE_URL
}));

app.use('/api/carts', createProxyMiddleware({
  ...proxyOptions,
  target: ORDER_SERVICE_URL
}));

app.use('/api/payments', createProxyMiddleware({
  ...proxyOptions,
  target: PAYMENT_SERVICE_URL
}));

app.use('/api/payment-methods', createProxyMiddleware({
  ...proxyOptions,
  target: PAYMENT_SERVICE_URL
}));

app.use('/api/payment-transactions', createProxyMiddleware({
  ...proxyOptions,
  target: PAYMENT_SERVICE_URL
}));

// 健康檢查
app.get('/health', (req, res) => {
  res.json({ status: 'UP', services: {
    auth: AUTH_SERVICE_URL,
    product: PRODUCT_SERVICE_URL,
    order: ORDER_SERVICE_URL,
    payment: PAYMENT_SERVICE_URL
  }});
});

// 404 處理
app.use((req, res) => {
  res.status(404).json({ message: '找不到請求的資源' });
});

// 錯誤處理
app.use((err, req, res, next) => {
  console.error('API Gateway 錯誤:', err);
  res.status(500).json({ message: '服務器錯誤' });
});

// 啟動服務器
app.listen(PORT, () => {
  console.log(`API Gateway 運行在端口 ${PORT}`);
});