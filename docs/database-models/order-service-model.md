# 訂單服務資料模型設計

## 概述

訂單服務負責管理用戶訂單、購物車和訂單狀態追蹤。根據系統架構設計和技術選型，訂單服務使用 PostgreSQL 作為資料庫，利用其強大的事務支持和關係模型來確保訂單數據的一致性和完整性。

## 資料庫選擇

**PostgreSQL**

選擇理由：
- 強大的事務支持，確保訂單處理的原子性和一致性
- 完善的關係模型，適合處理訂單與訂單項目的關係
- 支持複雜查詢，便於訂單分析和報表生成
- 穩定可靠，廣泛用於需要數據完整性的應用場景

## 表結構設計

### 1. 購物車表 (carts)

存儲用戶的購物車信息。

```sql
CREATE TABLE carts (
    id SERIAL PRIMARY KEY,
    user_id VARCHAR(36) NULL,           -- 用戶ID (已登入用戶)
    session_id VARCHAR(100) NOT NULL,   -- 會話ID (未登入用戶)
    status VARCHAR(20) NOT NULL DEFAULT 'active', -- 狀態: active, abandoned, converted
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NULL,          -- 購物車過期時間
    metadata JSONB NULL,                -- 額外元數據
    CONSTRAINT uk_carts_session_id UNIQUE (session_id)
);
```

### 2. 購物車項目表 (cart_items)

存儲購物車中的商品項目。

```sql
CREATE TABLE cart_items (
    id SERIAL PRIMARY KEY,
    cart_id INTEGER NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
    product_id VARCHAR(36) NOT NULL,    -- 商品ID
    variant_id VARCHAR(36) NULL,        -- 商品變體ID
    quantity INTEGER NOT NULL DEFAULT 1,
    unit_price DECIMAL(10, 2) NOT NULL, -- 單價
    name VARCHAR(255) NOT NULL,         -- 商品名稱 (快照)
    attributes JSONB NULL,              -- 商品屬性 (顏色、尺寸等)
    added_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT ck_cart_items_quantity CHECK (quantity > 0)
);
```

### 3. 訂單表 (orders)

存儲訂單的主要信息。

```sql
CREATE TABLE orders (
    id VARCHAR(36) PRIMARY KEY,         -- UUID格式的訂單ID
    order_number VARCHAR(20) NOT NULL,  -- 人類可讀的訂單編號
    user_id VARCHAR(36) NOT NULL,       -- 用戶ID
    status VARCHAR(20) NOT NULL,        -- 訂單狀態: pending, paid, processing, shipped, delivered, cancelled, refunded
    total_amount DECIMAL(10, 2) NOT NULL, -- 訂單總金額
    items_count INTEGER NOT NULL,       -- 訂單項目數量
    shipping_address_id INTEGER NULL,   -- 配送地址ID
    billing_address_id INTEGER NULL,    -- 帳單地址ID
    payment_id VARCHAR(36) NULL,        -- 支付ID
    shipping_method VARCHAR(50) NULL,   -- 配送方式
    shipping_fee DECIMAL(10, 2) NOT NULL DEFAULT 0, -- 配送費用
    tax_amount DECIMAL(10, 2) NOT NULL DEFAULT 0,   -- 稅額
    discount_amount DECIMAL(10, 2) NOT NULL DEFAULT 0, -- 折扣金額
    notes TEXT NULL,                    -- 訂單備註
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,        -- 訂單完成時間
    cancelled_at TIMESTAMP NULL,        -- 訂單取消時間
    cancellation_reason VARCHAR(255) NULL, -- 取消原因
    metadata JSONB NULL,                -- 額外元數據
    CONSTRAINT uk_orders_order_number UNIQUE (order_number)
);
```

### 4. 訂單項目表 (order_items)

存儲訂單中的商品項目。

```sql
CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id VARCHAR(36) NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id VARCHAR(36) NOT NULL,    -- 商品ID
    variant_id VARCHAR(36) NULL,        -- 商品變體ID
    name VARCHAR(255) NOT NULL,         -- 商品名稱 (快照)
    quantity INTEGER NOT NULL,          -- 數量
    unit_price DECIMAL(10, 2) NOT NULL, -- 單價 (快照)
    total_price DECIMAL(10, 2) NOT NULL, -- 總價
    attributes JSONB NULL,              -- 商品屬性 (顏色、尺寸等) (快照)
    sku VARCHAR(50) NULL,               -- 庫存單位 (快照)
    image_url VARCHAR(255) NULL,        -- 商品圖片URL (快照)
    CONSTRAINT ck_order_items_quantity CHECK (quantity > 0)
);
```

### 5. 地址表 (addresses)

存儲用戶的配送和帳單地址。

```sql
CREATE TABLE addresses (
    id SERIAL PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,       -- 用戶ID
    name VARCHAR(100) NOT NULL,         -- 收件人/帳單人姓名
    phone VARCHAR(20) NOT NULL,         -- 電話
    address_line1 VARCHAR(255) NOT NULL, -- 地址行1
    address_line2 VARCHAR(255) NULL,    -- 地址行2
    city VARCHAR(100) NOT NULL,         -- 城市
    state VARCHAR(100) NULL,            -- 州/省
    postal_code VARCHAR(20) NOT NULL,   -- 郵政編碼
    country VARCHAR(100) NOT NULL,      -- 國家
    is_default BOOLEAN NOT NULL DEFAULT false, -- 是否為默認地址
    address_type VARCHAR(20) NOT NULL,  -- 地址類型: shipping, billing, both
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

### 6. 訂單狀態歷史表 (order_status_history)

記錄訂單狀態變更歷史。

```sql
CREATE TABLE order_status_history (
    id SERIAL PRIMARY KEY,
    order_id VARCHAR(36) NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL,        -- 訂單狀態
    comment TEXT NULL,                  -- 狀態變更說明
    changed_by VARCHAR(36) NULL,        -- 操作人ID
    changed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

### 7. 訂單事件表 (order_events)

記錄與訂單相關的事件，用於與其他服務的集成。

```sql
CREATE TABLE order_events (
    id SERIAL PRIMARY KEY,
    order_id VARCHAR(36) NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    event_type VARCHAR(50) NOT NULL,    -- 事件類型: created, updated, cancelled, etc.
    payload JSONB NOT NULL,             -- 事件數據
    processed BOOLEAN NOT NULL DEFAULT false, -- 是否已處理
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP NULL         -- 處理時間
);
```

## 索引設計

為提高查詢效能，建立以下索引：

1. 購物車表 (carts):
   - `user_id`: 支持按用戶查詢購物車
   - `status, expires_at`: 支持查詢過期購物車

2. 購物車項目表 (cart_items):
   - `cart_id`: 支持查詢特定購物車的項目
   - `product_id`: 支持查詢包含特定商品的購物車

3. 訂單表 (orders):
   - `user_id`: 支持按用戶查詢訂單
   - `status`: 支持按狀態查詢訂單
   - `created_at`: 支持按創建時間查詢訂單
   - `order_number`: 支持按訂單編號查詢

4. 訂單項目表 (order_items):
   - `order_id`: 支持查詢特定訂單的項目
   - `product_id`: 支持查詢包含特定商品的訂單

5. 地址表 (addresses):
   - `user_id, is_default`: 支持查詢用戶的默認地址
   - `user_id, address_type`: 支持查詢用戶的特定類型地址

6. 訂單狀態歷史表 (order_status_history):
   - `order_id, changed_at`: 支持查詢訂單狀態變更歷史

7. 訂單事件表 (order_events):
   - `processed, created_at`: 支持查詢未處理的事件
   - `event_type`: 支持按事件類型查詢

## 數據完整性約束

1. 外鍵約束:
   - `cart_items.cart_id` -> `carts.id`
   - `order_items.order_id` -> `orders.id`
   - `order_status_history.order_id` -> `orders.id`
   - `order_events.order_id` -> `orders.id`

2. 檢查約束:
   - `cart_items.quantity > 0`
   - `order_items.quantity > 0`

3. 唯一約束:
   - `carts.session_id`
   - `orders.order_number`

## 事務管理

訂單服務中的關鍵操作需要使用事務來確保數據一致性:

1. 創建訂單: 將購物車轉換為訂單時，需要在一個事務中:
   - 創建訂單記錄
   - 創建訂單項目記錄
   - 更新購物車狀態
   - 創建訂單事件

2. 更新訂單狀態: 更新訂單狀態時，需要在一個事務中:
   - 更新訂單狀態
   - 添加狀態歷史記錄
   - 創建訂單事件

## 數據遷移策略

1. 初始化遷移:
   - 創建基本表結構
   - 創建索引和約束

2. 版本管理:
   - 使用語義化版本控制追蹤模式變更
   - 使用 Sequelize 或其他 ORM 工具管理遷移

## 本地開發配置

為本地開發環境配置 PostgreSQL:

```yaml
# docker-compose.yml 片段
postgres:
  image: postgres:14
  ports:
    - "5432:5432"
  environment:
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres
    POSTGRES_DB: order_service
  volumes:
    - postgres_data:/var/lib/postgresql/data
    - ./init-scripts:/docker-entrypoint-initdb.d
```

初始化腳本 (init-scripts/01-create-schema.sql):
```sql
-- 創建訂單服務數據庫架構
CREATE SCHEMA IF NOT EXISTS order_service;
SET search_path TO order_service;

-- 建立表結構
CREATE TABLE carts (
    -- 表結構定義...
);

-- 其他表結構...

-- 創建索引
CREATE INDEX idx_carts_user_id ON carts(user_id);
-- 其他索引...
```