using MongoDB.Driver;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;
using MongoDB.Bson;

namespace ProductService.Services
{
    /// <summary>
    /// 庫存服務實現
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IProductDbContext _dbContext;
        private readonly ILogger<InventoryService> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="dbContext">數據庫上下文</param>
        /// <param name="logger">日誌記錄器</param>
        public InventoryService(
            IProductDbContext dbContext,
            ILogger<InventoryService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 創建庫存變動記錄
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <param name="type">變動類型</param>
        /// <param name="quantity">變動數量</param>
        /// <param name="reason">變動原因</param>
        /// <param name="referenceId">相關單據ID</param>
        /// <param name="userId">操作用戶ID</param>
        /// <returns>庫存變動記錄</returns>
        public async Task<InventoryChange> CreateInventoryChangeAsync(
            string productId,
            string? variantId,
            string type,
            int quantity,
            string reason,
            string? referenceId = null,
            string? userId = null)
        {
            _logger.LogInformation("創建庫存變動記錄: 商品ID={ProductId}, 變體ID={VariantId}, 類型={Type}, 數量={Quantity}, 原因={Reason}",
                productId, variantId ?? "無", type, quantity, reason);

            // 獲取當前庫存
            int previousQuantity;
            int newQuantity;

            if (string.IsNullOrWhiteSpace(variantId))
            {
                // 主商品庫存
                var product = await _dbContext.Products
                    .Find(p => p.Id == productId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    throw new KeyNotFoundException($"商品不存在: {productId}");
                }

                previousQuantity = product.Stock.Quantity;

                // 計算新庫存
                if (type == "increment")
                {
                    newQuantity = previousQuantity + quantity;
                }
                else if (type == "decrement")
                {
                    newQuantity = previousQuantity - quantity;
                    if (newQuantity < 0)
                    {
                        throw new InvalidOperationException($"庫存不足: 當前={previousQuantity}, 請求={quantity}");
                    }
                }
                else if (type == "adjustment")
                {
                    newQuantity = quantity;
                }
                else
                {
                    throw new ArgumentException($"無效的變動類型: {type}", nameof(type));
                }
            }
            else
            {
                // 變體庫存
                var product = await _dbContext.Products
                    .Find(p => p.Id == productId && p.Variants.Any(v => v.VariantId == variantId))
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    throw new KeyNotFoundException($"商品不存在: {productId}");
                }

                var variant = product.Variants.FirstOrDefault(v => v.VariantId == variantId);
                if (variant == null)
                {
                    throw new KeyNotFoundException($"商品變體不存在: {variantId}");
                }

                previousQuantity = variant.Stock;

                // 計算新庫存
                if (type == "increment")
                {
                    newQuantity = previousQuantity + quantity;
                }
                else if (type == "decrement")
                {
                    newQuantity = previousQuantity - quantity;
                    if (newQuantity < 0)
                    {
                        throw new InvalidOperationException($"變體庫存不足: 當前={previousQuantity}, 請求={quantity}");
                    }
                }
                else if (type == "adjustment")
                {
                    newQuantity = quantity;
                }
                else
                {
                    throw new ArgumentException($"無效的變動類型: {type}", nameof(type));
                }
            }

            // 創建庫存變動記錄
            var inventoryChange = new InventoryChange
            {
                ProductId = productId,
                VariantId = variantId,
                Type = type,
                Quantity = quantity,
                Reason = reason,
                ReferenceId = referenceId,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            await _dbContext.InventoryChanges.InsertOneAsync(inventoryChange);

            return inventoryChange;
        }

        /// <summary>
        /// 獲取商品庫存變動記錄
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>分頁庫存變動記錄</returns>
        public async Task<PagedResponse<InventoryChange>> GetInventoryChangesAsync(
            string productId,
            string? variantId = null,
            int page = 1,
            int pageSize = 10)
        {
            _logger.LogInformation("獲取庫存變動記錄: 商品ID={ProductId}, 變體ID={VariantId}, 頁碼={Page}, 每頁大小={PageSize}",
                productId, variantId ?? "無", page, pageSize);

            // 構建過濾條件
            var filterBuilder = Builders<InventoryChange>.Filter;
            var filter = filterBuilder.Eq(i => i.ProductId, productId);

            if (!string.IsNullOrWhiteSpace(variantId))
            {
                filter = filter & filterBuilder.Eq(i => i.VariantId, variantId);
            }

            // 計算總數
            var totalItems = await _dbContext.InventoryChanges
                .CountDocumentsAsync(filter);

            // 獲取數據
            var inventoryChanges = await _dbContext.InventoryChanges
                .Find(filter)
                .Sort(Builders<InventoryChange>.Sort.Descending(i => i.Timestamp))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // 計算總頁數
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // 返回分頁結果
            return new PagedResponse<InventoryChange>
            {
                Items = inventoryChanges,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// 創建商品預留
        /// </summary>
        /// <param name="request">預留請求</param>
        /// <returns>預留</returns>
        public async Task<Reservation> CreateReservationAsync(CreateReservationRequest request)
        {
            _logger.LogInformation("創建商品預留: 商品ID={ProductId}, 變體ID={VariantId}, 數量={Quantity}, 會話ID={SessionId}",
                request.ProductId, request.VariantId ?? "無", request.Quantity, request.SessionId);

            // 驗證請求
            if (string.IsNullOrWhiteSpace(request.ProductId))
            {
                throw new ArgumentException("商品ID不能為空", nameof(request.ProductId));
            }

            if (request.Quantity <= 0)
            {
                throw new ArgumentException("預留數量必須大於0", nameof(request.Quantity));
            }

            if (string.IsNullOrWhiteSpace(request.SessionId))
            {
                throw new ArgumentException("會話ID不能為空", nameof(request.SessionId));
            }

            // 檢查庫存是否足夠
            if (string.IsNullOrWhiteSpace(request.VariantId))
            {
                // 主商品庫存
                var product = await _dbContext.Products
                    .Find(p => p.Id == request.ProductId)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    throw new KeyNotFoundException($"商品不存在: {request.ProductId}");
                }

                if (product.Stock.Available < request.Quantity)
                {
                    throw new InvalidOperationException($"庫存不足: 當前可用={product.Stock.Available}, 請求={request.Quantity}");
                }

                // 更新庫存預留數量
                var update = Builders<Product>.Update
                    .Inc(p => p.Stock.Reserved, request.Quantity)
                    .Inc(p => p.Stock.Available, -request.Quantity)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                var result = await _dbContext.Products
                    .UpdateOneAsync(p => p.Id == request.ProductId, update);

                if (result.ModifiedCount == 0)
                {
                    throw new InvalidOperationException($"更新商品庫存預留失敗: {request.ProductId}");
                }
            }
            else
            {
                // 變體庫存
                var product = await _dbContext.Products
                    .Find(p => p.Id == request.ProductId && p.Variants.Any(v => v.VariantId == request.VariantId))
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    throw new KeyNotFoundException($"商品不存在: {request.ProductId}");
                }

                var variant = product.Variants.FirstOrDefault(v => v.VariantId == request.VariantId);
                if (variant == null)
                {
                    throw new KeyNotFoundException($"商品變體不存在: {request.VariantId}");
                }

                if (variant.Stock < request.Quantity)
                {
                    throw new InvalidOperationException($"變體庫存不足: 當前={variant.Stock}, 請求={request.Quantity}");
                }

                // 更新變體庫存
                var arrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("elem.variantId", request.VariantId))
                };

                var variantUpdate = Builders<Product>.Update
                    .Inc("variants.$[elem].stock", -request.Quantity)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

                var result = await _dbContext.Products
                    .UpdateOneAsync(
                        p => p.Id == request.ProductId && p.Variants.Any(v => v.VariantId == request.VariantId),
                        variantUpdate,
                        updateOptions);

                if (result.ModifiedCount == 0)
                {
                    throw new InvalidOperationException($"更新商品變體庫存失敗: {request.ProductId}, 變體ID: {request.VariantId}");
                }
            }

            // 創建預留
            var reservation = new Reservation
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Quantity = request.Quantity,
                SessionId = request.SessionId,
                UserId = request.UserId,
                ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpirationMinutes ?? 30),
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.Reservations.InsertOneAsync(reservation);

            return reservation;
        }

        /// <summary>
        /// 確認預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <param name="referenceId">相關單據ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> ConfirmReservationAsync(string id, string referenceId)
        {
            _logger.LogInformation("確認預留: ID={Id}, 單據ID={ReferenceId}", id, referenceId);

            // 獲取預留
            var reservation = await _dbContext.Reservations
                .Find(r => r.Id == id && r.Status == "active")
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw new KeyNotFoundException($"預留不存在或已過期: {id}");
            }

            // 更新預留狀態
            var reservationUpdate = Builders<Reservation>.Update
                .Set(r => r.Status, "used")
                .Set(r => r.ReferenceId, referenceId);

            var result = await _dbContext.Reservations
                .UpdateOneAsync(r => r.Id == id && r.Status == "active", reservationUpdate);

            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException($"確認預留失敗: {id}");
            }

            // 創建庫存變動記錄
            await CreateInventoryChangeAsync(
                reservation.ProductId,
                reservation.VariantId,
                "decrement",
                reservation.Quantity,
                "order",
                referenceId,
                reservation.UserId);

            // 如果是主商品預留，需要更新庫存預留數量
            if (string.IsNullOrWhiteSpace(reservation.VariantId))
            {
                var productUpdate = Builders<Product>.Update
                    .Inc(p => p.Stock.Reserved, -reservation.Quantity)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                await _dbContext.Products
                    .UpdateOneAsync(p => p.Id == reservation.ProductId, productUpdate);
            }

            return true;
        }

        /// <summary>
        /// 取消預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> CancelReservationAsync(string id)
        {
            _logger.LogInformation("取消預留: ID={Id}", id);

            // 獲取預留
            var reservation = await _dbContext.Reservations
                .Find(r => r.Id == id && r.Status == "active")
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                throw new KeyNotFoundException($"預留不存在或已過期: {id}");
            }

            // 更新預留狀態
            var reservationUpdate = Builders<Reservation>.Update
                .Set(r => r.Status, "cancelled");

            var result = await _dbContext.Reservations
                .UpdateOneAsync(r => r.Id == id && r.Status == "active", reservationUpdate);

            if (result.ModifiedCount == 0)
            {
                throw new InvalidOperationException($"取消預留失敗: {id}");
            }

            // 恢復庫存
            if (string.IsNullOrWhiteSpace(reservation.VariantId))
            {
                // 主商品庫存
                var productUpdate = Builders<Product>.Update
                    .Inc(p => p.Stock.Reserved, -reservation.Quantity)
                    .Inc(p => p.Stock.Available, reservation.Quantity)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                await _dbContext.Products
                    .UpdateOneAsync(p => p.Id == reservation.ProductId, productUpdate);
            }
            else
            {
                // 變體庫存
                var arrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
                {
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(
                        new BsonDocument("elem.variantId", reservation.VariantId))
                };

                var variantUpdate = Builders<Product>.Update
                    .Inc("variants.$[elem].stock", reservation.Quantity)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

                await _dbContext.Products
                    .UpdateOneAsync(
                        p => p.Id == reservation.ProductId && p.Variants.Any(v => v.VariantId == reservation.VariantId),
                        variantUpdate,
                        updateOptions);
            }

            return true;
        }

        /// <summary>
        /// 清理過期預留
        /// </summary>
        /// <returns>清理的預留數量</returns>
        public async Task<int> CleanupExpiredReservationsAsync()
        {
            _logger.LogInformation("清理過期預留");

            // 獲取過期預留
            var expiredReservations = await _dbContext.Reservations
                .Find(r => r.Status == "active" && r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            int count = 0;
            foreach (var reservation in expiredReservations)
            {
                try
                {
                    // 更新預留狀態
                    var reservationUpdate = Builders<Reservation>.Update
                        .Set(r => r.Status, "expired");

                    var result = await _dbContext.Reservations
                        .UpdateOneAsync(r => r.Id == reservation.Id && r.Status == "active", reservationUpdate);

                    if (result.ModifiedCount > 0)
                    {
                        // 恢復庫存
                        if (string.IsNullOrWhiteSpace(reservation.VariantId))
                        {
                            // 主商品庫存
                            var productUpdate = Builders<Product>.Update
                                .Inc(p => p.Stock.Reserved, -reservation.Quantity)
                                .Inc(p => p.Stock.Available, reservation.Quantity)
                                .Set(p => p.UpdatedAt, DateTime.UtcNow);

                            await _dbContext.Products
                                .UpdateOneAsync(p => p.Id == reservation.ProductId, productUpdate);
                        }
                        else
                        {
                            // 變體庫存
                            var arrayFilters = new List<ArrayFilterDefinition<BsonDocument>>
                            {
                                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                                    new BsonDocument("elem.variantId", reservation.VariantId))
                            };

                            var variantUpdate = Builders<Product>.Update
                                .Inc("variants.$[elem].stock", reservation.Quantity)
                                .Set(p => p.UpdatedAt, DateTime.UtcNow);

                            var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

                            await _dbContext.Products
                                .UpdateOneAsync(
                                    p => p.Id == reservation.ProductId && p.Variants.Any(v => v.VariantId == reservation.VariantId),
                                    variantUpdate,
                                    updateOptions);
                        }

                        count++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "清理過期預留失敗: ID={Id}", reservation.Id);
                }
            }

            _logger.LogInformation("清理過期預留完成: 數量={Count}", count);

            return count;
        }

        /// <summary>
        /// 獲取會話的預留
        /// </summary>
        /// <param name="sessionId">會話ID</param>
        /// <returns>預留列表</returns>
        public async Task<List<Reservation>> GetReservationsBySessionAsync(string sessionId)
        {
            _logger.LogInformation("獲取會話的預留: 會話ID={SessionId}", sessionId);

            return await _dbContext.Reservations
                .Find(r => r.SessionId == sessionId && r.Status == "active")
                .ToListAsync();
        }

        /// <summary>
        /// 獲取用戶的預留
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>預留列表</returns>
        public async Task<List<Reservation>> GetReservationsByUserAsync(string userId)
        {
            _logger.LogInformation("獲取用戶的預留: 用戶ID={UserId}", userId);

            return await _dbContext.Reservations
                .Find(r => r.UserId == userId && r.Status == "active")
                .ToListAsync();
        }
    }
}