# 認證服務 API 文檔

本文檔描述認證服務(Auth Service)的API端點。

## API 端點

### 用戶註冊

註冊新用戶。

```
POST /api/auth/register
```

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| username | string | 是 | 用戶名，長度3-20字符 |
| email | string | 是 | 電子郵件地址 |
| password | string | 是 | 密碼，長度最少8字符 |
| fullName | string | 是 | 用戶全名 |
| phoneNumber | string | 否 | 電話號碼 |

#### 請求示例

```json
{
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "securePassword123",
  "fullName": "John Doe",
  "phoneNumber": "0912345678"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 201 | 用戶創建成功 |
| 400 | 請求參數無效 |
| 409 | 用戶名或電子郵件已存在 |

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c85",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "createdAt": "2023-05-20T10:30:00Z"
}
```

#### 錯誤響應示例

```json
{
  "error": {
    "code": "USER_EXISTS",
    "message": "用戶名或電子郵件已存在"
  }
}
```

### 用戶登入

用戶登入並獲取訪問令牌。

```
POST /api/auth/login
```

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| username | string | 是 | 用戶名或電子郵件 |
| password | string | 是 | 密碼 |

#### 請求示例

```json
{
  "username": "johndoe",
  "password": "securePassword123"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 登入成功 |
| 400 | 請求參數無效 |
| 401 | 認證失敗 |

#### 成功響應示例

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer",
  "user": {
    "id": "60d21b4967d0d8992e610c85",
    "username": "johndoe",
    "fullName": "John Doe",
    "roles": ["user"]
  }
}
```

#### 錯誤響應示例

```json
{
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "用戶名或密碼不正確"
  }
}
```

### 刷新令牌

使用刷新令牌獲取新的訪問令牌。

```
POST /api/auth/refresh-token
```

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| refreshToken | string | 是 | 刷新令牌 |

#### 請求示例

```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 令牌刷新成功 |
| 400 | 請求參數無效 |
| 401 | 刷新令牌無效或已過期 |

#### 成功響應示例

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

### 登出

使當前令牌失效。

```
POST /api/auth/logout
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 登出成功 |
| 401 | 未認證 |

#### 成功響應示例

```json
{
  "message": "登出成功"
}
```

### 獲取當前用戶資料

獲取當前認證用戶的詳細資料。

```
GET /api/auth/me
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

#### 成功響應示例

```json
{
  "id": "60d21b4967d0d8992e610c85",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "phoneNumber": "0912345678",
  "roles": ["user"],
  "createdAt": "2023-05-20T10:30:00Z",
  "updatedAt": "2023-05-20T10:30:00Z"
}
```

### 修改密碼

修改當前用戶的密碼。

```
PUT /api/auth/change-password
```

#### 請求頭

| 參數名 | 必填 | 描述 |
|--------|------|------|
| Authorization | 是 | Bearer {accessToken} |

#### 請求參數

| 參數名 | 類型 | 必填 | 描述 |
|--------|------|------|------|
| currentPassword | string | 是 | 當前密碼 |
| newPassword | string | 是 | 新密碼，長度最少8字符 |

#### 請求示例

```json
{
  "currentPassword": "securePassword123",
  "newPassword": "newSecurePassword456"
}
```

#### 響應

| 狀態碼 | 描述 |
|--------|------|
| 200 | 密碼修改成功 |
| 400 | 請求參數無效 |
| 401 | 當前密碼不正確 |

#### 成功響應示例

```json
{
  "message": "密碼修改成功"
}
```

## 錯誤碼

| 錯誤碼 | 描述 |
|--------|------|
| USER_EXISTS | 用戶名或電子郵件已存在 |
| INVALID_CREDENTIALS | 用戶名或密碼不正確 |
| TOKEN_EXPIRED | 令牌已過期 |
| INVALID_TOKEN | 令牌無效 |
| PERMISSION_DENIED | 權限不足 |