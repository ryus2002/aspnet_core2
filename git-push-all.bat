@echo off
echo ===================================================
echo Git - Commit and Push All Changes
echo ===================================================
echo.

:: 檢查是否在 Git 存儲庫中
git rev-parse --is-inside-work-tree >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: 當前目錄不是 Git 存儲庫!
    exit /b 1
)

:: 顯示當前狀態
echo 當前 Git 狀態:
git status --short
echo.

:: 添加所有文件
echo 添加所有更改...
git add .
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: 添加文件失敗!
    exit /b 1
)

:: 提示用戶輸入提交訊息
set /p COMMIT_MSG="請輸入提交訊息 (默認: 'Update'): "
if "%COMMIT_MSG%"=="" set COMMIT_MSG=Update

:: 提交更改
echo 提交更改...
git commit -m "%COMMIT_MSG%"
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: 提交更改失敗!
    exit /b 1
)

:: 獲取當前分支
for /f "tokens=*" %%a in ('git rev-parse --abbrev-ref HEAD') do set CURRENT_BRANCH=%%a
echo 當前分支: %CURRENT_BRANCH%

:: 詢問是否推送到遠程
set /p PUSH_CONFIRM="是否推送到遠程存儲庫? (Y/N, 默認: Y): "
if /i "%PUSH_CONFIRM%"=="N" (
    echo 跳過推送到遠程存儲庫。
    echo 提交已完成。
    exit /b 0
)

:: 推送到遠程
echo 推送到遠程存儲庫...
git push origin %CURRENT_BRANCH%
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: 推送到遠程存儲庫失敗!
    echo 可能需要先拉取最新更改或解決衝突。
    exit /b 1
)

echo.
echo ===================================================
echo 所有更改已成功提交並推送到遠程存儲庫!
echo ===================================================