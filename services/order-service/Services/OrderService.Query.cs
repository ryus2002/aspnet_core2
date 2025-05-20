using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using System.Text.Json;

namespace OrderService.Services
{
    /// <summary>
    /// 訂單服務實現 - 查詢功能
    /// </summary>
    public partial class OrderService : IOrderService
    {
        /// <summary>
        /// 獲取用戶的訂單列表
        /// </summary>
        public async Task<PagedResponse<OrderResponse>> GetUserOrdersAsync(
            string userId, 
            string? status = null, 
            int page = 1, 
            int pageSize = 10)
        {
            // 確保頁碼和每頁大小有效
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            // 構建查詢
            IQueryable<Order> query = _dbContext.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId);

            // 根據狀態過濾
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // 獲取總記錄數
            var totalCount = await query.CountAsync();

            // 分頁並排序
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 構建響應
            var orderResponses = new List<OrderResponse>();
            foreach (var order in orders)
            {
                var response = await GetOrderResponseAsync(order.Id);
                orderResponses.Add(response);
            }

            return new PagedResponse<OrderResponse>
            {
                Data = orderResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// 獲取訂單狀態歷史
        /// </summary>
        public async Task<List<OrderStatusHistoryResponse>> GetOrderStatusHistoryAsync(string id)
        {
            var history = await _dbContext.OrderStatusHistories
                .Where(h => h.OrderId == id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync();

            return history.Select(h => new OrderStatusHistoryResponse
            {
                Id = h.Id,
                Status = h.Status,
                Comment = h.Comment,
                ChangedBy = h.ChangedBy,
                ChangedAt = h.ChangedAt
            }).ToList();
        }

        /// <summary>
        /// 將訂單實體轉換為響應DTO
        /// </summary>
        private async Task<OrderResponse> GetOrderResponseAsync(string orderId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new KeyNotFoundException($"訂單不存在: {orderId}");
            }

            // 獲取地址
            Address? shippingAddress = null;
            Address? billingAddress = null;

            if (order.ShippingAddressId.HasValue)
            {
                shippingAddress = await _dbContext.Addresses.FindAsync(order.ShippingAddressId.Value);
            }

            if (order.BillingAddressId.HasValue)
            {
                billingAddress = await _dbContext.Addresses.FindAsync(order.BillingAddressId.Value);
            }

            // 獲取訂單狀態歷史
            var statusHistory = await GetOrderStatusHistoryAsync(orderId);

            // 構建響應
            var response = new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                ItemsCount = order.ItemsCount,
                ShippingAddress = shippingAddress != null ? MapAddressToDTO(shippingAddress) : null,
                BillingAddress = billingAddress != null ? MapAddressToDTO(billingAddress) : null,
                PaymentId = order.PaymentId,
                ShippingMethod = order.ShippingMethod,
                ShippingFee = order.ShippingFee,
                TaxAmount = order.TaxAmount,
                DiscountAmount = order.DiscountAmount,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                CompletedAt = order.CompletedAt,
                CancelledAt = order.CancelledAt,
                CancellationReason = order.CancellationReason,
                StatusHistory = statusHistory,
                Metadata = order.Metadata != null ? 
                    JsonSerializer.Deserialize<Dictionary<string, object>>(order.Metadata) : null,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    VariantId = i.VariantId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice,
                    Attributes = i.Attributes != null ? 
                        JsonSerializer.Deserialize<Dictionary<string, object>>(i.Attributes) : null,
                    SKU = i.SKU,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };

            return response;
        }

        /// <summary>
        /// 將地址實體映射為DTO
        /// </summary>
        private AddressDTO MapAddressToDTO(Address address)
        {
            return new AddressDTO
            {
                Name = address.Name,
                Phone = address.Phone,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                AddressType = address.AddressType,
                SaveAsDefault = address.IsDefault
            };
        }
    }
}