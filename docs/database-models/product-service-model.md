# 商品服務資料模型設計

## 概述

商品服務負責管理商品資訊、分類和庫存。根據系統架構設計和技術選型，商品服務使用 MongoDB 作為資料庫，利用其靈活的文檔模型來存儲商品相關資料。

## 資料庫選擇

**MongoDB 6.0**

選擇理由：
- 靈活的文檔模型適合存儲多變的商品屬性
- 良好的查詢性能支持商品搜尋功能
- 易於擴展，適合處理大量商品數據
- 支持地理空間索引，可用於未來的位置相關功能

## 集合設計

### 1. 商品集合 (products)

存儲商品的基本資訊和詳細屬性。

```javascript
{
  "_id": ObjectId,                // 商品唯一標識符
  "name": String,                 // 商品名稱
  "description": String,          // 商品描述
  "price": {
    "regular": Number,            // 正常價格
    "discount": Number,           // 折扣價格
    "currency": String            // 貨幣單位 (預設: TWD)
  },
  "images": [                     // 商品圖片
    {
      "url": String,              // 圖片URL
      "alt": String,              // 替代文字
      "isMain": Boolean           // 是否為主圖
    }
  ],
  "categoryId": ObjectId,         // 分類ID，關聯到categories集合
  "attributes": {                 // 商品屬性，靈活的鍵值對
    "color": String,
    "size": String,
    "weight": Number,
    "material": String,
    // 其他可變屬性...
  },
  "variants": [                   // 商品變體
    {
      "variantId": String,        // 變體ID
      "attributes": {             // 變體特定屬性
        "color": String,
        "size": String
      },
      "price": Number,            // 變體價格
      "sku": String,              // 庫存單位
      "stock": Number             // 庫存數量
    }
  ],
  "stock": {                      // 庫存資訊
    "quantity": Number,           // 庫存數量
    "reserved": Number,           // 已預留數量
    "available": Number,          // 可用數量
    "lowStockThreshold": Number   // 低庫存閾值
  },
  "status": String,               // 商品狀態 (active, inactive, outOfStock)
  "tags": [String],               // 標籤
  "ratings": {                    // 評分統計
    "average": Number,            // 平均評分
    "count": Number               // 評分數量
  },
  "createdAt": Date,              // 創建時間
  "updatedAt": Date,              // 更新時間
  "metadata": {                   // 元數據
    "searchKeywords": [String],   // 搜尋關鍵字
    "seoTitle": String,           // SEO標題
    "seoDescription": String,     // SEO描述
  }
}
```

### 2. 商品分類集合 (categories)

存儲商品分類的層次結構。

```javascript
{
  "_id": ObjectId,                // 分類唯一標識符
  "name": String,                 // 分類名稱
  "description": String,          // 分類描述
  "slug": String,                 // URL友好的標識符
  "parentId": ObjectId,           // 父分類ID (頂級分類為null)
  "level": Number,                // 分類層級 (0為頂級)
  "path": [ObjectId],             // 從頂級到當前分類的路徑
  "isActive": Boolean,            // 是否啟用
  "image": {                      // 分類圖片
    "url": String,
    "alt": String
  },
  "createdAt": Date,              // 創建時間
  "updatedAt": Date               // 更新時間
}
```

### 3. 庫存變動記錄集合 (inventory_changes)

記錄商品庫存的變動歷史。

```javascript
{
  "_id": ObjectId,                // 記錄唯一標識符
  "productId": ObjectId,          // 商品ID
  "variantId": String,            // 變體ID (如果適用)
  "type": String,                 // 變動類型 (increment, decrement, adjustment)
  "quantity": Number,             // 變動數量
  "reason": String,               // 變動原因 (order, return, adjustment, etc.)
  "referenceId": String,          // 相關單據ID (如訂單ID)
  "previousQuantity": Number,     // 變動前數量
  "newQuantity": Number,          // 變動後數量
  "timestamp": Date,              // 變動時間
  "userId": String                // 操作用戶ID
}
```

### 4. 商品預留集合 (reservations)

記錄暫時預留的商品庫存。

```javascript
{
  "_id": ObjectId,                // 預留唯一標識符
  "productId": ObjectId,          // 商品ID
  "variantId": String,            // 變體ID (如果適用)
  "quantity": Number,             // 預留數量
  "sessionId": String,            // 會話ID
  "userId": String,               // 用戶ID (可選)
  "expiresAt": Date,              // 預留過期時間
  "status": String,               // 預留狀態 (active, used, expired, cancelled)
  "createdAt": Date               // 創建時間
}
```

## 索引設計

為提高查詢效能，建立以下索引：

1. 商品集合 (products):
   - `name`: 文字索引，支持商品名稱搜尋
   - `categoryId`: 支持按分類查詢商品
   - `tags`: 支持按標籤查詢商品
   - `"stock.quantity"`: 支持庫存查詢
   - `status`: 支持按狀態過濾商品
   - 複合索引 `{name: 1, price.regular: 1}`: 支持名稱和價格排序

2. 商品分類集合 (categories):
   - `parentId`: 支持查詢子分類
   - `slug`: 唯一索引，支持URL友好的查詢
   - `path`: 支持層次結構查詢

3. 庫存變動記錄集合 (inventory_changes):
   - `productId`: 支持查詢特定商品的庫存變動
   - `timestamp`: 支持時間範圍查詢

4. 商品預留集合 (reservations):
   - `productId`: 支持查詢特定商品的預留
   - `expiresAt`: 支持查詢過期預留
   - `sessionId`: 支持按會話查詢預留

## 數據驗證與約束

MongoDB 6.0 支持 JSON Schema 驗證，將為每個集合定義驗證規則：

1. 商品集合:
   - `name`: 必填，長度限制
   - `price.regular`: 必填，大於0
   - `categoryId`: 必填，有效的ObjectId
   - `stock.quantity`: 必填，非負整數

2. 商品分類集合:
   - `name`: 必填，唯一
   - `slug`: 必填，唯一，符合URL格式

## 數據遷移策略

1. 初始化遷移:
   - 創建基本分類結構
   - 導入示例商品數據

2. 版本管理:
   - 使用語義化版本控制追蹤模式變更
   - 每次模式變更創建新的遷移腳本

## 本地開發配置

為本地開發環境配置MongoDB:

```yaml
# docker-compose.yml 片段
mongodb:
  image: mongo:6.0
  ports:
    - "27017:27017"
  environment:
    MONGO_INITDB_ROOT_USERNAME: root
    MONGO_INITDB_ROOT_PASSWORD: example
  volumes:
    - mongodb_data:/data/db
    - ./init-mongo.js:/docker-entrypoint-initdb.d/init-mongo.js:ro
```

初始化腳本 (init-mongo.js):
```javascript
db = db.getSiblingDB('product_service');
db.createUser({
  user: 'product_user',
  pwd: 'product_password',
  roles: [{ role: 'readWrite', db: 'product_service' }]
});

// 創建集合並設置驗證規則
db.createCollection('products', {
  validator: {
    $jsonSchema: {
      bsonType: "object",
      required: ["name", "price", "categoryId", "stock"],
      // 詳細驗證規則...
    }
  }
});

// 創建索引
db.products.createIndex({ name: "text" });
db.products.createIndex({ categoryId: 1 });
// 其他索引...
```