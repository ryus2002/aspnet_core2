const { Role, Permission, User } = require('../models');
const AppError = require('../utils/appError');
const logger = require('../utils/logger');

/**
 * 獲取所有角色
 * @route GET /api/roles
 * @access 管理員
 */
exports.getAllRoles = async (req, res, next) => {
  try {
    const roles = await Role.findAll({
      include: [{ model: Permission }]
    });

    res.status(200).json({
      status: 'success',
      count: roles.length,
      data: {
      roles: roles.map(role => ({
        id: role.id,
        name: role.name,
        description: role.description,
        is_system: role.is_system,
        permissions: role.Permissions.map(permission => ({
          id: permission.id,
          name: permission.name,
          resource: permission.resource,
          action: permission.action
        }))
      }))
  }
    });
  } catch (error) {
    logger.error('獲取所有角色失敗:', error);
    next(error);
  }
};

/**
 * 獲取特定角色
 * @route GET /api/roles/:id
 * @access 管理員
 */
exports.getRoleById = async (req, res, next) => {
  try {
    const { id } = req.params;
    const role = await Role.findByPk(id, {
      include: [{ model: Permission }]
    });

    if (!role) {
      return next(new AppError('角色不存在', 404));
    }

    res.status(200).json({
      status: 'success',
      data: {
        role: {
          id: role.id,
          name: role.name,
          description: role.description,
          is_system: role.is_system,
      permissions: role.Permissions.map(permission => ({
        id: permission.id,
        name: permission.name,
        resource: permission.resource,
        action: permission.action
      }))
  }
      }
    });
  } catch (error) {
    logger.error('獲取角色失敗:', error);
    next(error);
  }
};

/**
 * 創建角色
 * @route POST /api/roles
 * @access 管理員
 */
exports.createRole = async (req, res, next) => {
  try {
    const { name, description, permissions } = req.body;

    // 檢查角色名稱是否已存在
    const existingRole = await Role.findOne({ where: { name } });
    if (existingRole) {
      return next(new AppError('角色名稱已存在', 400));
    }

    // 創建角色
    const role = await Role.create({
      name,
      description,
      is_system: false
    });

    // 添加權限
    if (permissions && permissions.length > 0) {
      const rolePermissions = await Permission.findAll({
        where: {
          id: permissions
        }
      });
      await role.setPermissions(rolePermissions);
    }

    // 獲取完整角色信息
    const newRole = await Role.findByPk(role.id, {
      include: [{ model: Permission }]
    });

    res.status(201).json({
      status: 'success',
      data: {
        role: {
          id: newRole.id,
          name: newRole.name,
          description: newRole.description,
          is_system: newRole.is_system,
          permissions: newRole.Permissions.map(permission => ({
            id: permission.id,
            name: permission.name,
            resource: permission.resource,
            action: permission.action
          }))
        }
      }
    });
  } catch (error) {
    logger.error('創建角色失敗:', error);
    next(error);
  }
};

/**
 * 更新角色
 * @route PATCH /api/roles/:id
 * @access 管理員
 */
exports.updateRole = async (req, res, next) => {
  try {
    const { id } = req.params;
    const { name, description, permissions } = req.body;

    // 檢查角色是否存在
    const role = await Role.findByPk(id);
    if (!role) {
      return next(new AppError('角色不存在', 404));
    }

    // 檢查是否為系統角色
    if (role.is_system) {
      return next(new AppError('系統角色不能修改', 403));
    }

    // 檢查角色名稱是否已存在
    if (name && name !== role.name) {
      const existingRole = await Role.findOne({ where: { name } });
      if (existingRole) {
        return next(new AppError('角色名稱已存在', 400));
      }
    }

    // 更新角色
    await role.update({
      name: name || role.name,
      description: description || role.description
    });

    // 更新權限
    if (permissions && permissions.length > 0) {
      const rolePermissions = await Permission.findAll({
        where: {
          id: permissions
        }
      });
      await role.setPermissions(rolePermissions);
    }

    // 獲取更新後的角色信息
    const updatedRole = await Role.findByPk(id, {
      include: [{ model: Permission }]
    });

    res.status(200).json({
      status: 'success',
      data: {
        role: {
          id: updatedRole.id,
          name: updatedRole.name,
          description: updatedRole.description,
          is_system: updatedRole.is_system,
          permissions: updatedRole.Permissions.map(permission => ({
            id: permission.id,
            name: permission.name,
            resource: permission.resource,
            action: permission.action
          }))
        }
      }
    });
  } catch (error) {
    logger.error('更新角色失敗:', error);
    next(error);
  }
};

/**
 * 刪除角色
 * @route DELETE /api/roles/:id
 * @access 管理員
 */
exports.deleteRole = async (req, res, next) => {
  try {
    const { id } = req.params;

    // 檢查角色是否存在
    const role = await Role.findByPk(id);
    if (!role) {
      return next(new AppError('角色不存在', 404));
    }

    // 檢查是否為系統角色
    if (role.is_system) {
      return next(new AppError('系統角色不能刪除', 403));
    }

    // 檢查是否有用戶使用此角色
    const usersWithRole = await User.count({ where: { role_id: id } });
    if (usersWithRole > 0) {
      return next(new AppError('該角色正在使用中，無法刪除', 400));
    }

    // 刪除角色
    await role.destroy();

    res.status(204).json({
      status: 'success',
      data: null
    });
  } catch (error) {
    logger.error('刪除角色失敗:', error);
    next(error);
  }
};

/**
 * 獲取角色的所有權限
 * @route GET /api/roles/:id/permissions
 * @access 管理員
 */
exports.getRolePermissions = async (req, res, next) => {
  try {
    const { id } = req.params;

    // 檢查角色是否存在
    const role = await Role.findByPk(id, {
      include: [{ model: Permission }]
    });

    if (!role) {
      return next(new AppError('角色不存在', 404));
    }

    res.status(200).json({
      status: 'success',
      count: role.Permissions.length,
      data: {
        permissions: role.Permissions.map(permission => ({
          id: permission.id,
          name: permission.name,
          resource: permission.resource,
          action: permission.action
        }))
      }
    });
  } catch (error) {
    logger.error('獲取角色權限失敗:', error);
    next(error);
  }
};

/**
 * 為角色分配權限
 * @route POST /api/roles/:id/permissions
 * @access 管理員
 */
exports.assignPermissionsToRole = async (req, res, next) => {
  try {
    const { id } = req.params;
    const { permissions } = req.body;

    // 檢查角色是否存在
    const role = await Role.findByPk(id);
    if (!role) {
      return next(new AppError('角色不存在', 404));
    }

    // 檢查是否為系統角色
    if (role.is_system) {
      return next(new AppError('系統角色的權限不能修改', 403));
    }

    // 檢查權限是否存在
    if (!permissions || !Array.isArray(permissions) || permissions.length === 0) {
      return next(new AppError('請提供有效的權限ID列表', 400));
    }

    const permissionsToAssign = await Permission.findAll({
      where: {
        id: permissions
      }
    });

    if (permissionsToAssign.length !== permissions.length) {
      return next(new AppError('部分權限ID無效', 400));
    }

    // 分配權限
    await role.setPermissions(permissionsToAssign);

    // 獲取更新後的角色信息
    const updatedRole = await Role.findByPk(id, {
      include: [{ model: Permission }]
    });

    res.status(200).json({
      status: 'success',
      data: {
        role: {
          id: updatedRole.id,
          name: updatedRole.name,
          description: updatedRole.description,
          is_system: updatedRole.is_system,
          permissions: updatedRole.Permissions.map(permission => ({
            id: permission.id,
            name: permission.name,
            resource: permission.resource,
            action: permission.action
          }))
        }
      }
    });
  } catch (error) {
    logger.error('為角色分配權限失敗:', error);
    next(error);
  }
};
