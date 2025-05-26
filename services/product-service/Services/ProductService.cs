        public async Task<Product> UpdateProductStockAsync(string id, UpdateStockRequest request)
        {
            _logger.LogInformation("更新商品庫存: ID={Id}, 數量={Quantity}", id, request.Quantity);

            // 獲取商品
            var product = await GetProductByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"商品不存在: {id}");
            }

            // 驗證請求
            if (request.Quantity < 0)
            {
                throw new ArgumentException("庫存數量不能為負數", nameof(request.Quantity));
            }

            int previousQuantity;
            string type;
            string reason = request.Reason ?? "manual";

            if (string.IsNullOrWhiteSpace(request.VariantId))
            {
                // 更新主商品庫存
                previousQuantity = product.Stock.Quantity;
                
                // 確定庫存變動類型
                if (request.Quantity > previousQuantity)
                {
                    type = "increment";
                }
                else if (request.Quantity < previousQuantity)
                {
                    type = "decrement";
                }
                else
                {
                    // 庫存沒有變化
                    return product;
                }

                // 計算可用庫存
                int available = request.Quantity - product.Stock.Reserved;
                if (available < 0)
                {
                    throw new InvalidOperationException($"新庫存數量不能小於已預留數量: 預留={product.Stock.Reserved}, 請求={request.Quantity}");
                }

                // 更新庫存
                var update = Builders<Product>.Update
                    .Set(p => p.Stock.Quantity, request.Quantity)
                    .Set(p => p.Stock.Available, available)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                if (request.LowStockThreshold.HasValue)
                {
                    update = update.Set(p => p.Stock.LowStockThreshold, request.LowStockThreshold.Value);
                }

                var result = await _dbContext.Products
                    .UpdateOneAsync(p => p.Id == id, update);

                if (result.ModifiedCount == 0)
                {
                    throw new InvalidOperationException($"更新商品庫存失敗: {id}");
                }
            }
            else
            {
                // 更新變體庫存
                var variant = product.Variants?.FirstOrDefault(v => v.VariantId == request.VariantId);
                if (variant == null)
                {
                    throw new KeyNotFoundException($"商品變體不存在: {request.VariantId}");
                }

                // 修正：從變體的 StockInfo 中獲取 Quantity
                previousQuantity = variant.Stock.Quantity;
                
                // 確定庫存變動類型
                if (request.Quantity > previousQuantity)
                {
                    type = "increment";
                }
                else if (request.Quantity < previousQuantity)
                {
                    type = "decrement";
                }
                else
                {
                    // 庫存沒有變化
                    return product;
                }

                // 更新變體庫存
                // 修正：使用正確的路徑來更新變體的 StockInfo.Quantity
                var arrayFilters = new List<ArrayFilterDefinition<BsonValue>> 
                { 
                    new BsonDocumentArrayFilterDefinition<BsonValue>(
                        new BsonDocument("elem.variantId", request.VariantId)
                    ) 
                };

                var update = Builders<Product>.Update
                    .Set("Variants.$[elem].Stock.Quantity", request.Quantity)
                    .Set("Variants.$[elem].Stock.Available", request.Quantity) // 變體沒有預留概念，所以可用量等於總量
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                var result = await _dbContext.Products
                    .UpdateOneAsync(
                        p => p.Id == id && p.Variants.Any(v => v.VariantId == request.VariantId),
                        update,
                        new UpdateOptions { ArrayFilters = arrayFilters });

                if (result.ModifiedCount == 0)
                {
                    throw new InvalidOperationException($"更新商品變體庫存失敗: {id}, 變體ID: {request.VariantId}");
                }
            }

            // 創建庫存變動記錄
    var inventoryChange = await _inventoryService.CreateInventoryChangeAsync(
                id,
                request.VariantId,
                type,
                Math.Abs(request.Quantity - previousQuantity),
                reason,
                null,
        request.UserId
    );

            // 返回更新後的商品
            return await GetProductByIdAsync(id) ?? throw new InvalidOperationException($"無法獲取更新後的商品: {id}");
        }
