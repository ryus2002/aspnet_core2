讓我為您完成這些任務。首先，我會瀏覽專案目錄結構，以便了解現有的文檔狀態。
我看到專案中已經有一個 `inventory-alert-system.md` 文件，這是我們之前創建的庫存預警系統設計文檔。現在我將繼續完成用戶操作指南和更新 API 文檔。

讓我先查看一下產品服務的 Swagger 文檔，以了解現有的 API 文檔格式：
現在我已經了解了現有的文檔結構和格式。接下來，我將創建庫存預警系統的用戶操作指南，並更新 API 文檔以包含新的庫存預警 API 端點。

首先，讓我創建用戶操作指南：

```markdown:docs/inventory-alert-system-user-guide.md
# 庫存預警系統用戶操作指南

## 概述

本指南旨在幫助管理員和倉庫管理人員使用庫存預警系統，有效監控商品庫存水平，及時處理庫存不足的情況。

## 功能總覽

庫存預警系統提供以下核心功能：

1. 自動監控商品庫存水平
2. 當庫存低於預設閾值時生成預警
3. 查看和管理預警（解決、忽略）
4. 設定和調整低庫存閾值
5. 定期掃描所有商品庫存

## 使用指南

### 1. 設定低庫存閾值

每個商品或變體都可以設定自己的低庫存閾值，當庫存數量低於此閾值時，系統將生成預警。

#### 設定步驟：

1. 進入商品管理頁面
2. 選擇需要設定閾值的商品
3. 點擊「設定庫存閾值」按鈕
4. 輸入閾值數量（例如：當庫存低於 10 件時發出預警）
5. 點擊「保存」按鈕

> **注意**：對於變體商品，可以為每個變體單獨設定閾值。

#### API 使用方式：

```http
PUT /api/inventory/alerts/threshold/{productId}
Content-Type: application/json

{
  "threshold": 10
}
```

若要為變體設定閾值，請添加變體 ID 參數：

```http
PUT /api/inventory/alerts/threshold/{productId}?variantId={variantId}
Content-Type: application/json

{
  "threshold": 5
}
```

### 2. 查看庫存預警

系統會自動生成庫存預警，管理員可以通過以下方式查看：

#### 查看步驟：

1. 進入「庫存預警」頁面
2. 系統顯示所有未解決的預警，按嚴重程度排序
3. 點擊預警可查看詳細信息

#### 預警類型：

- **低庫存預警**：庫存低於閾值但尚未售罄
- **缺貨預警**：庫存已售罄

#### 嚴重程度：

- **低**：庫存接近閾值
- **中**：庫存低於閾值的一半
- **高**：商品已缺貨

#### API 使用方式：

獲取所有未解決的預警：

```http
GET /api/inventory/alerts
```

獲取特定商品的預警：

```http
GET /api/inventory/alerts/product/{productId}
```

### 3. 處理庫存預警

當發現庫存預警時，管理員需要採取適當的行動：

#### 處理步驟：

1. 檢查預警詳情，了解庫存狀況
2. 根據情況採取行動：
   - 補充庫存
   - 暫停銷售該商品
   - 調整庫存閾值
3. 處理完成後，將預警標記為「已解決」或「已忽略」

#### API 使用方式：

解決預警：

```http
POST /api/inventory/alerts/{alertId}/resolve
Content-Type: application/json

{
  "userId": "管理員ID",
  "notes": "已補充庫存 50 件"
}
```

忽略預警：

```http
POST /api/inventory/alerts/{alertId}/ignore
Content-Type: application/json

{
  "userId": "管理員ID",
  "notes": "此商品即將下架，無需補充庫存"
}
```

### 4. 手動檢查庫存

除了系統自動檢查外，管理員還可以隨時手動檢查商品庫存：

#### 檢查步驟：

1. 進入商品詳情頁面
2. 點擊「檢查庫存」按鈕
3. 系統會立即檢查該商品的庫存狀態，並在必要時生成預警

#### API 使用方式：

檢查特定商品：

```http
POST /api/inventory/alerts/check/{productId}
```

檢查所有商品：

```http
POST /api/inventory/alerts/check-all
```

## 最佳實踐

1. **定期檢查預警**：建議每天檢查庫存預警，及時處理庫存問題
2. **合理設定閾值**：根據商品的銷售速度和補貨週期設定合適的閾值
3. **記錄處理過程**：解決預警時添加詳細備註，便於後續追蹤
4. **定期分析數據**：定期分析庫存預警數據，優化庫存管理策略

## 常見問題

### Q: 如何為所有商品批量設定閾值？

A: 目前系統不支持批量設定閾值，需要為每個商品單獨設定。未來版本將考慮添加此功能。

### Q: 預警通知方式有哪些？

A: 目前系統通過界面顯示預警信息。未來版本計劃添加電子郵件和手機推送通知功能。

### Q: 如何調整預警檢查頻率？

A: 預警檢查頻率可在系統配置中調整，默認為每 6 小時檢查一次。請聯繫系統管理員修改此設置。

### Q: 已解決的預警可以重新打開嗎？

A: 已解決的預警無法重新打開。如果問題再次出現，系統會生成新的預警。

## 技術支持

如有任何問題或需要幫助，請聯繫技術支持團隊：

- 電子郵件：support@example.com
- 電話：(02) 1234-5678
- 工作時間：週一至週五 9:00-18:00
```

接下來，我將更新 API 文檔，添加庫存預警系統的 API 端點：

```markdown:docs/swagger/product-service-swagger.md
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

### 獲取未解決的庫存預警

獲取所有未解決的庫存預警，支持分頁。

```
GET /api/inventory/alerts
```

#### 查詢參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| page | integer | 否 | 頁碼，默認為1 |
| pageSize | integer | 否 | 每頁項目數，默認為20 |

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

#### 成功響應示例

```json
{
  "items": [
    {
      "id": "60d21b4967d0d8992e610c90",
      "productId": "60d21b4967d0d8992e610c85",
      "productName": "智能手錶",
      "variantId": null,
      "variantName": null,
      "alertType": "LowStock",
      "severity": "Medium",
      "status": "Created",
      "currentStock": 3,
      "threshold": 10,
      "message": "智能手錶 庫存過低，目前庫存為 3，低於閾值 10。",
      "suggestedAction": "請考慮補充庫存以避免缺貨。",
      "createdAt": "2023-05-22T09:15:00Z",
      "updatedAt": "2023-05-22T09:15:00Z",
      "notifiedAt": null,
      "resolvedAt": null,
      "resolvedBy": null,
      "resolutionNotes": null
    },
    {
      "id": "60d21b4967d0d8992e610c91",
      "productId": "60d21b4967d0d8992e610c86",
      "productName": "無線藍牙耳機",
      "variantId": "60d21b4967d0d8992e610c87",
      "variantName": "黑色",
      "alertType": "OutOfStock",
      "severity": "High",
      "status": "Notified",
      "currentStock": 0,
      "threshold": 5,
      "message": "無線藍牙耳機 - 黑色 已缺貨，目前庫存為 0。",
      "suggestedAction": "請立即補充庫存或暫停銷售此商品。",
      "createdAt": "2023-05-21T15:45:00Z",
      "updatedAt": "2023-05-21T16:00:00Z",
      "notifiedAt": "2023-05-21T16:00:00Z",
      "resolvedAt": null,
      "resolvedBy": null,
      "resolutionNotes": null
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 2,
  "totalPages": 1
}
```

### 獲取特定商品的庫存預警

獲取特定商品的庫存預警。

```
GET /api/inventory/alerts/product/{productId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| productId | string | 是 | 商品ID |

#### 查詢參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| includeResolved | boolean | 否 | 是否包含已解決的預警，默認為false |

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

#### 成功響應示例

```json
[
  {
    "id": "60d21b4967d0d8992e610c90",
    "productId": "60d21b4967d0d8992e610c85",
    "productName": "智能手錶",
    "variantId": null,
    "variantName": null,
    "alertType": "LowStock",
    "severity": "Medium",
    "status": "Created",
    "currentStock": 3,
    "threshold": 10,
    "message": "智能手錶 庫存過低，目前庫存為 3，低於閾值 10。",
    "suggestedAction": "請考慮補充庫存以避免缺貨。",
    "createdAt": "2023-05-22T09:15:00Z",
    "updatedAt": "2023-05-22T09:15:00Z",
    "notifiedAt": null,
    "resolvedAt": null,
    "resolvedBy": null,
    "resolutionNotes": null
  }
]
```

### 檢查特定商品庫存並生成預警

檢查特定商品的庫存狀態，如果庫存低於閾值則生成預警。

```
POST /api/inventory/alerts/check/{productId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| productId | string | 是 | 商品ID |

#### 查詢參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| variantId | string | 否 | 變體ID |

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
| 404 | 商品不存在 |

#### 成功響應示例 (有預警)

```json
{
  "id": "60d21b4967d0d8992e610c90",
  "productId": "60d21b4967d0d8992e610c85",
  "productName": "智能手錶",
  "variantId": null,
  "variantName": null,
  "alertType": "LowStock",
  "severity": "Medium",
  "status": "Created",
  "currentStock": 3,
  "threshold": 10,
  "message": "智能手錶 庫存過低，目前庫存為 3，低於閾值 10。",
  "suggestedAction": "請考慮補充庫存以避免缺貨。",
  "createdAt": "2023-05-22T09:15:00Z",
  "updatedAt": "2023-05-22T09:15:00Z",
  "notifiedAt": null,
  "resolvedAt": null,
  "resolvedBy": null,
  "resolutionNotes": null
}
```

#### 成功響應示例 (無預警)

```json
{
  "message": "商品庫存正常，無需生成預警"
}
```

### 批量檢查所有商品庫存

檢查所有商品的庫存狀態，並為庫存低於閾值的商品生成預警。

```
POST /api/inventory/alerts/check-all
```

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

#### 成功響應示例

```json
{
  "message": "已檢查所有商品庫存，生成了 5 個預警",
  "alertCount": 5
}
```

### 解決庫存預警

將庫存預警標記為已解決。

```
POST /api/inventory/alerts/{alertId}/resolve
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| alertId | string | 是 | 預警ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| userId | string | 是 | 處理者ID |
| notes | string | 否 | 解決備註 |

#### 請求示例

```json
{
  "userId": "60d21b4967d0d8992e610c01",
  "notes": "已補充庫存 50 件"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 預警不存在 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c90",
  "productId": "60d21b4967d0d8992e610c85",
  "productName": "智能手錶",
  "variantId": null,
  "variantName": null,
  "alertType": "LowStock",
  "severity": "Medium",
  "status": "Resolved",
  "currentStock": 3,
  "threshold": 10,
  "message": "智能手錶 庫存過低，目前庫存為 3，低於閾值 10。",
  "suggestedAction": "請考慮補充庫存以避免缺貨。",
  "createdAt": "2023-05-22T09:15:00Z",
  "updatedAt": "2023-05-22T10:30:00Z",
  "notifiedAt": null,
  "resolvedAt": "2023-05-22T10:30:00Z",
  "resolvedBy": "60d21b4967d0d8992e610c01",
  "resolutionNotes": "已補充庫存 50 件"
}
```

### 忽略庫存預警

將庫存預警標記為已忽略。

```
POST /api/inventory/alerts/{alertId}/ignore
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| alertId | string | 是 | 預警ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| userId | string | 是 | 處理者ID |
| notes | string | 否 | 忽略備註 |

#### 請求示例

```json
{
  "userId": "60d21b4967d0d8992e610c01",
  "notes": "此商品即將下架，無需補充庫存"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 預警不存在 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c90",
  "productId": "60d21b4967d0d8992e610c85",
  "productName": "智能手錶",
  "variantId": null,
  "variantName": null,
  "alertType": "LowStock",
  "severity": "Medium",
  "status": "Ignored",
  "currentStock": 3,
  "threshold": 10,
  "message": "智能手錶 庫存過低，目前庫存為 3，低於閾值 10。",
  "suggestedAction": "請考慮補充庫存以避免缺貨。",
  "createdAt": "2023-05-22T09:15:00Z",
  "updatedAt": "2023-05-22T10:30:00Z",
  "notifiedAt": null,
  "resolvedAt": "2023-05-22T10:30:00Z",
  "resolvedBy": "60d21b4967d0d8992e610c01",
  "resolutionNotes": "此商品即將下架，無需補充庫存"
}
```

### 設定低庫存閾值

設定商品或變體的低庫存閾值。

```
PUT /api/inventory/alerts/threshold/{productId}
```

#### 路徑參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| productId | string | 是 | 商品ID |

#### 查詢參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| variantId | string | 否 | 變體ID |

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| threshold | integer | 是 | 低庫存閾值 |

#### 請求示例

```json
{
  "threshold": 10
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 成功 |
| 400 | 請求參數無效 |
| 401 | 未認證 |
| 403 | 權限不足 |
| 404 | 商品不存在 |

#### 成功響應示例

```json
{
  "message": "已設定低庫存閾值為 10",
  "productId": "60d21b4967d0d8992e610c85",
  "variantId": null,
  "threshold": 10
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
| ALERT_NOT_FOUND | 預警不存在 |
| INVALID_THRESHOLD | 無效的閾值 |
```
