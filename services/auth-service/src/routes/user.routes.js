const express = require('express');
const { body, param } = require('express-validator');
const userController = require('../controllers/user.controller');
const validateRequest = require('../middleware/validateRequest');
const authenticate = require('../middleware/authenticate');
const authorize = require('../middleware/authorize');

const router = express.Router();

// 所有路由需要認證
router.use(authenticate);

// 獲取當前用戶資料
router.get('/me', userController.getCurrentUser);

// 更新當前用戶資料
router.put(
  '/me',
  [
    body('first_name').optional().isLength({ max: 50 }),
    body('last_name').optional().isLength({ max: 50 }),
    body('phone').optional().isLength({ max: 20 }),
    validateRequest
  ],
  userController.updateCurrentUser
);

// 更新當前用戶密碼
router.put(
  '/me/password',
  [
    body('currentPassword').notEmpty().withMessage('當前密碼不能為空'),
    body('newPassword').isLength({ min: 6 }).withMessage('新密碼長度至少為6個字符'),
    validateRequest
  ],
  userController.updatePassword
);

// 以下路由需要管理員權限
router.use(authorize('admin'));

// 獲取所有用戶
router.get('/', userController.getAllUsers);

// 獲取特定用戶
router.get('/:id', userController.getUserById);

// 創建用戶 (管理員)
router.post(
  '/',
  [
    body('username').isLength({ min: 3, max: 50 }).withMessage('用戶名長度必須在3到50個字符之間'),
    body('email').isEmail().withMessage('請提供有效的電子郵件地址'),
    body('password').isLength({ min: 6 }).withMessage('密碼長度至少為6個字符'),
    body('roles').optional().isArray(),
    validateRequest
  ],
  userController.createUser
);

// 更新用戶
router.put(
  '/:id',
  [
    param('id').isUUID().withMessage('無效的用戶ID'),
    body('username').optional().isLength({ min: 3, max: 50 }),
    body('email').optional().isEmail(),
    body('status').optional().isIn(['active', 'inactive', 'suspended', 'pending']),
    body('roles').optional().isArray(),
    validateRequest
  ],
  userController.updateUser
);

// 刪除用戶
router.delete(
  '/:id',
  [
    param('id').isUUID().withMessage('無效的用戶ID'),
    validateRequest
  ],
  userController.deleteUser
);

module.exports = router;