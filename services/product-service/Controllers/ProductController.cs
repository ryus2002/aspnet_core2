using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers
{
    /// <summary>
    /// 商品控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="productService">商品服務</param>
        /// <param name="logger">日誌記錄器</param>
        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 創建商品
        /// </summary>
        /// <param name="request">創建商品請求</param>
        /// <returns>創建的商品</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                _logger.LogInformation("接收到創建商品請求: {Name}", request.Name);

                var product = await _productService.CreateProductAsync(request);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "創建商品參數錯誤: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建商品時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "創建商品時發生錯誤" });
            }
        }

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="request">更新商品請求</param>
        /// <returns>更新後的商品</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                _logger.LogInformation("接收到更新商品請求: ID={Id}", id);

                var product = await _productService.UpdateProductAsync(id, request);

                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "更新商品時找不到商品: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "更新商品參數錯誤: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新商品時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "更新商品時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>商品</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(string id)
        {
            try
            {
                _logger.LogInformation("接收到獲取商品請求: ID={Id}", id);

                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound(new { message = $"商品不存在: {id}" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取商品時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取商品時發生錯誤" });
            }
        }

        /// <summary>
        /// 刪除商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>操作結果</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            try
            {
                _logger.LogInformation("接收到刪除商品請求: ID={Id}", id);

                var result = await _productService.DeleteProductAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"商品不存在: {id}" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除商品時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "刪除商品時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取商品列表
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <param name="categoryId">分類ID</param>
        /// <param name="status">商品狀態</param>
        /// <param name="minPrice">最低價格</param>
        /// <param name="maxPrice">最高價格</param>
        /// <param name="sortBy">排序字段</param>
        /// <param name="sortDirection">排序方向</param>
        /// <returns>分頁商品列表</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<Product>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? categoryId = null,
            [FromQuery] string? status = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortDirection = "desc")
        {
            try
            {
                _logger.LogInformation("接收到獲取商品列表請求: 頁碼={Page}, 每頁大小={PageSize}", page, pageSize);

                var products = await _productService.GetProductsAsync(
                    page, pageSize, categoryId, status, minPrice, maxPrice, sortBy, sortDirection);

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取商品列表時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取商品列表時發生錯誤" });
            }
        }

        /// <summary>
        /// 搜尋商品
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁大小</param>
        /// <param name="categoryId">分類ID</param>
        /// <param name="minPrice">最低價格</param>
        /// <param name="maxPrice">最高價格</param>
        /// <param name="sortBy">排序字段</param>
        /// <param name="sortDirection">排序方向</param>
        /// <returns>分頁商品列表</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(PagedResponse<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string sortBy = "score",
            [FromQuery] string sortDirection = "desc")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new { message = "關鍵字不能為空" });
                }

                _logger.LogInformation("接收到搜尋商品請求: 關鍵字={Keyword}, 頁碼={Page}, 每頁大小={PageSize}", keyword, page, pageSize);

                var products = await _productService.SearchProductsAsync(
                    keyword, page, pageSize, categoryId, minPrice, maxPrice, sortBy, sortDirection);

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋商品時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "搜尋商品時發生錯誤" });
            }
        }

        /// <summary>
        /// 更新商品庫存
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="request">庫存更新請求</param>
        /// <returns>更新後的商品</returns>
        [HttpPut("{id}/stock")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProductStock(string id, [FromBody] UpdateStockRequest request)
        {
            try
            {
                _logger.LogInformation("接收到更新商品庫存請求: ID={Id}, 數量={Quantity}", id, request.Quantity);

                var product = await _productService.UpdateProductStockAsync(id, request);

                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "更新商品庫存時找不到商品: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "更新商品庫存參數錯誤: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新商品庫存時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "更新商品庫存時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取商品庫存
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <returns>庫存信息</returns>
        [HttpGet("{id}/stock")]
        [ProducesResponseType(typeof(StockInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductStock(string id, [FromQuery] string? variantId = null)
        {
            try
            {
                _logger.LogInformation("接收到獲取商品庫存請求: ID={Id}, 變體ID={VariantId}", id, variantId ?? "無");

                var stockInfo = await _productService.GetProductStockAsync(id, variantId);

                return Ok(stockInfo);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "獲取商品庫存時找不到商品: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取商品庫存時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取商品庫存時發生錯誤" });
            }
        }
    }
}