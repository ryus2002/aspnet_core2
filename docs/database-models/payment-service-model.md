# 支付服務資料模型設計 (第一部分：概述與資料庫選擇)

## 概述

支付服務是電商平台的核心組件，負責處理所有與金錢交易相關的操作。在微服務架構中，支付服務需要獨立運行，並與訂單服務、用戶服務等進行協作。本文檔詳細描述支付服務的資料模型設計。

## 服務職責

支付服務主要負責：

1. 處理各種支付方式的交易
2. 記錄所有支付和退款操作
3. 維護支付狀態和歷史
4. 提供支付相關查詢功能
5. 處理支付通知和回調
6. 支持模擬支付流程（本地開發環境）

## 資料庫選擇

### 主數據庫：PostgreSQL

選擇理由：
- 強大的事務支持，確保支付數據的完整性
- 優秀的並發控制，適合處理高頻的支付操作
- 豐富的數據類型支持，包括JSON和貨幣類型
- 強大的查詢能力，支持複雜的支付數據分析
- 開源且社區活躍，適合本地開發環境

### 緩存：Redis

用途：
- 暫存支付處理狀態
- 實現分佈式鎖，防止重複支付
- 存儲支付處理的臨時數據
- 實現簡單的速率限制，防止惡意請求

## 技術考量

1. **數據一致性**：支付操作需要高度的數據一致性，因此使用強事務數據庫
2. **審計追蹤**：所有支付操作都需要詳細記錄，以便後續審計和問題排查
3. **安全性**：敏感支付信息需要加密存儲
4. **可擴展性**：模型設計需要考慮未來支持更多支付方式的可能性
5. **本地開發友好**：設計應當簡化，適合在本地環境運行和測試

# 支付服務資料模型設計 (第二部分：核心表結構)

## 核心表結構設計

### 1. 支付方式表 (payment_methods)

存儲系統支持的各種支付方式。

```sql
CREATE TABLE payment_methods (
    id VARCHAR(36) PRIMARY KEY,                -- 支付方式ID
    code VARCHAR(50) NOT NULL,                 -- 支付方式代碼 (例如: credit_card, paypal, apple_pay)
    name VARCHAR(100) NOT NULL,                -- 支付方式名稱
    description TEXT NULL,                     -- 描述
    icon_url VARCHAR(255) NULL,                -- 圖標URL
    is_active BOOLEAN NOT NULL DEFAULT true,   -- 是否啟用
    processing_fee DECIMAL(5,2) NOT NULL DEFAULT 0.00, -- 處理費率(%)
    min_fee DECIMAL(10,2) NOT NULL DEFAULT 0.00,       -- 最低手續費
    max_fee DECIMAL(10,2) NULL,                -- 最高手續費
    config JSONB NULL,                         -- 支付方式配置 (JSON格式)
    display_order INT NOT NULL DEFAULT 0,      -- 顯示順序
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_payment_methods_code UNIQUE (code)
);
```

### 2. 支付交易表 (payment_transactions)

記錄所有支付交易。

```sql
CREATE TABLE payment_transactions (
    id VARCHAR(36) PRIMARY KEY,                -- 交易ID
    order_id VARCHAR(36) NOT NULL,             -- 關聯訂單ID
    user_id VARCHAR(36) NOT NULL,              -- 用戶ID
    payment_method_id VARCHAR(36) NOT NULL,    -- 支付方式ID
    amount DECIMAL(12,2) NOT NULL,             -- 交易金額
    currency VARCHAR(3) NOT NULL DEFAULT 'TWD', -- 貨幣代碼
    status VARCHAR(20) NOT NULL,               -- 交易狀態 (pending, completed, failed, cancelled, refunded)
    transaction_reference VARCHAR(100) NULL,   -- 外部交易參考號
    payment_provider_response JSONB NULL,      -- 支付提供商響應數據
    processing_fee DECIMAL(10,2) NOT NULL DEFAULT 0.00, -- 處理費用
    error_message TEXT NULL,                   -- 錯誤信息
    client_ip VARCHAR(50) NULL,                -- 客戶端IP
    payment_intent_id VARCHAR(100) NULL,       -- 支付意圖ID
    metadata JSONB NULL,                       -- 元數據
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,               -- 完成時間
    INDEX idx_payment_transactions_order_id (order_id),
    INDEX idx_payment_transactions_user_id (user_id),
    INDEX idx_payment_transactions_status (status),
    FOREIGN KEY (payment_method_id) REFERENCES payment_methods(id)
);
```

### 3. 退款表 (refunds)

記錄所有退款操作。

```sql
CREATE TABLE refunds (
    id VARCHAR(36) PRIMARY KEY,                -- 退款ID
    payment_transaction_id VARCHAR(36) NOT NULL, -- 關聯的支付交易ID
    amount DECIMAL(12,2) NOT NULL,             -- 退款金額
    currency VARCHAR(3) NOT NULL DEFAULT 'TWD', -- 貨幣代碼
    status VARCHAR(20) NOT NULL,               -- 退款狀態 (pending, completed, failed)
    reason VARCHAR(255) NULL,                  -- 退款原因
    notes TEXT NULL,                           -- 備註
    refund_reference VARCHAR(100) NULL,        -- 外部退款參考號
    refunded_by VARCHAR(36) NULL,              -- 執行退款的管理員ID
    provider_response JSONB NULL,              -- 支付提供商響應數據
    metadata JSONB NULL,                       -- 元數據
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,               -- 完成時間
    INDEX idx_refunds_payment_transaction_id (payment_transaction_id),
    INDEX idx_refunds_status (status),
    FOREIGN KEY (payment_transaction_id) REFERENCES payment_transactions(id)
);
```

### 4. 支付狀態變更歷史表 (payment_status_history)

記錄支付交易狀態的所有變更。

```sql
CREATE TABLE payment_status_history (
    id VARCHAR(36) PRIMARY KEY,                -- 歷史記錄ID
    payment_transaction_id VARCHAR(36) NOT NULL, -- 關聯的支付交易ID
    previous_status VARCHAR(20) NULL,          -- 先前狀態
    new_status VARCHAR(20) NOT NULL,           -- 新狀態
    changed_by VARCHAR(36) NULL,               -- 變更者ID (系統或管理員)
    reason VARCHAR(255) NULL,                  -- 變更原因
    notes TEXT NULL,                           -- 備註
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_payment_status_history_transaction_id (payment_transaction_id),
    FOREIGN KEY (payment_transaction_id) REFERENCES payment_transactions(id)
);
```

# 支付服務資料模型設計 (第三部分：輔助表結構與索引設計)

## 輔助表結構設計

### 5. 支付提供商表 (payment_providers)

存儲支付服務提供商的信息。

```sql
CREATE TABLE payment_providers (
    id VARCHAR(36) PRIMARY KEY,                -- 提供商ID
    code VARCHAR(50) NOT NULL,                 -- 提供商代碼
    name VARCHAR(100) NOT NULL,                -- 提供商名稱
    description TEXT NULL,                     -- 描述
    is_active BOOLEAN NOT NULL DEFAULT true,   -- 是否啟用
    config JSONB NULL,                         -- 提供商配置 (JSON格式)
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (code)
);
```

### 6. 支付方式與提供商關聯表 (payment_method_providers)

建立支付方式與提供商之間的關聯。

```sql
CREATE TABLE payment_method_providers (
    payment_method_id VARCHAR(36) NOT NULL,    -- 支付方式ID
    payment_provider_id VARCHAR(36) NOT NULL,  -- 支付提供商ID
    is_default BOOLEAN NOT NULL DEFAULT false, -- 是否為默認提供商
    config JSONB NULL,                         -- 特定配置
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (payment_method_id, payment_provider_id),
    FOREIGN KEY (payment_method_id) REFERENCES payment_methods(id),
    FOREIGN KEY (payment_provider_id) REFERENCES payment_providers(id)
);
```

### 7. 支付通知表 (payment_notifications)

記錄從支付提供商接收的通知。

```sql
CREATE TABLE payment_notifications (
    id VARCHAR(36) PRIMARY KEY,                -- 通知ID
    payment_transaction_id VARCHAR(36) NULL,   -- 關聯的支付交易ID
    payment_provider_id VARCHAR(36) NOT NULL,  -- 支付提供商ID
    notification_type VARCHAR(50) NOT NULL,    -- 通知類型
    raw_payload TEXT NOT NULL,                 -- 原始通知內容
    processed BOOLEAN NOT NULL DEFAULT false,  -- 是否已處理
    processing_result VARCHAR(50) NULL,        -- 處理結果
    error_message TEXT NULL,                   -- 錯誤信息
    received_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, -- 接收時間
    processed_at TIMESTAMP NULL,               -- 處理時間
    CONSTRAINT fk_payment_notifications_transaction_id FOREIGN KEY (payment_transaction_id) REFERENCES payment_transactions(id),
    CONSTRAINT fk_payment_notifications_provider_id FOREIGN KEY (payment_provider_id) REFERENCES payment_providers(id)
);

-- 創建索引
CREATE INDEX idx_payment_notifications_transaction_id ON payment_notifications(payment_transaction_id);
CREATE INDEX idx_payment_notifications_provider_id ON payment_notifications(payment_provider_id);
CREATE INDEX idx_payment_notifications_processed ON payment_notifications(processed);
```

### 8. 支付設置表 (payment_settings)

存儲支付服務的全局設置。

```sql
CREATE TABLE payment_settings (
    id VARCHAR(36) PRIMARY KEY,                -- 設置ID
    setting_key VARCHAR(100) NOT NULL,         -- 設置鍵
    setting_value TEXT NULL,                   -- 設置值
    description TEXT NULL,                     -- 描述
    is_encrypted BOOLEAN NOT NULL DEFAULT false, -- 是否加密
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (setting_key)
);
```

### 9. 用戶支付方式表 (user_payment_methods)

存儲用戶保存的支付方式信息。

```sql
CREATE TABLE user_payment_methods (
    id VARCHAR(36) PRIMARY KEY,                -- ID
    user_id VARCHAR(36) NOT NULL,              -- 用戶ID
    payment_method_id VARCHAR(36) NOT NULL,    -- 支付方式ID
    token VARCHAR(255) NULL,                   -- 支付令牌 (加密)
    is_default BOOLEAN NOT NULL DEFAULT false, -- 是否為默認
    nickname VARCHAR(100) NULL,                -- 用戶定義的名稱
    last_digits VARCHAR(4) NULL,               -- 卡號後四位 (如適用)
    expiry_month SMALLINT NULL,                -- 到期月份 (如適用)
    expiry_year SMALLINT NULL,                 -- 到期年份 (如適用)
    card_type VARCHAR(50) NULL,                -- 卡片類型 (如適用)
    billing_address_id VARCHAR(36) NULL,       -- 帳單地址ID
    is_verified BOOLEAN NOT NULL DEFAULT false, -- 是否已驗證
    metadata JSONB NULL,                       -- 元數據
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_payment_methods_payment_method_id FOREIGN KEY (payment_method_id) REFERENCES payment_methods(id)
);

-- 創建索引
CREATE INDEX idx_user_payment_methods_user_id ON user_payment_methods(user_id);
CREATE INDEX idx_user_payment_methods_payment_method_id ON user_payment_methods(payment_method_id);
```

## 索引設計

為了優化查詢性能，我們在關鍵欄位上建立了以下索引：

1. 支付交易表 (payment_transactions):
   - `order_id`: 支持按訂單ID查詢交易
   - `user_id`: 支持按用戶ID查詢交易
   - `status`: 支持按狀態查詢交易
   - `created_at`: 支持按創建時間查詢交易

2. 退款表 (refunds):
   - `payment_transaction_id`: 支持按交易ID查詢退款
   - `status`: 支持按狀態查詢退款
   - `created_at`: 支持按創建時間查詢退款

3. 支付狀態變更歷史表 (payment_status_history):
   - `payment_transaction_id`: 支持按交易ID查詢狀態變更歷史

4. 支付通知表 (payment_notifications):
   - `payment_transaction_id`: 支持按交易ID查詢通知
   - `payment_provider_id`: 支持按提供商ID查詢通知
   - `processed`: 支持查詢未處理的通知

5. 用戶支付方式表 (user_payment_methods):
   - `user_id`: 支持按用戶ID查詢保存的支付方式
   - `payment_method_id`: 支持按支付方式ID查詢

   # 支付服務資料模型設計 (第四部分：數據完整性、安全性與本地開發配置)

## 數據完整性約束

為確保支付數據的完整性和一致性，我們實施以下約束：

### 外鍵約束

1. `payment_transactions.payment_method_id` -> `payment_methods.id`
2. `refunds.payment_transaction_id` -> `payment_transactions.id`
3. `payment_status_history.payment_transaction_id` -> `payment_transactions.id`
4. `payment_method_providers.payment_method_id` -> `payment_methods.id`
5. `payment_method_providers.payment_provider_id` -> `payment_providers.id`
6. `payment_notifications.payment_transaction_id` -> `payment_transactions.id`
7. `payment_notifications.payment_provider_id` -> `payment_providers.id`
8. `user_payment_methods.payment_method_id` -> `payment_methods.id`

### 唯一性約束

1. `payment_methods.code`: 確保支付方式代碼唯一
2. `payment_providers.code`: 確保支付提供商代碼唯一
3. `payment_settings.setting_key`: 確保設置鍵唯一

### 檢查約束

```sql
-- 確保交易金額為正數
ALTER TABLE payment_transactions ADD CONSTRAINT chk_payment_transactions_amount_positive CHECK (amount > 0);

-- 確保退款金額為正數
ALTER TABLE refunds ADD CONSTRAINT chk_refunds_amount_positive CHECK (amount > 0);

-- 確保退款金額不超過原交易金額
ALTER TABLE refunds ADD CONSTRAINT chk_refunds_amount_not_exceeding 
    CHECK (amount <= (SELECT amount FROM payment_transactions WHERE id = payment_transaction_id));

-- 確保處理費率在有效範圍內
ALTER TABLE payment_methods ADD CONSTRAINT chk_payment_methods_fee_rate 
    CHECK (processing_fee >= 0 AND processing_fee <= 100);
```

## 安全性考量

### 敏感數據加密

對於支付服務中的敏感數據，我們採用以下加密策略：

1. **支付令牌**：使用強加密算法加密存儲
2. **配置信息**：包含API密鑰等敏感信息的配置使用加密存儲
3. **個人識別信息**：如卡號僅存儲最後四位，其餘部分不保存

### 加密實現

在本地開發環境中，我們使用以下方法實現加密：

```sql
-- 創建加密函數
CREATE OR REPLACE FUNCTION encrypt_sensitive_data(data TEXT, key TEXT) RETURNS TEXT AS $$
BEGIN
    -- 在實際環境中使用適當的加密算法
    -- 本地開發環境可使用簡化版本
    RETURN encode(encrypt(data::bytea, key::bytea, 'aes'), 'base64');
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- 創建解密函數
CREATE OR REPLACE FUNCTION decrypt_sensitive_data(encrypted_data TEXT, key TEXT) RETURNS TEXT AS $$
BEGIN
    -- 在實際環境中使用適當的解密算法
    -- 本地開發環境可使用簡化版本
    RETURN convert_from(decrypt(decode(encrypted_data, 'base64'), key::bytea, 'aes'), 'utf8');
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;
```

### 審計追蹤

為支持完整的審計追蹤，我們記錄所有關鍵操作：

1. 支付狀態變更記錄在 `payment_status_history` 表中
2. 所有支付通知記錄在 `payment_notifications` 表中
3. 所有數據表都包含 `created_at` 和 `updated_at` 時間戳

## 本地開發配置

### Docker 配置

在本地開發環境中使用 Docker 配置 PostgreSQL 和 Redis：

```yaml
# docker-compose.yml 片段
services:
  payment-db:
    image: postgres:14
    container_name: payment-service-db
    environment:
      POSTGRES_DB: payment_service
      POSTGRES_USER: payment_user
      POSTGRES_PASSWORD: payment_password
    ports:
      - "5432:5432"
    volumes:
      - payment-db-data:/var/lib/postgresql/data
      - ./init-scripts:/docker-entrypoint-initdb.d
    networks:
      - microservices-network

  payment-cache:
    image: redis:6
    container_name: payment-service-cache
    ports:
      - "6379:6379"
    volumes:
      - payment-cache-data:/data
    networks:
      - microservices-network

volumes:
  payment-db-data:
  payment-cache-data:

networks:
  microservices-network:
    external: true
```

### 初始化腳本

創建數據庫初始化腳本 (`init-scripts/01-create-schema.sql`):

```sql
-- 創建支付服務數據庫架構
\c payment_service;

-- 創建表結構
CREATE TABLE payment_methods (
    -- 表結構定義...
);

-- 其他表結構...

-- 插入初始數據
INSERT INTO payment_methods (id, code, name, description, is_active, processing_fee) VALUES
(gen_random_uuid(), 'credit_card', '信用卡', '使用信用卡支付', true, 2.5),
(gen_random_uuid(), 'debit_card', '金融卡', '使用金融卡支付', true, 1.5),
(gen_random_uuid(), 'bank_transfer', '銀行轉帳', '使用銀行轉帳支付', true, 1.0),
(gen_random_uuid(), 'cash_on_delivery', '貨到付款', '貨物送達時以現金支付', true, 0.0),
(gen_random_uuid(), 'line_pay', 'LINE Pay', '使用LINE Pay支付', true, 3.0),
(gen_random_uuid(), 'apple_pay', 'Apple Pay', '使用Apple Pay支付', true, 2.8);

INSERT INTO payment_providers (id, code, name, description, is_active) VALUES
(gen_random_uuid(), 'stripe', 'Stripe', '國際支付處理商', true),
(gen_random_uuid(), 'newebpay', '藍新金流', '台灣本地支付處理商', true),
(gen_random_uuid(), 'ecpay', '綠界科技', '台灣本地支付處理商', true),
(gen_random_uuid(), 'mock', '模擬支付', '用於本地開發的模擬支付提供商', true);

-- 插入測試設置
INSERT INTO payment_settings (id, setting_key, setting_value, description, is_encrypted) VALUES
(gen_random_uuid(), 'payment_timeout_minutes', '30', '支付超時時間（分鐘）', false),
(gen_random_uuid(), 'auto_cancel_expired_payments', 'true', '自動取消過期支付', false),
(gen_random_uuid(), 'enable_payment_notifications', 'true', '啟用支付通知', false),
(gen_random_uuid(), 'mock_payment_delay_seconds', '5', '模擬支付延遲時間（秒）', false);
```

## 模擬支付處理

為本地開發環境實現模擬支付處理：

1. **模擬支付提供商**：創建一個模擬支付提供商，用於本地測試
2. **可配置結果**：允許開發者配置支付成功或失敗
3. **模擬延遲**：模擬真實支付處理的延遲

```sql
-- 創建模擬支付處理函數
CREATE OR REPLACE FUNCTION process_mock_payment(
    p_transaction_id VARCHAR(36),
    p_should_succeed BOOLEAN DEFAULT true,
    p_delay_seconds INTEGER DEFAULT 5
) RETURNS BOOLEAN AS $$
BEGIN
    -- 模擬處理延遲
    PERFORM pg_sleep(p_delay_seconds);
    
    IF p_should_succeed THEN
        -- 更新為成功狀態
        UPDATE payment_transactions
        SET status = 'completed',
            completed_at = CURRENT_TIMESTAMP,
            transaction_reference = 'mock-' || p_transaction_id,
            payment_provider_response = '{"success": true, "message": "Payment processed successfully"}'::jsonb
        WHERE id = p_transaction_id;
        
        -- 記錄狀態變更
        INSERT INTO payment_status_history (
            id, payment_transaction_id, previous_status, new_status, changed_by, reason
        ) VALUES (
            gen_random_uuid(), p_transaction_id, 'pending', 'completed', 'system', 'Mock payment processed'
        );
        
        RETURN true;
    ELSE
        -- 更新為失敗狀態
        UPDATE payment_transactions
        SET status = 'failed',
            error_message = 'Mock payment failure',
            payment_provider_response = '{"success": false, "message": "Payment failed"}'::jsonb
        WHERE id = p_transaction_id;
        
        -- 記錄狀態變更
        INSERT INTO payment_status_history (
            id, payment_transaction_id, previous_status, new_status, changed_by, reason
        ) VALUES (
            gen_random_uuid(), p_transaction_id, 'pending', 'failed', 'system', 'Mock payment failed'
        );
        
        RETURN false;
    END IF;
END;
$$ LANGUAGE plpgsql;

