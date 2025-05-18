#!/bin/bash

# 顯示彩色輸出
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}===== 停止微服務電商平台開發環境 =====${NC}"

# 停止所有服務
echo -e "${GREEN}停止所有服務...${NC}"
docker-compose down

echo -e "${YELLOW}===== 開發環境已停止 =====${NC}"