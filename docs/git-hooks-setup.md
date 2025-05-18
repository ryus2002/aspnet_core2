# Git Hooks 設置指南

本專案使用 Git hooks 來幫助維護代碼質量和提交規範。以下是關於如何設置和使用這些 hooks 的說明。

## 已配置的 Git Hooks

我們已經配置了以下 Git hooks：

1. **pre-commit**: 在提交前運行，檢查代碼格式和語法錯誤
2. **commit-msg**: 在創建提交訊息後運行，檢查提交訊息是否符合規範

## Windows 環境設置

在 Windows 環境中，Git hooks 文件應該沒有副檔名。如果您在 Windows 環境下克隆了此倉庫，請確保 hooks 文件沒有副檔名。

## Linux/Mac 環境設置

在 Linux/Mac 環境中，Git hooks 文件需要有執行權限。如果您在 Linux/Mac 環境下克隆了此倉庫，請運行以下命令為 hooks 文件添加執行權限：

```bash
chmod +x .git/hooks/pre-commit .git/hooks/commit-msg
```

## 自動設置 Git Hooks

我們提供了一個腳本來自動設置 Git hooks。請運行以下命令：

```bash
# Windows
.\scripts\setup-git-hooks.bat

# Linux/Mac
./scripts/setup-git-hooks.sh
```

## 手動設置 Git Hooks

如果您需要手動設置 Git hooks，請按照以下步驟操作：

1. 將 `docs/git-hooks` 目錄中的文件複製到 `.git/hooks` 目錄：

   ```bash
   # Windows
   copy docs\git-hooks\* .git\hooks\

   # Linux/Mac
   cp docs/git-hooks/* .git/hooks/
   ```

2. 在 Linux/Mac 環境中，還需要為 hooks 文件添加執行權限：

   ```bash
   chmod +x .git/hooks/pre-commit .git/hooks/commit-msg
   ```

## 提交規範

我們的提交訊息必須遵循以下格式：
<類型>(<範圍>): <描述>
其中：

- **類型**：必須是以下之一：feat, fix, docs, style, refactor, perf, test, chore
- **範圍**：可選，表示提交影響的範圍，通常是服務名稱或模塊名稱
- **描述**：簡短描述提交的變更，使用現在時態，不要加句號

示例：
- `feat(auth): 實現 JWT 認證機制`
- `fix(product): 修復商品搜索過濾問題`
- `docs: 更新 API 文檔`

## 故障排除

如果您遇到 Git hooks 相關的問題，可以嘗試以下解決方法：

1. 確保 hooks 文件具有正確的格式（沒有 Windows 的 CRLF 行尾）
2. 確保 hooks 文件具有執行權限（Linux/Mac）
3. 臨時跳過 hooks 檢查（不推薦）：`git commit --no-verify -m "提交訊息"`