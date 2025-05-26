using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers
{
    /// <summary>
    /// 庫存預留控制器
    /// </summary>
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<ReservationController> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="inventoryService">庫存服務</param>
        /// <param name="logger">日誌記錄器</param>
        public ReservationController(
            IInventoryService inventoryService,
            ILogger<ReservationController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// 創建庫存預留
        /// </summary>
        /// <param name="request">預留請求</param>
        /// <returns>預留結果</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Reservation), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationRequest request)
        {
            try
            {
                _logger.LogInformation($"收到創建預留請求: OwnerId={request.OwnerId}, OwnerType={request.OwnerType}");
                
                var reservation = await _inventoryService.CreateReservationAsync(request);
                
                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "創建預留請求參數無效");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "創建預留操作無效");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建預留時發生錯誤");
                return StatusCode(500, new { message = "處理請求時發生內部錯誤" });
            }
        }

        /// <summary>
        /// 獲取預留詳情
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>預留詳情</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Reservation), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetReservation(string id)
        {
            try
            {
                _logger.LogInformation($"獲取預留詳情: Id={id}");
                
                var filter = new Dictionary<string, object> { { "Id", id } };
                var reservations = await _inventoryService.GetReservationsAsync(filter);
                
                if (reservations.Count == 0)
                {
                    return NotFound(new { message = $"找不到ID為 {id} 的預留" });
                }
                
                return Ok(reservations[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取預留詳情時發生錯誤: Id={id}");
                return StatusCode(500, new { message = "處理請求時發生內部錯誤" });
            }
        }

        /// <summary>
        /// 確認預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <param name="referenceId">關聯單據ID</param>
        /// <returns>確認結果</returns>
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ConfirmReservation(string id, [FromBody] ConfirmReservationRequest request)
        {
            try
            {
                _logger.LogInformation($"確認預留: Id={id}, ReferenceId={request.ReferenceId}");
                
                var result = await _inventoryService.ConfirmReservationAsync(id, request.ReferenceId);
                
                if (!result)
                {
                    return NotFound(new { message = $"找不到ID為 {id} 的活躍預留" });
                }
                
                return Ok(new { message = "預留確認成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"確認預留時發生錯誤: Id={id}");
                return StatusCode(500, new { message = "處理請求時發生內部錯誤" });
            }
        }

        /// <summary>
        /// 取消預留
        /// </summary>
        /// <param name="id">預留ID</param>
        /// <returns>取消結果</returns>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CancelReservation(string id)
        {
            try
            {
                _logger.LogInformation($"取消預留: Id={id}");
                
                var result = await _inventoryService.CancelReservationAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = $"找不到ID為 {id} 的活躍預留" });
                }
                
                return Ok(new { message = "預留取消成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"取消預留時發生錯誤: Id={id}");
                return StatusCode(500, new { message = "處理請求時發生內部錯誤" });
            }
        }

        /// <summary>
        /// 獲取用戶的預留
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>預留列表</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(List<Reservation>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetUserReservations(string userId)
        {
            try
            {
                _logger.LogInformation($"獲取用戶預留: UserId={userId}");
                
                var reservations = await _inventoryService.GetReservationsByUserAsync(userId);
                
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取用戶預留時發生錯誤: UserId={userId}");
                return StatusCode(500, new { message = "處理請求時發生內部錯誤" });
            }
        }

        /// <summary>
        /// 獲取會話的預留
        /// </summary>
        /// <param name="sessionId">會話ID</param>
        /// <returns>預留列表</returns>
        [HttpGet("session/{sessionId}")]
        [ProducesResponseType(typeof(List<Reservation>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetSessionReservations(string sessionId)
        {
            try
            {
                _logger.LogInformation($"獲取會話預留: SessionId={sessionId}");
                
                var reservations = await _inventoryService.GetReservationsBySessionAsync(sessionId);
                
                return Ok(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"獲取會話預留時發生錯誤: SessionId={sessionId}");
                return StatusCode(500, new { message = "處理請求時發生內部錯誤" });
            }
        }

        /// <summary>
        /// 清理過期預留
        /// </summary>
        /// <returns>清理結果</returns>
        [HttpPost("cleanup")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CleanupExpiredReservations()
        {
            try
            {
                _logger.LogInformation("清理過期預留");
                
                var count = await _inventoryService.CleanupExpiredReservationsAsync();
                
                return Ok(new { message = $"成功清理 {count} 條過期預留" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理過期預留時發生錯誤");
                return StatusCode(500, new { message = "處理請求時發生內部錯誤" });
            }
        }
    }

    /// <summary>
    /// 確認預留請求
    /// </summary>
    public class ConfirmReservationRequest
    {
        /// <summary>
        /// 關聯單據ID
        /// </summary>
        public string ReferenceId { get; set; } = null!;
    }
}