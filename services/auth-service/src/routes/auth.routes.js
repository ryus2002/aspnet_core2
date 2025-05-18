const express = require('express');
const { body } = require('express-validator');
const authController = require('../controllers/auth.controller');
const validateRequest = require('../middleware/validateRequest');
const authenticate = require('../middleware/authenticate');

const router = express.Router();

// 註冊
router.post(
  '/register',
  [
    body('username').isLength({ min: 3, max: 50 }).withMessage('用戶名長度必須在3到50個字符之間'),
    body('email').isEmail().withMessage('請提供有效的電子郵件地址'),
    body('password').isLength({ min: 6 }).withMessage('密碼長度至少為6個字符'),
    validateRequest
  ],
  authController.register
);

// 登入
router.post(
  '/login',
  [
    body('username').optional(),
    body('email').optional(),
    body('password').isLength({ min: 6 }).withMessage('密碼長度至少為6個字符'),
    validateRequest
  ],
  authController.login
);

// 刷新令牌
router.post(
  '/refresh-token',
  [
    body('refreshToken').notEmpty().withMessage('刷新令牌不能為空'),
    validateRequest
  ],
  authController.refreshToken
);

// 登出
router.post(
  '/logout',
  authenticate,
  authController.logout
);

// 驗證電子郵件
router.get(
  '/verify-email/:token',
  authController.verifyEmail
);

// 請求重置密碼
router.post(
  '/forgot-password',
  [
    body('email').isEmail().withMessage('請提供有效的電子郵件地址'),
    validateRequest
  ],
  authController.forgotPassword
);

// 重置密碼
router.post(
  '/reset-password/:token',
  [
    body('password').isLength({ min: 6 }).withMessage('密碼長度至少為6個字符'),
    validateRequest
  ],
  authController.resetPassword
);

// 檢查令牌有效性
router.get('/validate-token', authenticate, (req, res) => {
  res.status(200).json({
    message: '令牌有效',
    user: {
      id: req.user.id,
      username: req.user.username,
      email: req.user.email,
      roles: req.user.roles
    }
  });
});

module.exports = router;