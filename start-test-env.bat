@echo off
echo ===== 啟動微服務電商平台測試環境 =====

echo 啟動測試環境...
docker-compose -f docker-compose.test.yml up -d

echo 等待服務啟動...
timeout /t 10 /nobreak > nul

echo ===== 服務狀態 =====
docker-compose -f docker-compose.test.yml ps

echo ===== 測試環境訪問信息 =====
echo API Gateway: http://localhost:6000
echo 認證服務: http://localhost:6001
echo 商品服務: http://localhost:6002
echo 支付服務: http://localhost:6003
echo 訂單服務: http://localhost:6004

echo ===== 測試環境已啟動 =====