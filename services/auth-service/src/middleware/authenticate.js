const jwt = require('jsonwebtoken');
const { User, Role } = require('../models');
const AppError = require('../utils/appError');

/**
 * 驗證JWT令牌並將用戶信息附加到請求對象
 */
module.exports = async (req, res, next) => {
  try {
    // 檢查Authorization頭部
    const authHeader = req.headers.authorization;
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      return next(new AppError('未提供認證令牌', 401));
    }

    // 提取令牌
    const token = authHeader.split(' ')[1];

    // 驗證令牌
    let decoded;
    try {
      decoded = jwt.verify(token, process.env.JWT_SECRET);
    } catch (error) {
      if (error.name === 'TokenExpiredError') {
        return next(new AppError('認證令牌已過期', 401));
      }
      return next(new AppError('無效的認證令牌', 401));
    }

    // 檢查用戶是否存在
    const user = await User.findByPk(decoded.id, {
      include: [{ model: Role }]
    });

    if (!user) {
      return next(new AppError('令牌對應的用戶不存在', 401));
    }

    if (user.status !== 'active') {
      return next(new AppError('用戶賬戶已被停用', 401));
    }

    // 將用戶信息附加到請求對象
    req.user = {
      id: user.id,
      username: user.username,
      email: user.email,
      roles: user.Roles.map(role => role.name)
    };

    next();
  } catch (error) {
    next(error);
  }
};