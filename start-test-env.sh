#!/bin/bash

# 顯示彩色輸出
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}===== 啟動微服務電商平台測試環境 =====${NC}"

# 啟動測試環境
echo -e "${GREEN}啟動測試環境...${NC}"
docker-compose -f docker-compose.test.yml up -d

# 等待服務啟動
echo -e "${GREEN}等待服務啟動...${NC}"
sleep 10

# 檢查服務狀態
echo -e "${YELLOW}===== 服務狀態 =====${NC}"
docker-compose -f docker-compose.test.yml ps

# 顯示訪問信息
echo -e "${YELLOW}===== 測試環境訪問信息 =====${NC}"
echo -e "${GREEN}API Gateway:${NC} http://localhost:6000"
echo -e "${GREEN}認證服務:${NC} http://localhost:6001"
echo -e "${GREEN}商品服務:${NC} http://localhost:6002"
echo -e "${GREEN}支付服務:${NC} http://localhost:6003"
echo -e "${GREEN}訂單服務:${NC} http://localhost:6004"

echo -e "${YELLOW}===== 測試環境已啟動 =====${NC}"