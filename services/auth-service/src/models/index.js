const fs = require('fs');
const path = require('path');
const Sequelize = require('sequelize');
const basename = path.basename(__filename);
const env = process.env.NODE_ENV || 'development';
const config = require('../config/database')[env];
const logger = require('../utils/logger');

const db = {};
// 創建Sequelize實例
const sequelize = new Sequelize(
  config.database,
  config.username,
  config.password,
  {
    host: config.host,
    dialect: config.dialect,
    port: config.port,
    logging: msg => logger.debug(msg),
    ...config.options
  }
);

// 動態導入所有模型
fs.readdirSync(__dirname)
  .filter(file => {
    return (
      file.indexOf('.') !== 0 &&
      file !== basename &&
      file.slice(-3) === '.js'
    );
  })
  .forEach(file => {
    const model = require(path.join(__dirname, file))(sequelize);
    db[model.name] = model;
  });

// 設置模型之間的關聯
Object.keys(db).forEach(modelName => {
  if (db[modelName].associate) {
    db[modelName].associate(db);
  }
});
// 設置模型關聯
db.User.belongsToMany(db.Role, { through: 'UserRoles' });
db.Role.belongsToMany(db.User, { through: 'UserRoles' });

db.Role.belongsToMany(db.Permission, { through: 'RolePermissions' });
db.Permission.belongsToMany(db.Role, { through: 'RolePermissions' });

db.User.hasMany(db.RefreshToken);
db.RefreshToken.belongsTo(db.User);

db.User.hasMany(db.LoginHistory);
db.LoginHistory.belongsTo(db.User);

db.User.hasOne(db.UserProfile);
db.UserProfile.belongsTo(db.User);

// 導出數據庫對象
db.sequelize = sequelize;
db.Sequelize = Sequelize;

module.exports = db;
