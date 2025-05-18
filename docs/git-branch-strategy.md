# Git 分支策略

本文檔描述了微服務電商平台項目的 Git 分支管理策略。

## 分支結構

我們採用簡化版的 GitFlow 工作流，主要包含以下幾種分支類型：

### 主要分支

- **main** - 主分支，包含穩定的、可發布的代碼
  - 該分支受保護，不允許直接提交
  - 只能通過合併請求(Pull Request)更新
  - 每次合併到 main 分支都應該是一個可部署的版本

- **develop** - 開發分支，包含最新的開發代碼
  - 所有功能開發完成後首先合併到此分支
  - 當 develop 分支達到穩定狀態時，可以合併到 main 分支

### 輔助分支

- **feature/** - 功能分支，用於開發新功能
  - 命名規則：`feature/服務名稱-功能描述`
  - 示例：`feature/auth-jwt-implementation`
  - 從 develop 分支創建
  - 完成後合併回 develop 分支

- **bugfix/** - 錯誤修復分支，用於修復非緊急錯誤
  - 命名規則：`bugfix/服務名稱-錯誤描述`
  - 示例：`bugfix/product-search-filter`
  - 從 develop 分支創建
  - 完成後合併回 develop 分支

- **hotfix/** - 熱修復分支，用於修復生產環境中的緊急錯誤
  - 命名規則：`hotfix/服務名稱-錯誤描述`
  - 示例：`hotfix/payment-calculation-error`
  - 從 main 分支創建
  - 完成後同時合併回 main 和 develop 分支

- **release/** - 發布分支，用於準備發布新版本
  - 命名規則：`release/版本號`
  - 示例：`release/1.0.0`
  - 從 develop 分支創建
  - 只進行錯誤修復和文檔更新
  - 完成後同時合併回 main 和 develop 分支

## 工作流程

### 開發新功能

1. 從 develop 分支創建新的功能分支：
   ```
   git checkout develop
   git pull
   git checkout -b feature/auth-jwt-implementation
   ```

2. 在功能分支上進行開發和提交：
   ```
   git add .
   git commit -m "實現 JWT 認證"
   ```

3. 定期從 develop 分支同步更新：
   ```
   git checkout develop
   git pull
   git checkout feature/auth-jwt-implementation
   git merge develop
   ```

4. 功能開發完成後，合併回 develop 分支：
   ```
   git checkout develop
   git merge feature/auth-jwt-implementation
   git push
   ```

5. 刪除功能分支（可選）：
   ```
   git branch -d feature/auth-jwt-implementation
   ```

### 修復錯誤

1. 從 develop 分支創建新的錯誤修復分支：
   ```
   git checkout develop
   git pull
   git checkout -b bugfix/product-search-filter
   ```

2. 修復錯誤並提交：
   ```
   git add .
   git commit -m "修復商品搜索過濾問題"
   ```

3. 合併回 develop 分支：
   ```
   git checkout develop
   git merge bugfix/product-search-filter
   git push
   ```

### 處理緊急修復

1. 從 main 分支創建熱修復分支：
   ```
   git checkout main
   git pull
   git checkout -b hotfix/payment-calculation-error
   ```

2. 修復錯誤並提交：
   ```
   git add .
   git commit -m "修復支付計算錯誤"
   ```

3. 合併回 main 分支：
   ```
   git checkout main
   git merge hotfix/payment-calculation-error
   git tag -a v1.0.1 -m "版本 1.0.1"
   git push --follow-tags
   ```

4. 同時合併到 develop 分支：
   ```
   git checkout develop
   git merge hotfix/payment-calculation-error
   git push
   ```

## 提交訊息規範

為了保持提交歷史的清晰和一致性，我們採用以下提交訊息格式：

```
<類型>(<範圍>): <描述>

[可選的正文]

[可選的頁腳]
```

### 類型

- **feat**: 新功能
- **fix**: 錯誤修復
- **docs**: 文檔更新
- **style**: 代碼格式調整（不影響代碼運行）
- **refactor**: 代碼重構（既不是新增功能，也不是修復錯誤）
- **perf**: 性能優化
- **test**: 測試相關
- **chore**: 構建過程或輔助工具的變動

### 範圍

指定提交影響的範圍，通常是服務名稱或模塊名稱，例如：auth, product, order, payment 等。

### 描述

簡短描述提交的變更，使用現在時態，不要加句號。

### 示例

```
feat(auth): 實現 JWT 認證機制

- 添加 JWT 生成和驗證功能
- 實現 token 刷新機制
- 添加身份驗證中間件

Closes #123
```

## 代碼審查

所有合併到 develop 和 main 分支的代碼都應該經過代碼審查。代碼審查的目的是：

- 確保代碼質量
- 發現潛在問題
- 知識共享
- 確保遵循項目規範

## 版本號管理

我們使用語義化版本號（Semantic Versioning）來管理版本：

- **主版本號**：當進行不兼容的 API 修改時增加
- **次版本號**：當增加功能但保持向後兼容性時增加
- **修訂號**：當進行向後兼容的錯誤修復時增加

示例：1.0.0, 1.1.0, 1.1.1