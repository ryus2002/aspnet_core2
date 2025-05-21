using Microsoft.Extensions.Logging;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Messaging.Publishers;

namespace ProductService.Services
{
    /// <summary>
    /// 庫存服務的預留相關功能
    /// </summary>
    public partial class InventoryService : IInventoryService
    {
        /// <summary>
        /// 創建商品預留
        /// </summary>
        /// <param name="request">預留請求</param>
        /// <returns>預留</returns>
        public async Task<Reservation> CreateReservationAsync(CreateReservationRequest request)
        {
            _logger.LogInformation("創建商品預留: OwnerId={OwnerId}, OwnerType={OwnerType}, Items={ItemCount}",
                request.OwnerId, request.OwnerType, request.Items.Count);

            // 檢查預留項目
            if (request.Items == null || !request.Items.Any())
            {
                throw new BadRequestException("預留項目不能為空");
            }

            // 檢查每個商品的庫存是否足夠
            foreach (var item in request.Items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new NotFoundException($"找不到商品: {item.ProductId}");
                }

                int availableQuantity;
                if (item.VariantId != null)
                {
                    var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId);
                    if (variant == null)
                    {
                        throw new NotFoundException($"找不到商品變體: {item.VariantId}");
                    }
                    availableQuantity = variant.Stock.Quantity;
                }
                else
                {
                    availableQuantity = product.Stock.Quantity;
                }

                if (availableQuantity < item.Quantity)
                {
                    throw new BadRequestException($"商品 {product.Name} 庫存不足: 需要 {item.Quantity}, 可用 {availableQuantity}");
                }
            }

            // 創建預留
            var reservation = new Reservation
            {
                Id = Guid.NewGuid().ToString(),
                OwnerId = request.OwnerId,
                OwnerType = request.OwnerType,
                Status = "Active",
                ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpirationMinutes > 0 ? request.ExpirationMinutes : 30),
                CreatedAt = DateTime.UtcNow,
                Items = new List<ReservationItem>()
            };

            // 添加預留項目並更新庫存
            foreach (var item in request.Items)
            {
                reservation.Items.Add(new ReservationItem
                {
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity
                });

                // 創建庫存變動記錄（減少庫存）
                await CreateInventoryChangeAsync(
                    productId: item.ProductId,
                    variantId: item.VariantId,
                    type: "Reserved",
                    quantity: -item.Quantity, // 負數表示減少庫存
                    reason: $"商品預留: {reservation.Id}",
                    referenceId: reservation.Id,
                    userId: null);
            }

            // 保存預留
            await _dbContext.Reservations.InsertOneAsync(reservation);

            _logger.LogInformation("商品預留已創建: Id={Id}, OwnerId={OwnerId}, ExpiresAt={ExpiresAt}",
                reservation.Id, request.OwnerId, reservation.ExpiresAt);

            // 發布庫存預留事件
            await _inventoryEventPublisher.PublishInventoryReservedEventAsync(reservation);

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
            _logger.LogInformation("確認商品預留: Id={Id}, ReferenceId={ReferenceId}", id, referenceId);

            // 獲取預留
            var reservation = await _dbContext.Reservations.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (reservation == null)
            {
                throw new NotFoundException($"找不到預留: {id}");
            }

            // 檢查預留狀態
            if (reservation.Status != "Active")
            {
                throw new BadRequestException($"預留狀態無效: {reservation.Status}");
            }

            // 檢查預留是否過期
            if (reservation.ExpiresAt < DateTime.UtcNow)
            {
                throw new BadRequestException("預留已過期");
            }

            // 更新預留狀態
            reservation.Status = "Confirmed";
            reservation.ReferenceId = referenceId;
            reservation.UpdatedAt = DateTime.UtcNow;

            // 保存預留
            await _dbContext.Reservations.ReplaceOneAsync(r => r.Id == id, reservation);

            _logger.LogInformation("商品預留已確認: Id={Id}, ReferenceId={ReferenceId}", id, referenceId);

            return true;
        }

        /// <summary>
        /// 取消預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>是否成功</returns>
        public async Task<bool> CancelReservationAsync(string id)
        {
            _logger.LogInformation("取消商品預留: Id={Id}", id);

            // 獲取預留
            var reservation = await _dbContext.Reservations.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (reservation == null)
            {
                throw new NotFoundException($"找不到預留: {id}");
            }

            // 檢查預留狀態
            if (reservation.Status != "Active")
            {
                throw new BadRequestException($"預留狀態無效: {reservation.Status}");
            }

            // 更新預留狀態
            reservation.Status = "Cancelled";
            reservation.UpdatedAt = DateTime.UtcNow;

            // 保存預留
            await _dbContext.Reservations.ReplaceOneAsync(r => r.Id == id, reservation);

            // 恢復庫存
            foreach (var item in reservation.Items)
            {
                // 創建庫存變動記錄（增加庫存）
                await CreateInventoryChangeAsync(
                    productId: item.ProductId,
                    variantId: item.VariantId,
                    type: "ReservationCancelled",
                    quantity: item.Quantity, // 正數表示增加庫存
                    reason: $"預留取消: {id}",
                    referenceId: id,
                    userId: null);
            }

            _logger.LogInformation("商品預留已取消: Id={Id}", id);

            return true;
        }

        /// <summary>
        /// 清理過期預留
        /// </summary>
        /// <returns>清理的預留數量</returns>
        public async Task<int> CleanupExpiredReservationsAsync()
        {
            _logger.LogInformation("開始清理過期預留");

            // 獲取過期的預留
            var expiredReservations = await _dbContext.Reservations
                .Find(r => r.Status == "Active" && r.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (!expiredReservations.Any())
            {
                _logger.LogInformation("沒有過期的預留需要清理");
                return 0;
            }

            _logger.LogInformation("找到 {Count} 個過期預留", expiredReservations.Count);

            // 取消每個過期預留
            foreach (var reservation in expiredReservations)
            {
                // 更新預留狀態
                reservation.Status = "Expired";
                reservation.UpdatedAt = DateTime.UtcNow;

                // 保存預留
                await _dbContext.Reservations.ReplaceOneAsync(r => r.Id == reservation.Id, reservation);

                // 恢復庫存
                foreach (var item in reservation.Items)
                {
                    // 創建庫存變動記錄（增加庫存）
                    await CreateInventoryChangeAsync(
                        productId: item.ProductId,
                        variantId: item.VariantId,
                        type: "ReservationExpired",
                        quantity: item.Quantity, // 正數表示增加庫存
                        reason: $"預留過期: {reservation.Id}",
                        referenceId: reservation.Id,
                        userId: null);
                }

                _logger.LogInformation("過期預留已處理: Id={Id}", reservation.Id);
            }

            _logger.LogInformation("過期預留清理完成: 共處理 {Count} 個預留", expiredReservations.Count);

            return expiredReservations.Count;
        }

        /// <summary>
        /// 獲取會話的預留
        /// </summary>
        /// <param name="sessionId">會話ID</param>
        /// <returns>預留列表</returns>
        public async Task<List<Reservation>> GetReservationsBySessionAsync(string sessionId)
        {
            return await _dbContext.Reservations
                .Find(r => r.OwnerId == sessionId && r.OwnerType == "Session")
                .ToListAsync();
        }

        /// <summary>
        /// 獲取用戶的預留
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>預留列表</returns>
        public async Task<List<Reservation>> GetReservationsByUserAsync(string userId)
        {
            return await _dbContext.Reservations
                .Find(r => r.OwnerId == userId && r.OwnerType == "User")
                .ToListAsync();
        }
    }
}