using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Services;
using System.Threading.Tasks;

namespace OrderService.Controllers
{
    /// <summary>
    /// 訂單控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        /// <summary>
        /// 建構函式
        /// </summary>
        /// <param name="orderService">訂單服務</param>
        /// <param name="logger">日誌記錄器</param>
        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// 根據ID獲取訂單
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <returns>訂單響應</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponse>> GetOrderById(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        /// <summary>
        /// 根據訂單編號獲取訂單
        /// </summary>
        /// <param name="orderNumber">訂單編號</param>
        /// <returns>訂單響應</returns>
        [HttpGet("by-number/{orderNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponse>> GetOrderByNumber(string orderNumber)
        {
            var order = await _orderService.GetOrderByNumberAsync(orderNumber);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        /// <summary>
        /// 獲取用戶的訂單列表
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="status">訂單狀態</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <returns>分頁訂單響應</returns>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<OrderResponse>>> GetUserOrders(
            string userId, 
            [FromQuery] string? status = null, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            var orders = await _orderService.GetUserOrdersAsync(userId, status, page, pageSize);
            return Ok(orders);
        }

        /// <summary>
        /// 創建訂單
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="request">創建訂單請求</param>
        /// <returns>訂單響應</returns>
        [HttpPost("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderResponse>> CreateOrder(string userId, [FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(userId, request);
                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建訂單時發生錯誤");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// 取消訂單
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <param name="request">取消訂單請求</param>
        /// <returns>取消後的訂單響應</returns>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponse>> CancelOrder(string id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                // 從請求頭中獲取用戶ID，實際應用中可能從身份驗證中獲取
                var userId = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
                var order = await _orderService.CancelOrderAsync(id, request, userId);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "嘗試取消不存在的訂單: {OrderId}", id);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "訂單取消操作無效: {OrderId}", id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消訂單時發生錯誤: {OrderId}", id);
                return BadRequest(new { message = "取消訂單時發生錯誤" });
            }
        }

        /// <summary>
        /// 更新訂單狀態
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <param name="request">更新訂單狀態請求</param>
        /// <returns>更新後的訂單響應</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                // 從請求頭中獲取用戶ID，實際應用中可能從身份驗證中獲取
                var userId = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
                var order = await _orderService.UpdateOrderStatusAsync(id, request, userId);
                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新訂單狀態時發生錯誤: {OrderId}", id);
                return BadRequest(new { message = "更新訂單狀態時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取訂單狀態歷史
        /// </summary>
        /// <param name="id">訂單ID</param>
        /// <returns>訂單狀態歷史列表</returns>
        [HttpGet("{id}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<OrderStatusHistoryResponse>>> GetOrderStatusHistory(string id)
        {
            try
            {
                var history = await _orderService.GetOrderStatusHistoryAsync(id);
                return Ok(history);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取訂單狀態歷史時發生錯誤: {OrderId}", id);
                return BadRequest(new { message = "獲取訂單狀態歷史時發生錯誤" });
            }
        }
    }
}