現在我將為訂單服務和支付服務創建API文檔，參考現有的產品服務API文檔格式。

首先，我將創建訂單服務的API文檔：

```md:docs/swagger/order-service-swagger.md
# 訂單服務 API 文檔

本文檔描述訂單服務(Order Service)的API端點。

## API 端點

### 創建訂單

從購物車創建新訂單。

```
POST /api/orders
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| cartId | integer | 是 | 購物車ID |
| shippingAddress | object | 是 | 收貨地址 |
| paymentMethod | string | 是 | 支付方式代碼 |
| notes | string | 否 | 訂單備註 |

#### 請求示例

```json
{
  "cartId": 123,
  "shippingAddress": {
    "recipientName": "張三",
    "phone": "0912345678",
    "addressLine1": "台北市信義區信義路五段7號",
    "addressLine2": "89樓",
    "city": "台北市",
    "state": "信義區",
    "postalCode": "110",
    "country": "Taiwan"
  },
  "paymentMethod": "credit_card",
  "notes": "請在工作日配送"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 201 | 訂單創建成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 404 | 購物車不存在 |
| 409 | 購物車已被使用或庫存不足 |

#### 成功響應示例

```json
{
  "id": "ord_60d21b4967d0d8992e610c85",
  "orderNumber": "ORD20250521001",
  "userId": "usr_60d21b4967d0d8992e610c80",
  "status": "pending_payment",
  "items": [
    {
      "id": 1,
      "productId": "prd_60d21b4967d0d8992e610c90",
      "productName": "智能手錶",
      "variantId": "var_60d21b4967d0d8992e610c91",
      "variantName": "黑色 42mm",
      "quantity": 1,
      "unitPrice": 2499.99,
      "totalPrice": 2499.99,
      "imageUrl": "https://example.com/images/smartwatch-black.jpg"
    }
  ],
  "subtotal": 2499.99,
  "shippingFee": 60.00,
  "tax": 125.00,
  "discount": 0.00,
  "total": 2684.99,
  "shippingAddress": {
    "recipientName": "張三",
    "phone": "0912345678",
    "addressLine1": "台北市信義區信義路五段7號",
    "addressLine2": "89樓",
    "city": "台北市",
    "state": "信義區",
    "postalCode": "110",
    "country": "Taiwan"
  },
  "paymentMethod": "credit_card",
  "paymentId": null,
  "notes": "請在工作日配送",
  "statusHistory": [
    {
      "status": "pending_payment",
      "timestamp": "2025-05-21T01:15:30Z",
      "comment": "訂單已創建，等待支付"
    }
  ],
  "createdAt": "2025-05-21T01:15:30Z",
  "updatedAt": "2025-05-21T01:15:30Z"
}
```

### 獲取訂單

通過ID獲取特定訂單的詳細信息。

```
GET /api/orders/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 訂單ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 訂單不存在 |

#### 成功響應示例

```json
{
  "id": "ord_60d21b4967d0d8992e610c85",
  "orderNumber": "ORD20250521001",
  "userId": "usr_60d21b4967d0d8992e610c80",
  "status": "processing",
  "items": [
    {
      "id": 1,
      "productId": "prd_60d21b4967d0d8992e610c90",
      "productName": "智能手錶",
      "variantId": "var_60d21b4967d0d8992e610c91",
      "variantName": "黑色 42mm",
      "quantity": 1,
      "unitPrice": 2499.99,
      "totalPrice": 2499.99,
      "imageUrl": "https://example.com/images/smartwatch-black.jpg"
    }
  ],
  "subtotal": 2499.99,
  "shippingFee": 60.00,
  "tax": 125.00,
  "discount": 0.00,
  "total": 2684.99,
  "shippingAddress": {
    "recipientName": "張三",
    "phone": "0912345678",
    "addressLine1": "台北市信義區信義路五段7號",
    "addressLine2": "89樓",
    "city": "台北市",
    "state": "信義區",
    "postalCode": "110",
    "country": "Taiwan"
  },
  "paymentMethod": "credit_card",
  "paymentId": "pay_60d21b4967d0d8992e610d10",
  "notes": "請在工作日配送",
  "statusHistory": [
    {
      "status": "pending_payment",
      "timestamp": "2025-05-21T01:15:30Z",
      "comment": "訂單已創建，等待支付"
    },
    {
      "status": "processing",
      "timestamp": "2025-05-21T01:20:15Z",
      "comment": "支付已完成，訂單處理中"
    }
  ],
  "createdAt": "2025-05-21T01:15:30Z",
  "updatedAt": "2025-05-21T01:20:15Z"
}
```

### 根據訂單編號獲取訂單

通過訂單編號獲取特定訂單的詳細信息。

```
GET /api/orders/number/{orderNumber}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| orderNumber | string | 是 | 訂單編號 |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 訂單不存在 |

#### 成功響應示例

與獲取訂單API相同

### 獲取用戶訂單列表

獲取當前用戶的訂單列表，支持分頁和狀態過濾。

```
GET /api/orders/user
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 查詢參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| status | string | 否 | 訂單狀態過濾 |
| page | integer | 否 | 頁碼，默認為1 |
| pageSize | integer | 否 | 每頁項目數，默認為10 |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |

#### 成功響應示例

```json
{
  "items": [
    {
      "id": "ord_60d21b4967d0d8992e610c85",
      "orderNumber": "ORD20250521001",
      "status": "processing",
      "total": 2684.99,
      "itemCount": 1,
      "createdAt": "2025-05-21T01:15:30Z",
      "updatedAt": "2025-05-21T01:20:15Z"
    },
    {
      "id": "ord_60d21b4967d0d8992e610c86",
      "orderNumber": "ORD20250520005",
      "status": "delivered",
      "total": 1250.00,
      "itemCount": 2,
      "createdAt": "2025-05-20T14:30:00Z",
      "updatedAt": "2025-05-21T09:45:20Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

### 更新訂單狀態

更新指定訂單的狀態。需要管理員權限。

```
PUT /api/orders/{id}/status
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 訂單ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| status | string | 是 | 新的訂單狀態 |
| comment | string | 否 | 狀態更新備註 |

#### 請求示例

```json
{
  "status": "shipped",
  "comment": "訂單已發貨，預計3-5天送達"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 訂單狀態更新成功 |
| 400 | 請求參數無效或狀態轉換無效 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 訂單不存在 |

#### 成功響應示例

```json
{
  "id": "ord_60d21b4967d0d8992e610c85",
  "orderNumber": "ORD20250521001",
  "status": "shipped",
  "statusHistory": [
    {
      "status": "pending_payment",
      "timestamp": "2025-05-21T01:15:30Z",
      "comment": "訂單已創建，等待支付"
    },
    {
      "status": "processing",
      "timestamp": "2025-05-21T01:20:15Z",
      "comment": "支付已完成，訂單處理中"
    },
    {
      "status": "shipped",
      "timestamp": "2025-05-21T10:05:30Z",
      "comment": "訂單已發貨，預計3-5天送達"
    }
  ],
  "updatedAt": "2025-05-21T10:05:30Z"
}
```

### 取消訂單

取消指定的訂單。

```
POST /api/orders/{id}/cancel
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 訂單ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| reason | string | 是 | 取消原因 |

#### 請求示例

```json
{
  "reason": "客戶要求取消"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 訂單取消成功 |
| 400 | 請求參數無效或訂單狀態不允許取消 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 訂單不存在 |

#### 成功響應示例

```json
{
  "id": "ord_60d21b4967d0d8992e610c85",
  "orderNumber": "ORD20250521001",
  "status": "cancelled",
  "statusHistory": [
    {
      "status": "pending_payment",
      "timestamp": "2025-05-21T01:15:30Z",
      "comment": "訂單已創建，等待支付"
    },
    {
      "status": "cancelled",
      "timestamp": "2025-05-21T01:30:45Z",
      "comment": "客戶要求取消"
    }
  ],
  "updatedAt": "2025-05-21T01:30:45Z"
}
```

### 更新訂單支付信息

更新訂單的支付信息。通常由支付服務調用。

```
PUT /api/orders/{id}/payment
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 訂單ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| paymentId | string | 是 | 支付交易ID |

#### 請求示例

```json
{
  "paymentId": "pay_60d21b4967d0d8992e610d10"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 訂單支付信息更新成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 訂單不存在 |

#### 成功響應示例

```json
{
  "id": "ord_60d21b4967d0d8992e610c85",
  "orderNumber": "ORD20250521001",
  "status": "processing",
  "paymentId": "pay_60d21b4967d0d8992e610d10",
  "updatedAt": "2025-05-21T01:20:15Z"
}
```

### 獲取訂單狀態歷史

獲取指定訂單的狀態變更歷史。

```
GET /api/orders/{id}/history
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 訂單ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 訂單不存在 |

#### 成功響應示例

```json
[
  {
    "status": "pending_payment",
    "timestamp": "2025-05-21T01:15:30Z",
    "comment": "訂單已創建，等待支付",
    "userId": "usr_60d21b4967d0d8992e610c80"
  },
  {
    "status": "processing",
    "timestamp": "2025-05-21T01:20:15Z",
    "comment": "支付已完成，訂單處理中",
    "userId": "sys_payment"
  },
  {
    "status": "shipped",
    "timestamp": "2025-05-21T10:05:30Z",
    "comment": "訂單已發貨，預計3-5天送達",
    "userId": "adm_60d21b4967d0d8992e610c70"
  }
]
```

## 購物車 API 端點

### 創建購物車

創建新的購物車。

```
POST /api/carts
```

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| sessionId | string | 否 | 會話ID，未登入用戶必填 |
| userId | string | 否 | 用戶ID，已登入用戶必填 |

#### 請求示例

```json
{
  "sessionId": "sess_60d21b4967d0d8992e610d20"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 201 | 購物車創建成功 |
| 400 | 請求參數無效 |

#### 成功響應示例

```json
{
  "id": 123,
  "sessionId": "sess_60d21b4967d0d8992e610d20",
  "userId": null,
  "status": "active",
  "items": [],
  "subtotal": 0.00,
  "createdAt": "2025-05-21T01:00:00Z",
  "updatedAt": "2025-05-21T01:00:00Z",
  "expiresAt": "2025-05-28T01:00:00Z"
}
```

### 獲取購物車

通過ID獲取特定購物車。

```
GET /api/carts/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | integer | 是 | 購物車ID |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 404 | 購物車不存在 |

#### 成功響應示例

```json
{
  "id": 123,
  "sessionId": "sess_60d21b4967d0d8992e610d20",
  "userId": null,
  "status": "active",
  "items": [
    {
      "id": 1,
      "productId": "prd_60d21b4967d0d8992e610c90",
      "productName": "智能手錶",
      "variantId": "var_60d21b4967d0d8992e610c91",
      "variantName": "黑色 42mm",
      "quantity": 1,
      "unitPrice": 2499.99,
      "totalPrice": 2499.99,
      "imageUrl": "https://example.com/images/smartwatch-black.jpg"
    }
  ],
  "subtotal": 2499.99,
  "createdAt": "2025-05-21T01:00:00Z",
  "updatedAt": "2025-05-21T01:05:30Z",
  "expiresAt": "2025-05-28T01:00:00Z"
}
```

### 根據會話ID獲取購物車

通過會話ID獲取未登入用戶的購物車。

```
GET /api/carts/session/{sessionId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| sessionId | string | 是 | 會話ID |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 404 | 購物車不存在 |

#### 成功響應示例

與獲取購物車API相同

### 根據用戶ID獲取購物車

通過用戶ID獲取已登入用戶的購物車。

```
GET /api/carts/user/{userId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| userId | string | 是 | 用戶ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 購物車不存在 |

#### 成功響應示例

與獲取購物車API相同

### 添加購物車項目

向購物車添加商品。

```
POST /api/carts/{id}/items
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | integer | 是 | 購物車ID |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| productId | string | 是 | 商品ID |
| variantId | string | 否 | 商品變體ID |
| quantity | integer | 是 | 數量 |

#### 請求示例

```json
{
  "productId": "prd_60d21b4967d0d8992e610c90",
  "variantId": "var_60d21b4967d0d8992e610c91",
  "quantity": 1
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 400 | 請求參數無效 |
| 404 | 購物車或商品不存在 |
| 409 | 商品庫存不足 |

#### 成功響應示例

```json
{
  "id": 123,
  "sessionId": "sess_60d21b4967d0d8992e610d20",
  "userId": null,
  "status": "active",
  "items": [
    {
      "id": 1,
      "productId": "prd_60d21b4967d0d8992e610c90",
      "productName": "智能手錶",
      "variantId": "var_60d21b4967d0d8992e610c91",
      "variantName": "黑色 42mm",
      "quantity": 1,
      "unitPrice": 2499.99,
      "totalPrice": 2499.99,
      "imageUrl": "https://example.com/images/smartwatch-black.jpg"
    }
  ],
  "subtotal": 2499.99,
  "updatedAt": "2025-05-21T01:05:30Z"
}
```

### 更新購物車項目

更新購物車中的商品數量。

```
PUT /api/carts/{id}/items/{itemId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | integer | 是 | 購物車ID |
| itemId | integer | 是 | 購物車項目ID |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| quantity | integer | 是 | 新數量 |

#### 請求示例

```json
{
  "quantity": 2
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 400 | 請求參數無效 |
| 404 | 購物車或項目不存在 |
| 409 | 商品庫存不足 |

#### 成功響應示例

```json
{
  "id": 123,
  "items": [
    {
      "id": 1,
      "productId": "prd_60d21b4967d0d8992e610c90",
      "productName": "智能手錶",
      "variantId": "var_60d21b4967d0d8992e610c91",
      "variantName": "黑色 42mm",
      "quantity": 2,
      "unitPrice": 2499.99,
      "totalPrice": 4999.98,
      "imageUrl": "https://example.com/images/smartwatch-black.jpg"
    }
  ],
  "subtotal": 4999.98,
  "updatedAt": "2025-05-21T01:10:15Z"
}
```

### 刪除購物車項目

從購物車中刪除商品。

```
DELETE /api/carts/{id}/items/{itemId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | integer | 是 | 購物車ID |
| itemId | integer | 是 | 購物車項目ID |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 404 | 購物車或項目不存在 |

#### 成功響應示例

```json
{
  "id": 123,
  "items": [],
  "subtotal": 0.00,
  "updatedAt": "2025-05-21T01:15:00Z"
}
```

### 清空購物車

刪除購物車中的所有商品。

```
DELETE /api/carts/{id}/items
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | integer | 是 | 購物車ID |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 404 | 購物車不存在 |

#### 成功響應示例

```json
{
  "id": 123,
  "items": [],
  "subtotal": 0.00,
  "updatedAt": "2025-05-21T01:15:00Z"
}
```

### 合併購物車

合併未登入用戶的購物車到已登入用戶的購物車。

```
POST /api/carts/merge
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| sessionId | string | 是 | 會話ID |
| userId | string | 是 | 用戶ID |

#### 請求示例

```json
{
  "sessionId": "sess_60d21b4967d0d8992e610d20",
  "userId": "usr_60d21b4967d0d8992e610c80"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 404 | 購物車不存在 |

#### 成功響應示例

```json
{
  "id": 124,
  "sessionId": null,
  "userId": "usr_60d21b4967d0d8992e610c80",
  "status": "active",
  "items": [
    {
      "id": 2,
      "productId": "prd_60d21b4967d0d8992e610c90",
      "productName": "智能手錶",
      "variantId": "var_60d21b4967d0d8992e610c91",
      "variantName": "黑色 42mm",
      "quantity": 1,
      "unitPrice": 2499.99,
      "totalPrice": 2499.99,
      "imageUrl": "https://example.com/images/smartwatch-black.jpg"
    }
  ],
  "subtotal": 2499.99,
  "updatedAt": "2025-05-21T01:20:00Z"
}
```

## 訂單狀態

| 狀態 | 描述 |
|--------|------|
| pending_payment | 待支付 |
| processing | 處理中 |
| shipped | 已發貨 |
| delivered | 已送達 |
| cancelled | 已取消 |
| refunded | 已退款 |
| completed | 已完成 |

## 錯誤碼

| 錯誤碼 | 描述 |
|--------|------|
| ORDER_NOT_FOUND | 訂單不存在 |
| CART_NOT_FOUND | 購物車不存在 |
| CART_ITEM_NOT_FOUND | 購物車項目不存在 |
| INVALID_ORDER_DATA | 無效的訂單數據 |
| INVALID_CART_DATA | 無效的購物車數據 |
| INSUFFICIENT_STOCK | 庫存不足 |
| INVALID_STATUS_TRANSITION | 無效的狀態轉換 |
| ORDER_ALREADY_CANCELLED | 訂單已被取消 |
| CART_ALREADY_USED | 購物車已被使用 |
| PERMISSION_DENIED | 權限不足 |
```

現在我將創建支付服務的API文檔：

```md:docs/swagger/payment-service-swagger.md
# 支付服務 API 文檔

本文檔描述支付服務(Payment Service)的API端點。

## API 端點

### 創建支付交易

創建新的支付交易。

```
POST /api/payments
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| orderId | string | 是 | 訂單ID |
| orderNumber | string | 是 | 訂單編號 |
| amount | number | 是 | 支付金額 |
| currency | string | 是 | 貨幣代碼，例如 TWD |
| paymentMethod | string | 是 | 支付方式代碼 |
| returnUrl | string | 是 | 支付完成後的返回URL |
| metadata | object | 否 | 自定義元數據 |

#### 請求示例

```json
{
  "orderId": "ord_60d21b4967d0d8992e610c85",
  "orderNumber": "ORD20250521001",
  "amount": 2684.99,
  "currency": "TWD",
  "paymentMethod": "credit_card",
  "returnUrl": "https://example.com/checkout/complete",
  "metadata": {
    "customerName": "張三",
    "items": 1
  }
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 201 | 支付交易創建成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 404 | 支付方式不存在 |

#### 成功響應示例

```json
{
  "transactionId": "pay_60d21b4967d0d8992e610d10",
  "orderId": "ord_60d21b4967d0d8992e610c85",
  "orderNumber": "ORD20250521001",
  "amount": 2684.99,
  "currency": "TWD",
  "status": "pending",
  "paymentMethod": "credit_card",
  "paymentUrl": "https://payment-gateway.example.com/pay/session_123456",
  "createdAt": "2025-05-21T01:15:45Z",
  "expiresAt": "2025-05-21T01:45:45Z"
}
```

### 獲取支付交易

通過ID獲取特定支付交易的詳細信息。

```
GET /api/payments/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 支付交易ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 支付交易不存在 |

#### 成功響應示例

```json
{
  "transactionId": "pay_60d21b4967d0d8992e610d10",
  "orderId": "ord_60d21b4967d0d8992e610c85",
  "orderNumber": "ORD20250521001",
  "amount": 2684.99,
  "currency": "TWD",
  "status": "completed",
  "paymentMethod": "credit_card",
  "paymentMethodDetails": {
    "cardBrand": "Visa",
    "last4": "4242",
    "expiryMonth": 12,
    "expiryYear": 2025
  },
  "metadata": {
    "customerName": "張三",
    "items": 1
  },
  "statusHistory": [
    {
      "status": "pending",
      "timestamp": "2025-05-21T01:15:45Z",
      "comment": "支付交易已創建"
    },
    {
      "status": "processing",
      "timestamp": "2025-05-21T01:18:30Z",
      "comment": "支付處理中"
    },
    {
      "status": "completed",
      "timestamp": "2025-05-21T01:20:00Z",
      "comment": "支付已完成"
    }
  ],
  "createdAt": "2025-05-21T01:15:45Z",
  "updatedAt": "2025-05-21T01:20:00Z",
  "completedAt": "2025-05-21T01:20:00Z"
}
```

### 根據訂單ID獲取支付交易

通過訂單ID獲取相關的支付交易列表。

```
GET /api/payments/order/{orderId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| orderId | string | 是 | 訂單ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 訂單不存在 |

#### 成功響應示例

```json
[
  {
    "transactionId": "pay_60d21b4967d0d8992e610d10",
    "orderId": "ord_60d21b4967d0d8992e610c85",
    "orderNumber": "ORD20250521001",
    "amount": 2684.99,
    "currency": "TWD",
    "status": "completed",
    "paymentMethod": "credit_card",
    "createdAt": "2025-05-21T01:15:45Z",
    "updatedAt": "2025-05-21T01:20:00Z"
  }
]
```

### 獲取支付方式

獲取所有可用的支付方式。

```
GET /api/payments/methods
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |

#### 成功響應示例

```json
[
  {
    "id": "credit_card",
    "name": "信用卡",
    "description": "使用Visa、Mastercard或JCB信用卡付款",
    "icon": "https://example.com/icons/credit-card.png",
    "isActive": true,
    "supportedCurrencies": ["TWD", "USD", "JPY"],
    "processingFee": {
      "percentage": 2.5,
      "fixed": 0
    }
  },
  {
    "id": "line_pay",
    "name": "LINE Pay",
    "description": "使用LINE Pay電子錢包付款",
    "icon": "https://example.com/icons/line-pay.png",
    "isActive": true,
    "supportedCurrencies": ["TWD"],
    "processingFee": {
      "percentage": 2.0,
      "fixed": 0
    }
  }
]
```

### 完成支付

完成指定的支付交易。通常由支付提供商的回調觸發，或在測試環境中手動調用。

```
POST /api/payments/{id}/capture
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 支付交易ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 支付完成成功 |
| 400 | 請求參數無效或支付狀態不允許完成 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 支付交易不存在 |

#### 成功響應示例

```json
{
  "transactionId": "pay_60d21b4967d0d8992e610d10",
  "status": "completed",
  "statusHistory": [
    {
      "status": "pending",
      "timestamp": "2025-05-21T01:15:45Z",
      "comment": "支付交易已創建"
    },
    {
      "status": "processing",
      "timestamp": "2025-05-21T01:18:30Z",
      "comment": "支付處理中"
    },
    {
      "status": "completed",
      "timestamp": "2025-05-21T01:20:00Z",
      "comment": "支付已完成"
    }
  ],
  "updatedAt": "2025-05-21T01:20:00Z",
  "completedAt": "2025-05-21T01:20:00Z"
}
```

### 取消支付

取消指定的支付交易。

```
POST /api/payments/{id}/cancel
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 支付交易ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| reason | string | 否 | 取消原因 |

#### 請求示例

```json
{
  "reason": "客戶要求取消"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 支付取消成功 |
| 400 | 請求參數無效或支付狀態不允許取消 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 支付交易不存在 |

#### 成功響應示例

```json
{
  "transactionId": "pay_60d21b4967d0d8992e610d10",
  "status": "cancelled",
  "statusHistory": [
    {
      "status": "pending",
      "timestamp": "2025-05-21T01:15:45Z",
      "comment": "支付交易已創建"
    },
    {
      "status": "cancelled",
      "timestamp": "2025-05-21T01:25:30Z",
      "comment": "客戶要求取消"
    }
  ],
  "updatedAt": "2025-05-21T01:25:30Z"
}
```

### 創建退款

對已完成的支付交易創建退款。

```
POST /api/refunds
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| transactionId | string | 是 | 原支付交易ID |
| amount | number | 是 | 退款金額 |
| reason | string | 是 | 退款原因 |
| metadata | object | 否 | 自定義元數據 |

#### 請求示例

```json
{
  "transactionId": "pay_60d21b4967d0d8992e610d10",
  "amount": 2684.99,
  "reason": "客戶退貨",
  "metadata": {
    "returnId": "ret_60d21b4967d0d8992e610e10"
  }
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 201 | 退款創建成功 |
| 400 | 請求參數無效或原交易不允許退款 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 原支付交易不存在 |
| 409 | 退款金額超過可退款金額 |

#### 成功響應示例

```json
{
  "refundId": "ref_60d21b4967d0d8992e610e20",
  "transactionId": "pay_60d21b4967d0d8992e610d10",
  "amount": 2684.99,
  "currency": "TWD",
  "reason": "客戶退貨",
  "status": "pending",
  "metadata": {
    "returnId": "ret_60d21b4967d0d8992e610e10"
  },
  "createdAt": "2025-05-22T10:30:00Z",
  "updatedAt": "2025-05-22T10:30:00Z"
}
```

### 獲取退款

通過ID獲取特定退款的詳細信息。

```
GET /api/refunds/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 退款ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 退款不存在 |

#### 成功響應示例

```json
{
  "refundId": "ref_60d21b4967d0d8992e610e20",
  "transactionId": "pay_60d21b4967d0d8992e610d10",
  "amount": 2684.99,
  "currency": "TWD",
  "reason": "客戶退貨",
  "status": "completed",
  "metadata": {
    "returnId": "ret_60d21b4967d0d8992e610e10"
  },
  "statusHistory": [
    {
      "status": "pending",
      "timestamp": "2025-05-22T10:30:00Z",
      "comment": "退款請求已創建"
    },
    {
      "status": "processing",
      "timestamp": "2025-05-22T10:32:15Z",
      "comment": "退款處理中"
    },
    {
      "status": "completed",
      "timestamp": "2025-05-22T10:35:00Z",
      "comment": "退款已完成"
    }
  ],
  "createdAt": "2025-05-22T10:30:00Z",
  "updatedAt": "2025-05-22T10:35:00Z",
  "completedAt": "2025-05-22T10:35:00Z"
}
```

### 根據支付交易ID獲取退款

通過支付交易ID獲取相關的退款列表。

```
GET /api/refunds/transaction/{transactionId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| transactionId | string | 是 | 支付交易ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 支付交易不存在 |

#### 成功響應示例

```json
[
  {
    "refundId": "ref_60d21b4967d0d8992e610e20",
    "transactionId": "pay_60d21b4967d0d8992e610d10",
    "amount": 2684.99,
    "currency": "TWD",
    "reason": "客戶退貨",
    "status": "completed",
    "createdAt": "2025-05-22T10:30:00Z",
    "updatedAt": "2025-05-22T10:35:00Z"
  }
]
```

### 處理支付回調

處理來自支付提供商的回調通知。

```
POST /api/payments/webhook
```

#### 請求參數

依賴於支付提供商的回調格式，通常包含交易ID、狀態和簽名等信息。

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 回調處理成功 |
| 400 | 回調數據無效或處理失敗 |

#### 成功響應示例

```json
{
  "success": true,
  "message": "Webhook processed successfully"
}
```

### 獲取支付統計信息

獲取支付統計信息。需要管理員權限。

```
GET /api/payments/stats
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 查詢參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| startDate | string | 否 | 開始日期 (YYYY-MM-DD) |
| endDate | string | 否 | 結束日期 (YYYY-MM-DD) |
| paymentMethod | string | 否 | 支付方式過濾 |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |

#### 成功響應示例

```json
{
  "totalTransactions": 125,
  "totalAmount": 350750.25,
  "successfulTransactions": 120,
  "failedTransactions": 5,
  "refundedTransactions": 3,
  "refundedAmount": 8500.00,
  "averageTransactionAmount": 2922.92,
  "byPaymentMethod": [
    {
      "method": "credit_card",
      "count": 95,
      "amount": 290500.25
    },
    {
      "method": "line_pay",
      "count": 25,
      "amount": 60250.00
    }
  ],
  "byDate": [
    {
      "date": "2025-05-20",
      "count": 60,
      "amount": 175250.25
    },
    {
      "date": "2025-05-21",
      "count": 65,
      "amount": 175500.00
    }
  ]
}
```

## 支付交易狀態

| 狀態 | 描述 |
|--------|------|
| pending | 待處理 |
| processing | 處理中 |
| completed | 已完成 |
| failed | 失敗 |
| cancelled | 已取消 |
| refunded | 已退款 |
| partially_refunded | 部分退款 |

## 退款狀態

| 狀態 | 描述 |
|--------|------|
| pending | 待處理 |
| processing | 處理中 |
| completed | 已完成 |
| failed | 失敗 |

## 錯誤碼

| 錯誤碼 | 描述 |
|--------|------|
| PAYMENT_NOT_FOUND | 支付交易不存在 |
| REFUND_NOT_FOUND | 退款不存在 |
| PAYMENT_METHOD_NOT_FOUND | 支付方式不存在 |
| INVALID_PAYMENT_DATA | 無效的支付數據 |
| INVALID_REFUND_DATA | 無效的退款數據 |
| PAYMENT_ALREADY_COMPLETED | 支付已完成 |
| PAYMENT_ALREADY_CANCELLED | 支付已取消 |
| PAYMENT_NOT_COMPLETED | 支付未完成 |
| REFUND_AMOUNT_EXCEEDS_PAYMENT | 退款金額超過支付金額 |
| INVALID_WEBHOOK_SIGNATURE | 無效的回調簽名 |
| GATEWAY_ERROR | 支付網關錯誤 |
| PERMISSION_DENIED | 權限不足 |
```

現在我已經創建了兩個API文檔：
1. 訂單服務API文檔 - 包含訂單管理和購物車管理的所有端點
2. 支付服務API文檔 - 包含支付交易和退款管理的所有端點

這些文檔遵循了與現有產品服務API文檔相同的格式，提供了詳細的API端點描述、請求參數、響應格式和示例。每個文檔都包含了錯誤碼和狀態代碼的說明，以便開發人員能夠更好地理解和使用這些API。

這些API文檔可以幫助開發團隊了解訂單服務和支付服務的功能和使用方式，促進前後端開發的協作。同時，這些文檔也可以作為系統設計和實現的參考，確保各個服務之間的一致性和互操作性。