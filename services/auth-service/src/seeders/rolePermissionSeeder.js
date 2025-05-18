const { Role, Permission } = require('../models');
const logger = require('../utils/logger');

/**
 * 創建默認角色和權限
 */
module.exports = async () => {
  try {
    // 創建基本角色
    const roles = [
      { name: 'admin', description: '系統管理員', is_system: true },
      { name: 'user', description: '普通用戶', is_system: true },
      { name: 'guest', description: '訪客', is_system: true }
    ];

    for (const roleData of roles) {
      const [role, created] = await Role.findOrCreate({
        where: { name: roleData.name },
        defaults: roleData
      });

      if (created) {
        logger.info(`創建角色: ${role.name}`);
      }
    }

    // 創建基本權限
    const permissions = [
      // 用戶相關權限
      { name: 'user:read', description: '查看用戶', resource: 'user', action: 'read' },
      { name: 'user:create', description: '創建用戶', resource: 'user', action: 'create' },
      { name: 'user:update', description: '更新用戶', resource: 'user', action: 'update' },
      { name: 'user:delete', description: '刪除用戶', resource: 'user', action: 'delete' },
      
      // 角色相關權限
      { name: 'role:read', description: '查看角色', resource: 'role', action: 'read' },
      { name: 'role:create', description: '創建角色', resource: 'role', action: 'create' },
      { name: 'role:update', description: '更新角色', resource: 'role', action: 'update' },
      { name: 'role:delete', description: '刪除角色', resource: 'role', action: 'delete' },
      
      // 商品相關權限
      { name: 'product:read', description: '查看商品', resource: 'product', action: 'read' },
      { name: 'product:create', description: '創建商品', resource: 'product', action: 'create' },
      { name: 'product:update', description: '更新商品', resource: 'product', action: 'update' },
      { name: 'product:delete', description: '刪除商品', resource: 'product', action: 'delete' },
      
      // 訂單相關權限
      { name: 'order:read', description: '查看訂單', resource: 'order', action: 'read' },
      { name: 'order:create', description: '創建訂單', resource: 'order', action: 'create' },
      { name: 'order:update', description: '更新訂單', resource: 'order', action: 'update' },
      { name: 'order:delete', description: '刪除訂單', resource: 'order', action: 'delete' }
    ];

    for (const permData of permissions) {
      const [permission, created] = await Permission.findOrCreate({
        where: { name: permData.name },
        defaults: permData
      });

      if (created) {
        logger.info(`創建權限: ${permission.name}`);
      }
    }

    // 為角色分配權限
    const adminRole = await Role.findOne({ where: { name: 'admin' } });
    const userRole = await Role.findOne({ where: { name: 'user' } });
    const guestRole = await Role.findOne({ where: { name: 'guest' } });

    // 管理員擁有所有權限
    const allPermissions = await Permission.findAll();
    await adminRole.setPermissions(allPermissions);
    logger.info('管理員角色已分配所有權限');

    // 普通用戶擁有基本權限
    const userPermissions = await Permission.findAll({
      where: {
        name: [
          'user:read',
          'product:read',
          'order:read',
          'order:create'
        ]
      }
    });
    await userRole.setPermissions(userPermissions);
    logger.info('普通用戶角色已分配基本權限');

    // 訪客只有讀取權限
    const guestPermissions = await Permission.findAll({
      where: {
        name: [
          'product:read'
        ]
      }
    });
    await guestRole.setPermissions(guestPermissions);
    logger.info('訪客角色已分配讀取權限');

  } catch (error) {
    logger.error('創建默認角色和權限失敗:', error);
    throw error;
  }
};