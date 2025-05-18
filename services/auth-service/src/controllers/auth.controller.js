const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');
const { v4: uuidv4 } = require('uuid');
const { User, Role, RefreshToken, LoginHistory } = require('../models');
const AppError = require('../utils/appError');
const logger = require('../utils/logger');

// 註冊新用戶
exports.register = async (req, res, next) => {
  try {
    const { username, email, password, firstName, lastName, phone } = req.body;

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

    // 生成電子郵件驗證令牌
    const verificationToken = uuidv4();
    const verificationTokenExpires = new Date(Date.now() + 24 * 60 * 60 * 1000); // 24小時後過期

    // 創建用戶
    const user = await User.create({
      username,
      email,
      password_hash: passwordHash,
      salt,
      first_name: firstName,
      last_name: lastName,
      phone,
      verification_token: verificationToken,
      verification_token_expires: verificationTokenExpires
    });

    // 獲取普通用戶角色
    const userRole = await Role.findOne({ where: { name: 'user' } });
    if (userRole) {
      await user.addRole(userRole);
    }

    // 創建用戶個人資料
    await user.createUserProfile();

    // 在實際環境中，應該發送驗證電子郵件
    // 這裡僅返回驗證令牌用於測試
    res.status(201).json({
      message: '用戶註冊成功',
      verificationToken,
      user: {
        id: user.id,
        username: user.username,
        email: user.email
      }
    });
  } catch (error) {
    logger.error('註冊失敗:', error);
    next(error);
  }
};

// 用戶登入
exports.login = async (req, res, next) => {
  try {
    const { username, email, password } = req.body;
    const ipAddress = req.ip;
    const userAgent = req.headers['user-agent'];

    // 檢查是否提供了用戶名或電子郵件
    if (!username && !email) {
      return next(new AppError('請提供用戶名或電子郵件', 400));
    }

    // 查找用戶
    const user = await User.findOne({
      where: username ? { username } : { email },
      include: [{ model: Role }]
    });

    // 記錄登入嘗試
    const logLoginAttempt = async (success, reason = null) => {
      if (user) {
        await LoginHistory.create({
          user_id: user.id,
          login_at: new Date(),
          ip_address: ipAddress,
          user_agent: userAgent,
          success,
          failure_reason: reason
        });
      }
    };

    // 用戶不存在
    if (!user) {
      await logLoginAttempt(false, '用戶不存在');
      return next(new AppError('無效的憑證', 401));
    }

    // 檢查用戶狀態
    if (user.status !== 'active') {
      await logLoginAttempt(false, `用戶狀態為 ${user.status}`);
      return next(new AppError('用戶賬戶未激活', 401));
    }

    // 驗證密碼
    const isPasswordValid = await bcrypt.compare(password, user.password_hash);
    if (!isPasswordValid) {
      await logLoginAttempt(false, '密碼錯誤');
      return next(new AppError('無效的憑證', 401));
    }

    // 生成 JWT 令牌
    const roles = user.Roles.map(role => role.name);
    const accessToken = generateJwtToken(user, roles);
    const refreshToken = await generateRefreshToken(user, ipAddress);

    // 更新用戶最後登入時間和 IP
    await user.update({
      last_login_at: new Date(),
      last_login_ip: ipAddress
    });

    // 記錄成功登入
    await logLoginAttempt(true);

    // 返回令牌和用戶信息
    res.status(200).json({
      message: '登入成功',
      accessToken,
      refreshToken: refreshToken.token,
      user: {
        id: user.id,
        username: user.username,
        email: user.email,
        roles
      }
    });
  } catch (error) {
    logger.error('登入失敗:', error);
    next(error);
  }
};

// 刷新令牌
exports.refreshToken = async (req, res, next) => {
  try {
    const { refreshToken } = req.body;
    const ipAddress = req.ip;

    // 查找刷新令牌
    const token = await RefreshToken.findOne({
      where: { token: refreshToken },
      include: [{ model: User, include: [{ model: Role }] }]
    });

    // 令牌不存在或已撤銷
    if (!token || token.revoked) {
      return next(new AppError('無效的刷新令牌', 401));
    }

    // 令牌已過期
    if (new Date() > new Date(token.expires_at)) {
      return next(new AppError('刷新令牌已過期', 401));
    }

    // 生成新的令牌
    const user = token.User;
    const roles = user.Roles.map(role => role.name);
    const accessToken = generateJwtToken(user, roles);
    const newRefreshToken = await generateRefreshToken(user, ipAddress);

    // 撤銷舊令牌
    await token.update({
      revoked: true,
      revoked_at: new Date(),
      revoked_by_ip: ipAddress,
      replaced_by_token: newRefreshToken.token
    });

    // 返回新令牌
    res.status(200).json({
      message: '令牌刷新成功',
      accessToken,
      refreshToken: newRefreshToken.token
    });
  } catch (error) {
    logger.error('刷新令牌失敗:', error);
    next(error);
  }
};

// 登出
exports.logout = async (req, res, next) => {
  try {
    const { refreshToken } = req.body;
    const ipAddress = req.ip;

    if (!refreshToken) {
      return res.status(200).json({ message: '登出成功' });
    }

    // 查找並撤銷刷新令牌
    const token = await RefreshToken.findOne({ where: { token: refreshToken } });
    if (token && !token.revoked) {
      await token.update({
        revoked: true,
        revoked_at: new Date(),
        revoked_by_ip: ipAddress
      });
    }

    res.status(200).json({ message: '登出成功' });
  } catch (error) {
    logger.error('登出失敗:', error);
    next(error);
  }
};

// 驗證電子郵件
exports.verifyEmail = async (req, res, next) => {
  try {
    const { token } = req.params;

    // 查找用戶
    const user = await User.findOne({
      where: {
        verification_token: token,
        verification_token_expires: { [sequelize.Op.gt]: new Date() }
      }
    });

    if (!user) {
      return next(new AppError('無效或已過期的驗證令牌', 400));
    }

    // 更新用戶狀態
    await user.update({
      email_verified: true,
      status: 'active',
      verification_token: null,
      verification_token_expires: null
    });

    res.status(200).json({ message: '電子郵件驗證成功' });
  } catch (error) {
    logger.error('電子郵件驗證失敗:', error);
    next(error);
  }
};

// 請求重置密碼
exports.forgotPassword = async (req, res, next) => {
  try {
    const { email } = req.body;

    // 查找用戶
    const user = await User.findOne({ where: { email } });
    if (!user) {
      // 不透露用戶是否存在
      return res.status(200).json({ message: '如果該電子郵件存在，我們將發送重置密碼的說明' });
    }

    // 生成重置令牌
    const resetToken = uuidv4();
    const resetTokenExpires = new Date(Date.now() + 1 * 60 * 60 * 1000); // 1小時後過期

    // 更新用戶
    await user.update({
      reset_password_token: resetToken,
      reset_password_expires: resetTokenExpires
    });

    // 在實際環境中，應該發送重置密碼電子郵件
    // 這裡僅返回重置令牌用於測試
    res.status(200).json({
      message: '如果該電子郵件存在，我們將發送重置密碼的說明',
      resetToken // 僅用於測試
    });
  } catch (error) {
    logger.error('請求重置密碼失敗:', error);
    next(error);
  }
};

// 重置密碼
exports.resetPassword = async (req, res, next) => {
  try {
    const { token } = req.params;
    const { password } = req.body;

    // 查找用戶
    const user = await User.findOne({
      where: {
        reset_password_token: token,
        reset_password_expires: { [sequelize.Op.gt]: new Date() }
      }
    });

    if (!user) {
      return next(new AppError('無效或已過期的重置令牌', 400));
    }

    // 生成新的密碼哈希
    const salt = await bcrypt.genSalt(10);
    const passwordHash = await bcrypt.hash(password, salt);

    // 更新用戶
    await user.update({
      password_hash: passwordHash,
      salt,
      reset_password_token: null,
      reset_password_expires: null
    });

    // 撤銷所有刷新令牌
    await RefreshToken.update(
      { revoked: true, revoked_at: new Date(), revoked_by_ip: req.ip },
      { where: { user_id: user.id, revoked: false } }
    );

    res.status(200).json({ message: '密碼重置成功' });
  } catch (error) {
    logger.error('重置密碼失敗:', error);
    next(error);
  }
};

// 生成 JWT 令牌
function generateJwtToken(user, roles) {
  return jwt.sign(
    {
      id: user.id,
      username: user.username,
      email: user.email,
      roles
    },
    process.env.JWT_SECRET,
    { expiresIn: process.env.JWT_EXPIRES_IN }
  );
}

// 生成刷新令牌
async function generateRefreshToken(user, ipAddress) {
  // 計算過期時間
  const expiresIn = process.env.JWT_REFRESH_EXPIRES_IN || '7d';
  const expiresAt = new Date();
  
  if (expiresIn.endsWith('d')) {
    expiresAt.setDate(expiresAt.getDate() + parseInt(expiresIn));
  } else if (expiresIn.endsWith('h')) {
    expiresAt.setHours(expiresAt.getHours() + parseInt(expiresIn));
  } else {
    // 默認7天
    expiresAt.setDate(expiresAt.getDate() + 7);
  }

  // 創建刷新令牌
  const token = uuidv4();
  const refreshToken = await RefreshToken.create({
    user_id: user.id,
    token,
    expires_at: expiresAt,
    created_by_ip: ipAddress
  });

  return refreshToken;
}