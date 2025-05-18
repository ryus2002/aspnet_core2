const express = require('express');
const authRoutes = require('./auth.routes');
const userRoutes = require('./user.routes');
const roleRoutes = require('./role.routes');

const router = express.Router();

// 認證相關路由
router.use('/auth', authRoutes);

// 用戶相關路由
router.use('/users', userRoutes);

// 角色相關路由
router.use('/roles', roleRoutes);

module.exports = router;