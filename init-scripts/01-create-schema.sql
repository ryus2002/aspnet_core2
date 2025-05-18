-- 創建支付服務數據庫架構
\c payment_service;

-- 創建表結構
CREATE TABLE payment_methods (
    id VARCHAR(36) PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    processing_fee DECIMAL(5, 2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_payment_methods_code UNIQUE (code)
);

CREATE TABLE payment_providers (
    id VARCHAR(36) PRIMARY KEY,
    code VARCHAR(50) NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    config JSONB NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_payment_providers_code UNIQUE (code)
);

CREATE TABLE payment_transactions (
    id VARCHAR(36) PRIMARY KEY,
    order_id VARCHAR(36) NOT NULL,
    user_id VARCHAR(36) NOT NULL,
    payment_method_id VARCHAR(36) NOT NULL,
    payment_provider_id VARCHAR(36) NULL,
    amount DECIMAL(10, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL DEFAULT 'TWD',
    status VARCHAR(20) NOT NULL,
    transaction_reference VARCHAR(100) NULL,
    error_message TEXT NULL,
    payment_intent_id VARCHAR(100) NULL,
    payment_provider_response JSONB NULL,
    metadata JSONB NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,
    expires_at TIMESTAMP NULL,
    CONSTRAINT fk_payment_transactions_payment_method FOREIGN KEY (payment_method_id) REFERENCES payment_methods(id),
    CONSTRAINT fk_payment_transactions_payment_provider FOREIGN KEY (payment_provider_id) REFERENCES payment_providers(id),
    CONSTRAINT chk_payment_transactions_amount_positive CHECK (amount > 0)
);

CREATE TABLE refunds (
    id VARCHAR(36) PRIMARY KEY,
    payment_transaction_id VARCHAR(36) NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    status VARCHAR(20) NOT NULL,
    reason VARCHAR(255) NULL,
    notes TEXT NULL,
    refund_reference VARCHAR(100) NULL,
    refunded_by VARCHAR(36) NULL,
    provider_response JSONB NULL,
    metadata JSONB NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP NULL,
    CONSTRAINT fk_refunds_payment_transaction FOREIGN KEY (payment_transaction_id) REFERENCES payment_transactions(id),
    CONSTRAINT chk_refunds_amount_positive CHECK (amount > 0)
);

CREATE TABLE payment_status_history (
    id VARCHAR(36) PRIMARY KEY,
    payment_transaction_id VARCHAR(36) NOT NULL,
    previous_status VARCHAR(20) NULL,
    new_status VARCHAR(20) NOT NULL,
    changed_by VARCHAR(36) NULL,
    reason VARCHAR(255) NULL,
    notes TEXT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_payment_status_history_payment_transaction FOREIGN KEY (payment_transaction_id) REFERENCES payment_transactions(id)
);

CREATE TABLE payment_method_providers (
    payment_method_id VARCHAR(36) NOT NULL,
    payment_provider_id VARCHAR(36) NOT NULL,
    is_default BOOLEAN NOT NULL DEFAULT false,
    config JSONB NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (payment_method_id, payment_provider_id),
    CONSTRAINT fk_payment_method_providers_payment_method FOREIGN KEY (payment_method_id) REFERENCES payment_methods(id),
    CONSTRAINT fk_payment_method_providers_payment_provider FOREIGN KEY (payment_provider_id) REFERENCES payment_providers(id)
);

CREATE TABLE payment_notifications (
    id VARCHAR(36) PRIMARY KEY,
    payment_transaction_id VARCHAR(36) NULL,
    payment_provider_id VARCHAR(36) NOT NULL,
    notification_type VARCHAR(50) NOT NULL,
    raw_payload TEXT NOT NULL,
    processed BOOLEAN NOT NULL DEFAULT false,
    processing_result VARCHAR(50) NULL,
    error_message TEXT NULL,
    received_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMP NULL,
    CONSTRAINT fk_payment_notifications_payment_transaction FOREIGN KEY (payment_transaction_id) REFERENCES payment_transactions(id),
    CONSTRAINT fk_payment_notifications_payment_provider FOREIGN KEY (payment_provider_id) REFERENCES payment_providers(id)
);

CREATE TABLE payment_settings (
    id VARCHAR(36) PRIMARY KEY,
    setting_key VARCHAR(100) NOT NULL,
    setting_value TEXT NULL,
    description TEXT NULL,
    is_encrypted BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uk_payment_settings_key UNIQUE (setting_key)
);

CREATE TABLE user_payment_methods (
    id VARCHAR(36) PRIMARY KEY,
    user_id VARCHAR(36) NOT NULL,
    payment_method_id VARCHAR(36) NOT NULL,
    token VARCHAR(255) NULL,
    is_default BOOLEAN NOT NULL DEFAULT false,
    nickname VARCHAR(100) NULL,
    last_digits VARCHAR(4) NULL,
    expiry_month SMALLINT NULL,
    expiry_year SMALLINT NULL,
    card_type VARCHAR(50) NULL,
    billing_address_id VARCHAR(36) NULL,
    is_verified BOOLEAN NOT NULL DEFAULT false,
    metadata JSONB NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_payment_methods_payment_method FOREIGN KEY (payment_method_id) REFERENCES payment_methods(id)
);

-- 創建索引
CREATE INDEX idx_payment_transactions_order_id ON payment_transactions(order_id);
CREATE INDEX idx_payment_transactions_user_id ON payment_transactions(user_id);
CREATE INDEX idx_payment_transactions_status ON payment_transactions(status);
CREATE INDEX idx_payment_transactions_created_at ON payment_transactions(created_at);

CREATE INDEX idx_refunds_payment_transaction_id ON refunds(payment_transaction_id);
CREATE INDEX idx_refunds_status ON refunds(status);

CREATE INDEX idx_payment_status_history_transaction_id ON payment_status_history(payment_transaction_id);

CREATE INDEX idx_payment_notifications_transaction_id ON payment_notifications(payment_transaction_id);
CREATE INDEX idx_payment_notifications_provider_id ON payment_notifications(payment_provider_id);
CREATE INDEX idx_payment_notifications_processed ON payment_notifications(processed);

CREATE INDEX idx_user_payment_methods_user_id ON user_payment_methods(user_id);
CREATE INDEX idx_user_payment_methods_payment_method_id ON user_payment_methods(payment_method_id);

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