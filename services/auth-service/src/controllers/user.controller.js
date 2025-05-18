const bcrypt = require('bcrypt');
const { User, Role, UserProfile } = require('../models');
const AppError = require('../utils/appError');
const logger = require('../utils/logger');

// 獲取當前用戶資料
exports.getCurrentUser = async (req, res, next) => {
  try {
    const userId = req.user.id;

    const user = await User.findByPk(userId, {
      include: [
        { model: Role },
        { model: UserProfile }
      ],
      attributes: { exclude: ['password_hash', 'salt'] }
    });

    if (!user) {
      return next(new AppError('用戶不存在', 404));
    }

    res.status(200).json({
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        first_name: user.first_name,
        last_name: user.last_name,
        phone: user.phone,
        status: user.status,
        email_verified: user.email_verified,
        phone_verified: user.phone_verified,
        last_login_at: user.last_login_at,
        created_at: user.created_at,
        updated_at: user.updated_at,
        roles: user.Roles.map(role => ({
          id: role.id,
          name: role.name,
          description: role.description
        })),
        profile: user.UserProfile
      }
    });
  } catch (error) {
    logger.error('獲取當前用戶資料失敗:', error);
    next(error);
  }
};

// 更新當前用戶資料
exports.updateCurrentUser = async (req, res, next) => {
  try {
    const userId = req.user.id;
    const { first_name, last_name, phone } = req.body;
    const profileData = req.body.profile || {};

    // 更新用戶基本資料
    const user = await User.findByPk(userId);
    if (!user) {
      return next(new AppError('用戶不存在', 404));
    }

    await user.update({
      first_name: first_name !== undefined ? first_name : user.first_name,
      last_name: last_name !== undefined ? last_name : user.last_name,
      phone: phone !== undefined ? phone : user.phone
    });

    // 更新用戶個人資料
    if (Object.keys(profileData).length > 0) {
      let profile = await UserProfile.findByPk(userId);
      if (!profile) {
        profile = await UserProfile.create({
          user_id: userId,
          ...profileData
        });
      } else {
        await profile.update(profileData);
      }
    }

    res.status(200).json({
      message: '用戶資料更新成功',
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        first_name: user.first_name,
        last_name: user.last_name,
        phone: user.phone
      }
    });
  } catch (error) {
    logger.error('更新當前用戶資料失敗:', error);
    next(error);
  }
};

// 更新當前用戶密碼
exports.updatePassword = async (req, res, next) => {
  try {
    const userId = req.user.id;
    const { currentPassword, newPassword } = req.body;

    // 查找用戶
    const user = await User.findByPk(userId);
    if (!user) {
      return next(new AppError('用戶不存在', 404));
    }

    // 驗證當前密碼
    const isPasswordValid = await bcrypt.compare(currentPassword, user.password_hash);
    if (!isPasswordValid) {
      return next(new AppError('當前密碼不正確', 400));
    }

    // 生成新的密碼哈希
    const salt = await bcrypt.genSalt(10);
    const passwordHash = await bcrypt.hash(newPassword, salt);

    // 更新用戶密碼
    await user.update({
      password_hash: passwordHash,
      salt
    });

    res.status(200).json({ message: '密碼更新成功' });
  } catch (error) {
    logger.error('更新密碼失敗:', error);
    next(error);
  }
};

// 獲取所有用戶 (管理員)
exports.getAllUsers = async (req, res, next) => {
  try {
    const users = await User.findAll({
      include: [{ model: Role }],
      attributes: { exclude: ['password_hash', 'salt'] }
    });

    res.status(200).json({
      count: users.length,
      users: users.map(user => ({
        id: user.id,
        username: user.username,
        email: user.email,
        first_name: user.first_name,
        last_name: user.last_name,
        status: user.status,
        created_at: user.created_at,
        roles: user.Roles.map(role => ({
          id: role.id,
          name: role.name
        }))
      }))
    });
  } catch (error) {
    logger.error('獲取所有用戶失敗:', error);
    next(error);
  }
};

// 獲取特定用戶 (管理員)
exports.getUserById = async (req, res, next) => {
  try {
    const { id } = req.params;

    const user = await User.findByPk(id, {
      include: [
        { model: Role },
        { model: UserProfile }
      ],
      attributes: { exclude: ['password_hash', 'salt'] }
    });

    if (!user) {
      return next(new AppError('用戶不存在', 404));
    }

    res.status(200).json({
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        first_name: user.first_name,
        last_name: user.last_name,
        phone: user.phone,
        status: user.status,
        email_verified: user.email_verified,
        phone_verified: user.phone_verified,
        last_login_at: user.last_login_at,
        created_at: user.created_at,
        updated_at: user.updated_at,
        roles: user.Roles.map(role => ({
          id: role.id,
          name: role.name,
          description: role.description
        })),
        profile: user.UserProfile
      }
    });
  } catch (error) {
    logger.error('獲取用戶失敗:', error);
    next(error);
  }
};

// 創建用戶 (管理員)
exports.createUser = async (req, res, next) => {
  try {
    const { username, email, password, first_name, last_name, phone, roles } = req.body;

    // 檢查用戶名和電子郵件是否已存在
    const existingUser = await User.findOne({
      where: {
        [sequelize.Op.or]: [
          { username },
          { email }
        ]
      }
    });

    if (existingUser) {
      if (existingUser.username === username) {
        return next(new AppError('用戶名已被使用', 400));
      }
      if (existingUser.email === email) {
        return next(new AppError('電子郵件已被使用', 400));
      }
    }

    // 生成鹽和密碼哈希
    const salt = await bcrypt.genSalt(10);
    const passwordHash = await bcrypt.hash(password, salt);

    // 創建用戶
    const user = await User.create({
      username,
      email,
      password_hash: passwordHash,
      salt,
      first_name,
      last_name,
      phone,
      status: 'active',
      email_verified: true
    });

    // 添加角色
    if (roles && roles.length > 0) {
      const userRoles = await Role.findAll({
        where: {
          name: roles
        }
      });
      await user.setRoles(userRoles);
    } else {
      // 默認添加 'user' 角色
      const userRole = await Role.findOne({ where: { name: 'user' } });
      if (userRole) {
        await user.addRole(userRole);
      }
    }

    // 創建用戶個人資料
    await user.createUserProfile();

    // 獲取完整的用戶信息（包括角色）
    const createdUser = await User.findByPk(user.id, {
      include: [{ model: Role }],
      attributes: { exclude: ['password_hash', 'salt'] }
    });

    res.status(201).json({
      message: '用戶創建成功',
      user: {
        id: createdUser.id,
        username: createdUser.username,
        email: createdUser.email,
        first_name: createdUser.first_name,
        last_name: createdUser.last_name,
        status: createdUser.status,
        roles: createdUser.Roles.map(role => ({
          id: role.id,
          name: role.name
        }))
      }
    });
  } catch (error) {
    logger.error('創建用戶失敗:', error);
    next(error);
  }
};

// 更新用戶 (管理員)
exports.updateUser = async (req, res, next) => {
  try {
    const { id } = req.params;
    const { username, email, first_name, last_name, phone, status, roles } = req.body;

    // 查找用戶
    const user = await User.findByPk(id);
    if (!user) {
      return next(new AppError('用戶不存在', 404));
    }

    // 檢查用戶名和電子郵件是否已被其他用戶使用
    if (username && username !== user.username) {
      const existingUsername = await User.findOne({ where: { username } });
      if (existingUsername) {
        return next(new AppError('用戶名已被使用', 400));
      }
    }

    if (email && email !== user.email) {
      const existingEmail = await User.findOne({ where: { email } });
      if (existingEmail) {
        return next(new AppError('電子郵件已被使用', 400));
      }
    }

    // 更新用戶基本信息
    const updateData = {};
    if (username) updateData.username = username;
    if (email) updateData.email = email;
    if (first_name !== undefined) updateData.first_name = first_name;
    if (last_name !== undefined) updateData.last_name = last_name;
    if (phone !== undefined) updateData.phone = phone;
    if (status) updateData.status = status;

    await user.update(updateData);

    // 更新角色
    if (roles && roles.length > 0) {
      const userRoles = await Role.findAll({
        where: {
          name: roles
        }
      });
      await user.setRoles(userRoles);
    }

    // 獲取更新後的用戶信息
    const updatedUser = await User.findByPk(id, {
      include: [{ model: Role }],
      attributes: { exclude: ['password_hash', 'salt'] }
    });

    res.status(200).json({
      message: '用戶更新成功',
      user: {
        id: updatedUser.id,
        username: updatedUser.username,
        email: updatedUser.email,
        first_name: updatedUser.first_name,
        last_name: updatedUser.last_name,
        phone: updatedUser.phone,
        status: updatedUser.status,
        roles: updatedUser.Roles.map(role => ({
          id: role.id,
          name: role.name
        }))
      }
    });
  } catch (error) {
    logger.error('更新用戶失敗:', error);
    next(error);
  }
};

// 刪除用戶 (管理員)
exports.deleteUser = async (req, res, next) => {
  try {
    const { id } = req.params;

    // 檢查用戶是否存在
    const user = await User.findByPk(id);
    if (!user) {
      return next(new AppError('用戶不存在', 404));
    }

    // 不允許刪除管理員
    const isAdmin = await user.hasRole('admin');
    if (isAdmin) {
      return next(new AppError('不允許刪除管理員用戶', 403));
    }

    // 刪除用戶
    await user.destroy();

    res.status(200).json({
      message: '用戶刪除成功'
    });
  } catch (error) {
    logger.error('刪除用戶失敗:', error);
    next(error);
  }
};