# 用戶服務資料模型設計

## 概述

用戶服務負責管理用戶認證、授權和個人資料。根據系統架構設計和技術選型，用戶服務使用 MySQL 作為資料庫，利用其穩定性和關係模型來管理用戶相關數據。

## 資料庫選擇

**MySQL**

選擇理由：
- 穩定可靠，廣泛應用於身份認證系統
- 強大的事務支持，確保用戶數據的完整性
- 完善的關係模型，適合處理用戶與角色的關係
- 良好的安全特性，適合存儲敏感的用戶資訊

## 表結構設計

### 1. 用戶表 (users)

存儲用戶的基本信息和認證資料。

```sql
CREATE TABLE users (
    id VARCHAR(36) PRIMARY KEY,            -- UUID格式的用戶ID
    username VARCHAR(50) NOT NULL,         -- 用戶名
    email VARCHAR(100) NOT NULL,           -- 電子郵件
    phone VARCHAR(20) NULL,                -- 電話號碼
    password_hash VARCHAR(255) NOT NULL,   -- 密碼雜湊
    salt VARCHAR(100) NOT NULL,            -- 密碼鹽
    first_name VARCHAR(50) NULL,           -- 名
    last_name VARCHAR(50) NULL,            -- 姓
    status ENUM('active', 'inactive', 'suspended', 'pending') NOT NULL DEFAULT 'pending', -- 用戶狀態
    email_verified BOOLEAN NOT NULL DEFAULT false, -- 郵箱是否已驗證
    phone_verified BOOLEAN NOT NULL DEFAULT false, -- 電話是否已驗證
    verification_token VARCHAR(100) NULL,  -- 驗證令牌
    verification_token_expires DATETIME NULL, -- 驗證令牌過期時間
    reset_password_token VARCHAR(100) NULL, -- 重置密碼令牌
    reset_password_expires DATETIME NULL,  -- 重置密碼令牌過期時間
    last_login_at DATETIME NULL,           -- 最後登入時間
    last_login_ip VARCHAR(50) NULL,        -- 最後登入IP
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 創建時間
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 更新時間
    UNIQUE KEY uk_users_username (username),
    UNIQUE KEY uk_users_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 2. 角色表 (roles)

定義系統中的角色。

```sql
CREATE TABLE roles (
    id VARCHAR(36) PRIMARY KEY,            -- 角色ID
    name VARCHAR(50) NOT NULL,             -- 角色名稱
    description VARCHAR(255) NULL,         -- 角色描述
    is_system BOOLEAN NOT NULL DEFAULT false, -- 是否為系統角色
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 創建時間
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 更新時間
    UNIQUE KEY uk_roles_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3. 用戶角色關聯表 (user_roles)

存儲用戶與角色的多對多關係。

```sql
CREATE TABLE user_roles (
    user_id VARCHAR(36) NOT NULL,          -- 用戶ID
    role_id VARCHAR(36) NOT NULL,          -- 角色ID
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 創建時間
    PRIMARY KEY (user_id, role_id),
    FOREIGN KEY fk_user_roles_user_id (user_id) REFERENCES users (id) ON DELETE CASCADE,
    FOREIGN KEY fk_user_roles_role_id (role_id) REFERENCES roles (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 4. 權限表 (permissions)

定義系統中的權限。

```sql
CREATE TABLE permissions (
    id VARCHAR(36) PRIMARY KEY,            -- 權限ID
    name VARCHAR(100) NOT NULL,            -- 權限名稱
    description VARCHAR(255) NULL,         -- 權限描述
    resource VARCHAR(100) NOT NULL,        -- 資源名稱
    action VARCHAR(50) NOT NULL,           -- 操作類型 (create, read, update, delete, etc.)
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 創建時間
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 更新時間
    UNIQUE KEY uk_permissions_name (name),
    UNIQUE KEY uk_permissions_resource_action (resource, action)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 5. 角色權限關聯表 (role_permissions)

存儲角色與權限的多對多關係。

```sql
CREATE TABLE role_permissions (
    role_id VARCHAR(36) NOT NULL,          -- 角色ID
    permission_id VARCHAR(36) NOT NULL,    -- 權限ID
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 創建時間
    PRIMARY KEY (role_id, permission_id),
    FOREIGN KEY fk_role_permissions_role_id (role_id) REFERENCES roles (id) ON DELETE CASCADE,
    FOREIGN KEY fk_role_permissions_permission_id (permission_id) REFERENCES permissions (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 6. 用戶個人資料表 (user_profiles)

存儲用戶的詳細個人資料。

```sql
CREATE TABLE user_profiles (
    user_id VARCHAR(36) PRIMARY KEY,       -- 用戶ID
    avatar_url VARCHAR(255) NULL,          -- 頭像URL
    gender ENUM('male', 'female', 'other', 'prefer_not_to_say') NULL, -- 性別
    birth_date DATE NULL,                  -- 出生日期
    bio TEXT NULL,                         -- 個人簡介
    language VARCHAR(10) NULL DEFAULT 'zh-TW', -- 偏好語言
    country VARCHAR(50) NULL,              -- 國家
    city VARCHAR(50) NULL,                 -- 城市
    address TEXT NULL,                     -- 地址
    postal_code VARCHAR(20) NULL,          -- 郵政編碼
    preferences JSON NULL,                 -- 用戶偏好設置
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 創建時間
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- 更新時間
    FOREIGN KEY fk_user_profiles_user_id (user_id) REFERENCES users (id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 7. 刷新令牌表 (refresh_tokens)

存儲用戶的JWT刷新令牌。

```sql
CREATE TABLE refresh_tokens (
    id VARCHAR(36) PRIMARY KEY,            -- 令牌ID
    user_id VARCHAR(36) NOT NULL,          -- 用戶ID
    token VARCHAR(255) NOT NULL,           -- 刷新令牌
    expires_at DATETIME NOT NULL,          -- 過期時間
    created_by_ip VARCHAR(50) NULL,        -- 創建IP
    revoked BOOLEAN NOT NULL DEFAULT false, -- 是否已撤銷
    revoked_at DATETIME NULL,              -- 撤銷時間
    revoked_by_ip VARCHAR(50) NULL,        -- 撤銷IP
    replaced_by_token VARCHAR(255) NULL,   -- 替換令牌
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 創建時間
    FOREIGN KEY fk_refresh_tokens_user_id (user_id) REFERENCES users (id) ON DELETE CASCADE,
    INDEX idx_refresh_tokens_token (token),
    INDEX idx_refresh_tokens_expires_at (expires_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 8. 登入歷史表 (login_history)

記錄用戶的登入歷史。

```sql
CREATE TABLE login_history (
    id VARCHAR(36) PRIMARY KEY,            -- 歷史記錄ID
    user_id VARCHAR(36) NOT NULL,          -- 用戶ID
    login_at DATETIME NOT NULL,            -- 登入時間
    ip_address VARCHAR(50) NOT NULL,       -- IP地址
    user_agent TEXT NULL,                  -- 用戶代理
    success BOOLEAN NOT NULL,              -- 是否成功
    failure_reason VARCHAR(255) NULL,      -- 失敗原因
    FOREIGN KEY fk_login_history_user_id (user_id) REFERENCES users (id) ON DELETE CASCADE,
    INDEX idx_login_history_user_id (user_id),
    INDEX idx_login_history_login_at (login_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

## 索引設計

為提高查詢效能，建立以下索引：

1. 用戶表 (users):
   - `username`: 唯一索引，支持用戶名查詢
   - `email`: 唯一索引，支持郵箱查詢
   - `status`: 支持按狀態查詢用戶
   - `verification_token`: 支持驗證令牌查詢
   - `reset_password_token`: 支持重置密碼令牌查詢

2. 角色表 (roles):
   - `name`: 唯一索引，支持角色名稱查詢

3. 權限表 (permissions):
   - `name`: 唯一索引，支持權限名稱查詢
   - `resource, action`: 唯一索引，支持資源和操作查詢

4. 刷新令牌表 (refresh_tokens):
   - `token`: 支持令牌查詢
   - `expires_at`: 支持過期時間查詢
   - `user_id`: 支持按用戶查詢令牌

5. 登入歷史表 (login_history):
   - `user_id`: 支持按用戶查詢登入歷史
   - `login_at`: 支持按時間查詢登入歷史

## 數據完整性約束

1. 外鍵約束:
   - `user_roles.user_id` -> `users.id`
   - `user_roles.role_id` -> `roles.id`
   - `role_permissions.role_id` -> `roles.id`
   - `role_permissions.permission_id` -> `permissions.id`
   - `user_profiles.user_id` -> `users.id`
   - `refresh_tokens.user_id` -> `users.id`
   - `login_history.user_id` -> `users.id`

2. 唯一約束:
   - `users.username`
   - `users.email`
   - `roles.name`
   - `permissions.name`
   - `permissions.resource, permissions.action`

## 初始數據

系統初始化時需要創建的基本數據：

1. 基本角色:
   - `admin`: 系統管理員
   - `user`: 普通用戶
   - `guest`: 訪客

2. 基本權限:
   - 商品相關權限 (product:read, product:create, etc.)
   - 訂單相關權限 (order:read, order:create, etc.)
   - 用戶相關權限 (user:read, user:create, etc.)

## 數據遷移策略

1. 初始化遷移:
   - 創建基本表結構
   - 創建索引和約束
   - 插入初始角色和權限數據

2. 版本管理:
   - 使用語義化版本控制追蹤模式變更
   - 每次模式變更創建新的遷移腳本

## 安全考慮

1. 密碼存儲:
   - 使用 bcrypt 或 Argon2 等現代哈希算法
   - 為每個用戶生成唯一的鹽值
   - 設置適當的哈希強度

2. 敏感數據保護:
   - 考慮對敏感個人資訊進行加密
   - 實施適當的訪問控制

3. 令牌管理:
   - 設置合理的令牌過期時間
   - 實現令牌輪換機制
   - 支持令牌撤銷

## 本地開發配置

為本地開發環境配置 MySQL:

```yaml
# docker-compose.yml 片段
mysql:
  image: mysql:8.0
  ports:
    - "3306:3306"
  environment:
    MYSQL_ROOT_PASSWORD: root
    MYSQL_DATABASE: user_service
    MYSQL_USER: user_service
    MYSQL_PASSWORD: password
  volumes:
    - mysql_data:/var/lib/mysql
    - ./init-scripts:/docker-entrypoint-initdb.d
```

初始化腳本 (init-scripts/01-create-schema.sql):
```sql
-- 創建用戶服務數據庫架構
USE user_service;

-- 建立表結構
CREATE TABLE users (
    -- 表結構定義...
);

-- 其他表結構...

-- 創建索引
CREATE UNIQUE INDEX uk_users_username ON users(username);
-- 其他索引...

-- 插入初始數據
INSERT INTO roles (id, name, description, is_system) VALUES
(UUID(), 'admin', '系統管理員', true),
(UUID(), 'user', '普通用戶', true),
(UUID(), 'guest', '訪客', true);

-- 其他初始數據...
```