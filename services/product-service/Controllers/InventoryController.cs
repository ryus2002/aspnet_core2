using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;
using System.Threading.Tasks;

namespace ProductService.Controllers
{
    /// <summary>
    /// 庫存控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="inventoryService">庫存服務</param>
        /// <param name="logger">日誌記錄器</param>
        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// 創建庫存預留
        /// </summary>
        /// <param name="request">預留請求</param>
        /// <returns>預留響應</returns>
        [HttpPost("reservations")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ReservationResponse>> CreateReservation([FromBody] CreateReservationRequest request)
        {
            try
            {
                var reservation = await _inventoryService.CreateReservationAsync(request);
                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, new ReservationResponse
                {
                    Id = reservation.Id,
                    OwnerId = reservation.OwnerId,
                    OwnerType = reservation.OwnerType,
                    Status = reservation.Status,
                    ExpiresAt = reservation.ExpiresAt,
                    CreatedAt = reservation.CreatedAt
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建庫存預留時發生錯誤");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "創建庫存預留時發生錯誤" });
            }
        }

        /// <summary>
        /// 根據ID獲取預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>預留響應</returns>
        [HttpGet("reservations/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ReservationResponse>> GetReservation(string id)
        {
            var reservation = await _inventoryService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            
            return Ok(new ReservationResponse
            {
                Id = reservation.Id,
                OwnerId = reservation.OwnerId,
                OwnerType = reservation.OwnerType,
                Status = reservation.Status,
                Items = reservation.Items.Select(i => new ReservationItemResponse
                {
                    ProductId = i.ProductId,
                    VariantId = i.VariantId,
                    Quantity = i.Quantity
                }).ToList(),
                ExpiresAt = reservation.ExpiresAt,
                CreatedAt = reservation.CreatedAt
            });
        }

        /// <summary>
        /// 確認預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <param name="request">確認請求</param>
        /// <returns>操作結果</returns>
        [HttpPost("reservations/{id}/confirm")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> ConfirmReservation(string id, [FromBody] ConfirmReservationRequest request)
        {
            try
            {
                var result = await _inventoryService.ConfirmReservationAsync(id, request.ReferenceId);
                if (!result)
                {
                    return NotFound();
                }
                return Ok(new { message = "預留確認成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "確認庫存預留時發生錯誤: {ReservationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "確認庫存預留時發生錯誤" });
            }
        }

        /// <summary>
        /// 取消預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>操作結果</returns>
        [HttpPost("reservations/{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CancelReservation(string id)
        {
            try
            {
                var result = await _inventoryService.CancelReservationAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return Ok(new { message = "預留取消成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消庫存預留時發生錯誤: {ReservationId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "取消庫存預留時發生錯誤" });
            }
        }

        /// <summary>
        /// 回滾庫存
        /// </summary>
        /// <param name="request">回滾請求</param>
        /// <returns>操作結果</returns>
        [HttpPost("rollback")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RollbackInventory([FromBody] RollbackInventoryRequest request)
        {
            try
            {
                var result = await _inventoryService.RollbackInventoryAsync(request.OrderId, request.Items);
                return Ok(new { message = "庫存回滾成功", success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回滾庫存時發生錯誤: {OrderId}", request.OrderId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "回滾庫存時發生錯誤" });
            }
        }
    }
}