using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Controllers
{
    /// <summary>
    /// 庫存預警控制器
    /// </summary>
    [ApiController]
    [Route("api/inventory/alerts")]
    public class InventoryAlertController : ControllerBase
    {
        private readonly IInventoryAlertService _inventoryAlertService;
        private readonly ILogger<InventoryAlertController> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="inventoryAlertService">庫存預警服務</param>
        /// <param name="logger">日誌記錄器</param>
        public InventoryAlertController(IInventoryAlertService inventoryAlertService, ILogger<InventoryAlertController> logger)
        {
            _inventoryAlertService = inventoryAlertService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取所有未解決的預警
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>預警列表</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<InventoryAlertResponse>>> GetActiveAlerts(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var alertsPage = await _inventoryAlertService.GetActiveAlertsAsync(page, pageSize);
                
                var response = new PagedResponse<InventoryAlertResponse>
                {
                    Items = alertsPage.Items.Select(MapToResponse).ToList(),
                    Page = alertsPage.Page,
                    PageSize = alertsPage.PageSize,
                    TotalItems = alertsPage.TotalItems,
                    TotalPages = alertsPage.TotalPages
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取未解決的預警時發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取預警時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取特定商品的預警
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="includeResolved">是否包含已解決的預警</param>
        /// <returns>預警列表</returns>
        [HttpGet("product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<InventoryAlertResponse>>> GetProductAlerts(
            string productId, 
            [FromQuery] bool includeResolved = false)
        {
            try
            {
                var alerts = await _inventoryAlertService.GetAlertsByProductAsync(productId, includeResolved);
                return Ok(alerts.Select(MapToResponse).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取商品預警時發生錯誤: 商品ID={ProductId}", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取預警時發生錯誤" });
            }
        }

        /// <summary>
        /// 檢查特定商品庫存並生成預警
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="variantId">變體ID (可選)</param>
        /// <returns>生成的預警</returns>
        [HttpPost("check/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryAlertResponse?>> CheckProductStock(
            string productId, 
            [FromQuery] string? variantId = null)
        {
            try
            {
                var alert = await _inventoryAlertService.CheckAndCreateAlertAsync(productId, variantId);
                
                if (alert == null)
                {
                    return Ok(new { message = "商品庫存正常，無需生成預警" });
                }
                
                return Ok(MapToResponse(alert));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查商品庫存時發生錯誤: 商品ID={ProductId}, 變體ID={VariantId}", 
                    productId, variantId ?? "無");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "檢查庫存時發生錯誤" });
            }
        }

        /// <summary>
        /// 批量檢查所有商品庫存
        /// </summary>
        /// <returns>生成的預警數量</returns>
        [HttpPost("check-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> CheckAllInventory()
        {
            try
            {
                var alerts = await _inventoryAlertService.CheckAllInventoryAsync();
                
                return Ok(new { 
                    message = $"已檢查所有商品庫存，生成了 {alerts.Count} 個預警", 
                    alertCount = alerts.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量檢查商品庫存時發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "檢查庫存時發生錯誤" });
            }
        }

        /// <summary>
        /// 解決預警
        /// </summary>
        /// <param name="alertId">預警ID</param>
        /// <param name="request">解決請求</param>
        /// <returns>更新後的預警</returns>
        [HttpPost("{alertId}/resolve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryAlertResponse>> ResolveAlert(
            string alertId, 
            [FromBody] ResolveAlertRequest request)
        {
            try
            {
                var alert = await _inventoryAlertService.ResolveAlertAsync(alertId, request.UserId, request.Notes);
                
                if (alert == null)
                {
                    return NotFound(new { message = $"找不到預警: {alertId}" });
                }
                
                return Ok(MapToResponse(alert));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解決預警時發生錯誤: 預警ID={AlertId}", alertId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "解決預警時發生錯誤" });
            }
        }

        /// <summary>
        /// 忽略預警
        /// </summary>
        /// <param name="alertId">預警ID</param>
        /// <param name="request">解決請求</param>
        /// <returns>更新後的預警</returns>
        [HttpPost("{alertId}/ignore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InventoryAlertResponse>> IgnoreAlert(
            string alertId, 
            [FromBody] ResolveAlertRequest request)
        {
            try
            {
                var alert = await _inventoryAlertService.IgnoreAlertAsync(alertId, request.UserId, request.Notes);
                
                if (alert == null)
                {
                    return NotFound(new { message = $"找不到預警: {alertId}" });
                }
                
                return Ok(MapToResponse(alert));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "忽略預警時發生錯誤: 預警ID={AlertId}", alertId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "忽略預警時發生錯誤" });
            }
        }

        /// <summary>
        /// 設定商品的低庫存閾值
        /// </summary>
        /// <param name="productId">商品ID</param>
        /// <param name="request">閾值請求</param>
        /// <param name="variantId">變體ID (可選)</param>
        /// <returns>操作結果</returns>
        [HttpPut("threshold/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SetLowStockThreshold(
            string productId, 
            [FromBody] SetLowStockThresholdRequest request,
            [FromQuery] string? variantId = null)
        {
            try
            {
                var result = await _inventoryAlertService.SetLowStockThresholdAsync(productId, request.Threshold, variantId);
                
                if (!result)
                {
                    return NotFound(new { message = $"找不到商品: {productId}" + (variantId != null ? $", 變體: {variantId}" : "") });
                }
                
                return Ok(new { 
                    message = $"已設定低庫存閾值為 {request.Threshold}", 
                    productId = productId,
                    variantId = variantId,
                    threshold = request.Threshold
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "設定低庫存閾值時發生錯誤: 商品ID={ProductId}, 變體ID={VariantId}", 
                    productId, variantId ?? "無");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "設定閾值時發生錯誤" });
            }
        }

        /// <summary>
        /// 將庫存預警實體轉換為響應DTO
        /// </summary>
        /// <param name="alert">庫存預警</param>
        /// <returns>庫存預警響應</returns>
        private static InventoryAlertResponse MapToResponse(InventoryAlert alert)
        {
            return new InventoryAlertResponse
            {
                Id = alert.Id,
                ProductId = alert.ProductId,
                ProductName = alert.ProductName,
                VariantId = alert.VariantId,
                VariantName = alert.VariantName,
                AlertType = alert.AlertType.ToString(),
                Severity = alert.Severity.ToString(),
                Status = alert.Status.ToString(),
                CurrentStock = alert.CurrentStock,
                Threshold = alert.Threshold,
                Message = alert.Message,
                SuggestedAction = alert.SuggestedAction,
                CreatedAt = alert.CreatedAt,
                UpdatedAt = alert.UpdatedAt,
                NotifiedAt = alert.NotifiedAt,
                ResolvedAt = alert.ResolvedAt,
                ResolvedBy = alert.ResolvedBy,
                ResolutionNotes = alert.ResolutionNotes
            };
        }
    }
}