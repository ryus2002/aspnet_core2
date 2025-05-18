const { validationResult } = require('express-validator');
const AppError = require('../utils/appError');

/**
 * 驗證請求中間件
 * 用於驗證express-validator的驗證結果
 */
module.exports = (req, res, next) => {
  const errors = validationResult(req);
  if (!errors.isEmpty()) {
    const errorMessages = errors.array().map(err => err.msg).join(', ');
    return next(new AppError(errorMessages, 400));
  }
  next();
};