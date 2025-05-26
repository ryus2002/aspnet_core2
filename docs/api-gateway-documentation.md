# API 網關文檔

## 概述

API網關是客戶端與微服務系統交互的統一入口點。它負責路由請求、認證授權、請求轉發和錯誤處理等功能。本系統使用Ocelot作為API網關實現。

## 基本信息

- **基礎URL**: `http://localhost:5000`
- **API版本**: v1
- **支持格式**: JSON

## 路由規則

API網關將請求路由到以下微服務：

### 認證服務 (Auth Service)

| 路由模式 | HTTP方法 | 目標服務 | 需要認證 | 說明 |
|---------|---------|---------|---------|------|
| `/api/auth/{everything}` | GET, POST, PUT, DELETE | auth-service | 否 | 認證相關端點，包括登入、註冊等 |
| `/api/users/{everything}` | GET, POST, PUT, DELETE | auth-service | 是 | 用戶管理相關端點 |
| `/api/roles/{everything}` | GET, POST, PUT, DELETE | auth-service | 是 | 角色管理相關端點 |
| `/api/permissions/{everything}` | GET, POST, PUT, DELETE | auth-service | 是 | 權限管理相關端點 |

### 商品服務 (Product Service)

| 路由模式 | HTTP方法 | 目標服務 | 需要認證 | 說明 |
|---------|---------|---------|---------|------|
| `/api/products/{everything}` | GET, POST, PUT, DELETE | product-service | 部分需要 | 商品管理相關端點 |
| `/api/categories/{everything}` | GET, POST, PUT, DELETE | product-service | 部分需要 | 商品分類相關端點 |

### 訂單服務 (Order Service)

| 路由模式 | HTTP方法 | 目標服務 | 需要認證 | 說明 |
|---------|---------|---------|---------|------|
| `/api/orders/{everything}` | GET, POST, PUT, DELETE | order-service | 是 | 訂單管理相關端點 |
| `/api/carts/{everything}` | GET, POST, PUT, DELETE | order-service | 否 | 購物車管理相關端點 |

### 支付服務 (Payment Service)

| 路由模式 | HTTP方法 | 目標服務 | 需要認證 | 說明 |
|---------|---------|---------|---------|------|
| `/api/payments/{everything}` | GET, POST, PUT, DELETE | payment-service | 是 | 支付管理相關端點 |
| `/api/refunds/{everything}` | GET, POST, PUT, DELETE | payment-service | 是 | 退款管理相關端點 |
| `/api/mock-payments/{everything}` | GET, POST, PUT, DELETE | payment-service | 否 | 模擬支付相關端點(測試用) |

## 認證與授權

### 認證機制

系統使用JWT(JSON Web Token)進行認證。客戶端需要在請求頭中包含Bearer令牌：

```
Authorization: Bearer {token}
```

獲取令牌的流程：
1. 向 `/api/auth/login` 發送POST請求，提供用戶名和密碼
2. 服務返回訪問令牌(access token)和刷新令牌(refresh token)
3. 使用訪問令牌進行API調用
4. 令牌過期後，使用刷新令牌獲取新的訪問令牌

### 授權機制

系統使用基於角色(RBAC)的授權機制：
- 每個用戶可以擁有多個角色
- 每個角色可以擁有多個權限
- API端點根據所需權限進行保護

## 錯誤處理

### 標準錯誤響應格式

所有API錯誤響應使用統一的格式：

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "人類可讀的錯誤訊息",
    "details": {
      "field1": "錯誤詳情1",
      "field2": "錯誤詳情2"
    }
  }
}
```

### 常見錯誤碼

| HTTP狀態碼 | 錯誤碼 | 說明 |
|-----------|-------|------|
| 400 | BAD_REQUEST | 請求格式錯誤或參數無效 |
| 401 | UNAUTHORIZED | 未提供認證信息或認證失敗 |
| 403 | FORBIDDEN | 無權訪問請求的資源 |
| 404 | NOT_FOUND | 請求的資源不存在 |
| 409 | CONFLICT | 資源衝突(如創建已存在的資源) |
| 422 | VALIDATION_ERROR | 請求數據驗證失敗 |
| 429 | TOO_MANY_REQUESTS | 請求頻率超過限制 |
| 500 | INTERNAL_ERROR | 服務器內部錯誤 |
| 503 | SERVICE_UNAVAILABLE | 服務暫時不可用 |

## 請求限流與熔斷

API網關實施了請求限流和熔斷機制，以保護系統免受過載：

### 請求限流

- 匿名用戶: 每分鐘最多100個請求
- 認證用戶: 每分鐘最多300個請求
- 管理員用戶: 每分鐘最多1000個請求

超過限制的請求將收到429 (Too Many Requests)錯誤響應。

### 熔斷機制

當下游服務出現故障時，API網關會啟動熔斷機制：
- 連續5次請求失敗後，熔斷器開啟
- 熔斷器開啟期間(30秒)，所有請求立即返回503錯誤
- 30秒後，熔斷器進入半開狀態，允許部分請求通過
- 如果這些請求成功，熔斷器關閉；否則重新開啟

## 跨域資源共享(CORS)

API網關支持跨域資源共享(CORS)，允許從不同域的前端應用訪問API：

- 允許的來源: 所有(`*`)
- 允許的方法: `GET`, `POST`, `PUT`, `DELETE`, `OPTIONS`
- 允許的頭部: `Content-Type`, `Authorization`, `X-Requested-With`
- 允許憑證: 否
- 預檢請求緩存時間: 1小時

## API版本控制

系統使用以下版本控制策略：

1. **URL路徑版本控制**：
   - 格式: `/api/v{version_number}/{resource}`
   - 示例: `/api/v1/products`

2. **Swagger文檔版本**：
   - 每個服務的Swagger文檔都包含版本信息
   - 可通過API網關訪問: `/swagger/{service_key}/swagger.json`

### 版本兼容性政策

- 次要版本更新(v1.1, v1.2等)保持向後兼容性
- 主要版本更新(v1, v2等)可能包含不兼容變更
- 舊版API在新版發布後至少維護6個月

## 監控與日誌

API網關提供以下監控端點：

- `/health` - 健康檢查端點，返回所有微服務的健康狀態
- `/metrics` - 提供API使用指標(僅限內部訪問)

## Swagger文檔

每個微服務的Swagger文檔可通過以下URL訪問：

- 認證服務: `/swagger/auth/swagger.json`
- 商品服務: `/swagger/products/swagger.json`
- 訂單服務: `/swagger/orders/swagger.json`
- 支付服務: `/swagger/payments/swagger.json`

綜合Swagger UI界面可通過 `/swagger` 訪問。