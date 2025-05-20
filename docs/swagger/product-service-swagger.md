# 商品服務 API 文檔

本文檔描述商品服務(Product Service)的API端點。

## API 端點

### 獲取所有商品

獲取所有商品的列表，支持分頁、排序和過濾。

```
GET /api/products
```

#### 查詢參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| page | integer | 否 | 頁碼，默認為1 |
| pageSize | integer | 否 | 每頁項目數，默認為10 |
| sortBy | string | 否 | 排序字段，如"price"、"name"等 |
| sortOrder | string | 否 | 排序方向，"asc"或"desc"，默認為"asc" |
| categoryId | string | 否 | 按類別ID過濾 |
| minPrice | number | 否 | 最低價格過濾 |
| maxPrice | number | 否 | 最高價格過濾 |
| search | string | 否 | 搜索關鍵詞，匹配商品名稱或描述 |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 400 | 請求參數無效 |

#### 成功響應示例

```json
{
  "items": [
    {
      "id": "60d21b4967d0d8992e610c85",
      "name": "智能手錶",
      "description": "高級智能手錶，支持心率監測和通知推送",
      "price": 2499.99,
      "categoryId": "60d21b4967d0d8992e610c80",
      "categoryName": "電子產品",
      "imageUrl": "https://example.com/images/smartwatch.jpg",
      "inStock": true,
      "stockQuantity": 50,
      "createdAt": "2023-05-20T10:30:00Z",
      "updatedAt": "2023-05-20T10:30:00Z"
    },
    // 更多商品...
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  }
}
```

### 獲取單個商品

通過ID獲取特定商品的詳細信息。

```
GET /api/products/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 商品ID |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 404 | 商品不存在 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c85",
  "name": "智能手錶",
  "description": "高級智能手錶，支持心率監測和通知推送",
  "price": 2499.99,
  "categoryId": "60d21b4967d0d8992e610c80",
  "categoryName": "電子產品",
  "imageUrl": "https://example.com/images/smartwatch.jpg",
  "inStock": true,
  "stockQuantity": 50,
  "specifications": {
    "color": "黑色",
    "weight": "45g",
    "waterproof": true,
    "batteryLife": "5天"
  },
  "createdAt": "2023-05-20T10:30:00Z",
  "updatedAt": "2023-05-20T10:30:00Z"
}
```

### 創建商品

創建新商品。需要管理員權限。

```
POST /api/products
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| name | string | 是 | 商品名稱 |
| description | string | 是 | 商品描述 |
| price | number | 是 | 商品價格 |
| categoryId | string | 是 | 類別ID |
| imageUrl | string | 否 | 商品圖片URL |
| stockQuantity | integer | 是 | 庫存數量 |
| specifications | object | 否 | 商品規格，鍵值對格式 |

#### 請求示例

```json
{
  "name": "智能手錶",
  "description": "高級智能手錶，支持心率監測和通知推送",
  "price": 2499.99,
  "categoryId": "60d21b4967d0d8992e610c80",
  "imageUrl": "https://example.com/images/smartwatch.jpg",
  "stockQuantity": 50,
  "specifications": {
    "color": "黑色",
    "weight": "45g",
    "waterproof": true,
    "batteryLife": "5天"
  }
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 201 | 商品創建成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 403 | 權限不足 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c85",
  "name": "智能手錶",
  "description": "高級智能手錶，支持心率監測和通知推送",
  "price": 2499.99,
  "categoryId": "60d21b4967d0d8992e610c80",
  "categoryName": "電子產品",
  "imageUrl": "https://example.com/images/smartwatch.jpg",
  "inStock": true,
  "stockQuantity": 50,
  "specifications": {
    "color": "黑色",
    "weight": "45g",
    "waterproof": true,
    "batteryLife": "5天"
  },
  "createdAt": "2023-05-20T10:30:00Z",
  "updatedAt": "2023-05-20T10:30:00Z"
}
```

### 更新商品

更新現有商品。需要管理員權限。

```
PUT /api/products/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 商品ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| name | string | 否 | 商品名稱 |
| description | string | 否 | 商品描述 |
| price | number | 否 | 商品價格 |
| categoryId | string | 否 | 類別ID |
| imageUrl | string | 否 | 商品圖片URL |
| stockQuantity | integer | 否 | 庫存數量 |
| specifications | object | 否 | 商品規格，鍵值對格式 |

#### 請求示例

```json
{
  "price": 2299.99,
  "stockQuantity": 100
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 商品更新成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 商品不存在 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c85",
  "name": "智能手錶",
  "description": "高級智能手錶，支持心率監測和通知推送",
  "price": 2299.99,
  "categoryId": "60d21b4967d0d8992e610c80",
  "categoryName": "電子產品",
  "imageUrl": "https://example.com/images/smartwatch.jpg",
  "inStock": true,
  "stockQuantity": 100,
  "specifications": {
    "color": "黑色",
    "weight": "45g",
    "waterproof": true,
    "batteryLife": "5天"
  },
  "updatedAt": "2023-05-21T15:45:00Z"
}
```

### 刪除商品

刪除指定商品。需要管理員權限。

```
DELETE /api/products/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 商品ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 204 | 商品刪除成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 商品不存在 |

### 獲取所有類別

獲取所有商品類別。

```
GET /api/categories
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |

#### 成功響應示例

```json
[
  {
    "id": "60d21b4967d0d8992e610c80",
    "name": "電子產品",
    "description": "各類電子設備和配件",
    "parentId": null,
    "createdAt": "2023-05-20T10:30:00Z"
  },
  {
    "id": "60d21b4967d0d8992e610c81",
    "name": "智能手錶",
    "description": "智能手錶和配件",
    "parentId": "60d21b4967d0d8992e610c80",
    "createdAt": "2023-05-20T10:30:00Z"
  },
  // 更多類別...
]
```

### 獲取單個類別

通過ID獲取特定類別的詳細信息。

```
GET /api/categories/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 類別ID |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 404 | 類別不存在 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c80",
  "name": "電子產品",
  "description": "各類電子設備和配件",
  "parentId": null,
  "subCategories": [
    {
      "id": "60d21b4967d0d8992e610c81",
      "name": "智能手錶",
      "description": "智能手錶和配件",
      "parentId": "60d21b4967d0d8992e610c80"
    },
    // 更多子類別...
  ],
  "createdAt": "2023-05-20T10:30:00Z",
  "updatedAt": "2023-05-20T10:30:00Z"
}
```

### 創建類別

創建新類別。需要管理員權限。

```
POST /api/categories
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| name | string | 是 | 類別名稱 |
| description | string | 否 | 類別描述 |
| parentId | string | 否 | 父類別ID，如果是頂級類別則為null |

#### 請求示例

```json
{
  "name": "智能家居",
  "description": "智能家居產品和配件",
  "parentId": "60d21b4967d0d8992e610c80"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 201 | 類別創建成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 403 | 權限不足 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c82",
  "name": "智能家居",
  "description": "智能家居產品和配件",
  "parentId": "60d21b4967d0d8992e610c80",
  "createdAt": "2023-05-21T15:45:00Z",
  "updatedAt": "2023-05-21T15:45:00Z"
}
```

### 更新類別

更新現有類別。需要管理員權限。

```
PUT /api/categories/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 類別ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| name | string | 否 | 類別名稱 |
| description | string | 否 | 類別描述 |
| parentId | string | 否 | 父類別ID |

#### 請求示例

```json
{
  "name": "智能家居設備",
  "description": "各類智能家居產品和配件"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 類別更新成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 類別不存在 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c82",
  "name": "智能家居設備",
  "description": "各類智能家居產品和配件",
  "parentId": "60d21b4967d0d8992e610c80",
  "updatedAt": "2023-05-22T09:15:00Z"
}
```

### 刪除類別

刪除指定類別。需要管理員權限。

```
DELETE /api/categories/{id}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| id | string | 是 | 類別ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 204 | 類別刪除成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 類別不存在 |
| 409 | 類別包含子類別或商品，無法刪除 |

### 獲取庫存信息

獲取指定商品的庫存信息。

```
GET /api/inventory/{productId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| productId | string | 是 | 商品ID |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 404 | 商品不存在 |

#### 成功響應示例

```json
{
  "productId": "60d21b4967d0d8992e610c85",
  "inStock": true,
  "quantity": 50,
  "reservedQuantity": 5,
  "availableQuantity": 45,
  "lastUpdated": "2023-05-21T15:45:00Z"
}
```

### 更新庫存

更新指定商品的庫存數量。需要管理員權限。

```
PUT /api/inventory/{productId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| productId | string | 是 | 商品ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| quantity | integer | 是 | 新的庫存數量 |

#### 請求示例

```json
{
  "quantity": 100
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 庫存更新成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 商品不存在 |

#### 成功響應示例

```json
{
  "productId": "60d21b4967d0d8992e610c85",
  "inStock": true,
  "quantity": 100,
  "reservedQuantity": 5,
  "availableQuantity": 95,
  "lastUpdated": "2023-05-22T09:15:00Z"
}
```

## 錯誤碼

| 錯誤碼 | 描述 |
|--------|------|
| PRODUCT_NOT_FOUND | 商品不存在 |
| CATEGORY_NOT_FOUND | 類別不存在 |
| INVALID_PRODUCT_DATA | 無效的商品數據 |
| INVALID_CATEGORY_DATA | 無效的類別數據 |
| CATEGORY_IN_USE | 類別正在使用中，無法刪除 |
| INSUFFICIENT_STOCK | 庫存不足 |
| PERMISSION_DENIED | 權限不足 |