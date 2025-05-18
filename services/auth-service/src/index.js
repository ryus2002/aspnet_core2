require('dotenv').config();
const express = require('express');
const cors = require('cors');
const helmet = require('helmet');
const morgan = require('morgan');
const { sequelize } = require('./models');
const routes = require('./routes');
const errorHandler = require('./middleware/errorHandler');
const logger = require('./utils/logger');

// 創建Express應用
const app = express();
const PORT = process.env.PORT || 3001;

// 中間件
app.use(helmet());
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(morgan('combined', { stream: { write: message => logger.info(message.trim()) } }));

// 路由
app.use('/api', routes);
// 錯誤處理中間件
app.use(errorHandler);

// 啟動服務器
const startServer = async () => {
  try {
    // 同步數據庫模型
    await sequelize.sync({ alter: process.env.NODE_ENV === 'development' });
    logger.info('數據庫已同步');
    
    // 創建默認角色和權限
    await require('./seeders/rolePermissionSeeder')();
    logger.info('默認角色和權限已創建');

    // 啟動服務器
    app.listen(PORT, () => {
      logger.info(`認證服務運行在端口 ${PORT}`);
    });
  } catch (error) {
    logger.error('服務啟動失敗:', error);
    process.exit(1);
  }
};
startServer();

// 處理未捕獲的異常
process.on('uncaughtException', (error) => {
  logger.error('未捕獲的異常:', error);
  process.exit(1);
});

// 處理未處理的Promise拒絕
process.on('unhandledRejection', (error) => {
  logger.error('未處理的Promise拒絕:', error);
  process.exit(1);
});