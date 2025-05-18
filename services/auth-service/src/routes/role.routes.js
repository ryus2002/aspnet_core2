const express = require('express');
const { body, param } = require('express-validator');
const roleController = require('../controllers/role.controller');
const validateRequest = require('../middleware/validateRequest');
const authenticate = require('../middleware/authenticate');
const authorize = require('../middleware/authorize');

const router = express.Router();

// 所有路由需要認證和管理員權限
router.use(authenticate, authorize('admin'));

// 獲取所有角色
router.get('/', roleController.getAllRoles);

// 獲取特定角色
router.get(
  '/:id',
  [
    param('id').isUUID().withMessage('無效的角色ID'),
    validateRequest
  ],
  roleController.getRoleById
);

// 創建角色
router.post(
  '/',
  [
    body('name').isLength({ min: 2, max: 50 }).withMessage('角色名稱長度必須在2到50個字符之間'),
    body('description').optional(),
    body('permissions').optional().isArray(),
    validateRequest
  ],
  roleController.createRole
);

// 更新角色
router.put(
  '/:id',
  [
    param('id').isUUID().withMessage('無效的角色ID'),
    body('name').optional().isLength({ min: 2, max: 50 }),
    body('description').optional(),
    body('permissions').optional().isArray(),
    validateRequest
  ],
  roleController.updateRole
);

// 刪除角色
router.delete(
  '/:id',
  [
    param('id').isUUID().withMessage('無效的角色ID'),
    validateRequest
  ],
  roleController.deleteRole
);

// 獲取角色的權限
router.get(
  '/:id/permissions',
  [
    param('id').isUUID().withMessage('無效的角色ID'),
    validateRequest
  ],
  roleController.getRolePermissions
);

// 更新角色的權限
router.put(
  '/:id/permissions',
  [
    param('id').isUUID().withMessage('無效的角色ID'),
    body('permissions').isArray().withMessage('權限必須是陣列'),
    validateRequest
  ],
  roleController.updateRolePermissions
);

module.exports = router;