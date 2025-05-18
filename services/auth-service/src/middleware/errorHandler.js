const AppError = require('../utils/appError');
const logger = require('../utils/logger');

/**
 * 全局錯誤處理中間件
 */
module.exports = (err, req, res, next) => {
  // 複製錯誤對象，避免修改原始錯誤
  let error = { ...err };
  error.message = err.message;
  
  // 記錄錯誤
  logger.error(`${req.method} ${req.path} - ${err.message}`, { 
    stack: err.stack,
    body: req.body,
    params: req.params,
    query: req.query
  });

  // Sequelize唯一約束錯誤
  if (err.name === 'SequelizeUniqueConstraintError') {
    const message = err.errors.map(e => e.message).join(', ');
    error = new AppError(message, 400);
  }

  // Sequelize驗證錯誤
  if (err.name === 'SequelizeValidationError') {
    const message = err.errors.map(e => e.message).join(', ');
    error = new AppError(message, 400);
  }

  // JWT錯誤
  if (err.name === 'JsonWebTokenError') {
    error = new AppError('無效的令牌', 401);
  }

  // JWT過期錯誤
  if (err.name === 'TokenExpiredError') {
    error = new AppError('令牌已過期', 401);
  }

  // 發送錯誤響應
  if (error.isOperational) {
    // 已知的操作錯誤
    res.status(error.statusCode).json({
      status: error.status,
      message: error.message
    });
  } else {
    // 未知錯誤
    console.error('ERROR 💥', err);
    res.status(500).json({
      status: 'error',
      message: '發生了錯誤'
    });
  }
};