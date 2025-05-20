using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// 獲取所有商品
        /// </summary>
        /// <remarks>
        /// 獲取所有商品的列表，支持分頁、排序和過濾
        /// </remarks>
        /// <param name="page">頁碼，默認為1</param>
        /// <param name="pageSize">每頁項目數，默認為10</param>
        /// <param name="categoryId">按類別ID過濾</param>
        /// <param name="minPrice">最低價格過濾</param>
        /// <param name="maxPrice">最高價格過濾</param>
        /// <param name="sortBy">排序字段，如"price"、"name"等，默認為"createdAt"</param>
        /// <param name="sortDirection">排序方向，"asc"或"desc"，默認為"desc"</param>
        /// <returns>分頁商品列表</returns>
        /// <response code="200">成功</response>
        /// <response code="400">請求參數無效</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "獲取所有商品",
            Description = "獲取所有商品的列表，支持分頁、排序和過濾",
            OperationId = "GetProducts",
            Tags = new[] { "商品" }
        )]
        [SwaggerResponse(200, "成功", typeof(PagedResponse<Product>))]
        [SwaggerResponse(400, "請求參數無效")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? categoryId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortDirection = "desc")
        {
                var products = await _productService.GetProductsAsync(
                page, pageSize, categoryId, null, minPrice, maxPrice, sortBy, sortDirection);
                return Ok(products);
            }

        /// <summary>
        /// 獲取單個商品
        /// </summary>
        /// <remarks>
        /// 通過ID獲取特定商品的詳細信息
        /// </remarks>
        /// <param name="id">商品ID</param>
        /// <returns>商品詳細信息</returns>
        /// <response code="200">成功</response>
        /// <response code="404">商品不存在</response>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "獲取單個商品",
            Description = "通過ID獲取特定商品的詳細信息",
            OperationId = "GetProduct",
            Tags = new[] { "商品" }
        )]
        [SwaggerResponse(200, "成功", typeof(Product))]
        [SwaggerResponse(404, "商品不存在")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();
                return Ok(product);
            }

        /// <summary>
        /// 獲取商品庫存
        /// </summary>
        /// <remarks>
        /// 獲取指定商品的庫存信息
        /// </remarks>
        /// <param name="id">商品ID</param>
        /// <param name="variantId">變體ID</param>
        /// <returns>庫存信息</returns>
        /// <response code="200">成功</response>
        /// <response code="404">商品不存在</response>
        [HttpGet("{id}/stock")]
        [SwaggerOperation(
            Summary = "獲取商品庫存",
            Description = "獲取指定商品的庫存信息",
            OperationId = "GetProductStock",
            Tags = new[] { "庫存" }
        )]
        [SwaggerResponse(200, "成功", typeof(StockInfo))]
        [SwaggerResponse(404, "商品不存在")]
        public async Task<IActionResult> GetProductStock(string id, [FromQuery] string? variantId = null)
        {
            var stock = await _productService.GetProductStockAsync(id, variantId);
            return Ok(stock);
            }

        /// <summary>
        /// 搜索商品
        /// </summary>
        /// <remarks>
        /// 根據關鍵詞搜索商品
        /// </remarks>
        /// <param name="keyword">搜索關鍵詞</param>
        /// <param name="page">頁碼，默認為1</param>
        /// <param name="pageSize">每頁項目數，默認為10</param>
        /// <param name="categoryId">按類別ID過濾</param>
        /// <param name="minPrice">最低價格過濾</param>
        /// <param name="maxPrice">最高價格過濾</param>
        /// <param name="sortBy">排序字段，默認為"score"</param>
        /// <param name="sortDirection">排序方向，"asc"或"desc"，默認為"desc"</param>
        /// <returns>分頁商品列表</returns>
        /// <response code="200">成功</response>
        /// <response code="400">請求參數無效</response>
        [HttpGet("search")]
        [SwaggerOperation(
            Summary = "搜索商品",
            Description = "根據關鍵詞搜索商品",
            OperationId = "SearchProducts",
            Tags = new[] { "商品" }
        )]
        [SwaggerResponse(200, "成功", typeof(PagedResponse<Product>))]
        [SwaggerResponse(400, "請求參數無效")]
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
            var products = await _productService.SearchProductsAsync(
                keyword, page, pageSize, categoryId, minPrice, maxPrice, sortBy, sortDirection);
            return Ok(products);
            }

        /// <summary>
        /// 創建商品
        /// </summary>
        /// <remarks>
        /// 創建新商品。需要管理員權限。
        /// </remarks>
        /// <param name="request">創建商品請求</param>
        /// <returns>創建的商品</returns>
        /// <response code="201">商品創建成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="401">未認證</response>
        /// <response code="403">權限不足</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [SwaggerOperation(
            Summary = "創建商品",
            Description = "創建新商品。需要管理員權限。",
            OperationId = "CreateProduct",
            Tags = new[] { "商品管理" }
        )]
        [SwaggerResponse(201, "商品創建成功", typeof(Product))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "未認證")]
        [SwaggerResponse(403, "權限不足")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            var product = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <remarks>
        /// 更新現有商品。需要管理員權限。
        /// </remarks>
        /// <param name="id">商品ID</param>
        /// <param name="request">更新商品請求</param>
        /// <returns>更新後的商品</returns>
        /// <response code="200">商品更新成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="401">未認證</response>
        /// <response code="403">權限不足</response>
        /// <response code="404">商品不存在</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "更新商品",
            Description = "更新現有商品。需要管理員權限。",
            OperationId = "UpdateProduct",
            Tags = new[] { "商品管理" }
        )]
        [SwaggerResponse(200, "商品更新成功", typeof(Product))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "未認證")]
        [SwaggerResponse(403, "權限不足")]
        [SwaggerResponse(404, "商品不存在")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
        {
            var product = await _productService.UpdateProductAsync(id, request);
            return Ok(product);
}

        /// <summary>
        /// 更新商品庫存
        /// </summary>
        /// <remarks>
        /// 更新指定商品的庫存數量。需要管理員權限。
        /// </remarks>
        /// <param name="id">商品ID</param>
        /// <param name="request">庫存更新請求</param>
        /// <returns>更新後的商品</returns>
        /// <response code="200">庫存更新成功</response>
        /// <response code="400">請求參數無效</response>
        /// <response code="401">未認證</response>
        /// <response code="403">權限不足</response>
        /// <response code="404">商品不存在</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/stock")]
        [SwaggerOperation(
            Summary = "更新商品庫存",
            Description = "更新指定商品的庫存數量。需要管理員權限。",
            OperationId = "UpdateProductStock",
            Tags = new[] { "庫存管理" }
        )]
        [SwaggerResponse(200, "庫存更新成功", typeof(Product))]
        [SwaggerResponse(400, "請求參數無效")]
        [SwaggerResponse(401, "未認證")]
        [SwaggerResponse(403, "權限不足")]
        [SwaggerResponse(404, "商品不存在")]
        public async Task<IActionResult> UpdateProductStock(string id, [FromBody] UpdateStockRequest request)
        {
            var product = await _productService.UpdateProductStockAsync(id, request);
            return Ok(product);
        }

        /// <summary>
        /// 刪除商品
        /// </summary>
        /// <remarks>
        /// 刪除指定商品。需要管理員權限。
        /// </remarks>
        /// <param name="id">商品ID</param>
        /// <returns>無內容</returns>
        /// <response code="204">商品刪除成功</response>
        /// <response code="401">未認證</response>
        /// <response code="403">權限不足</response>
        /// <response code="404">商品不存在</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "刪除商品",
            Description = "刪除指定商品。需要管理員權限。",
            OperationId = "DeleteProduct",
            Tags = new[] { "商品管理" }
        )]
        [SwaggerResponse(204, "商品刪除成功")]
        [SwaggerResponse(401, "未認證")]
        [SwaggerResponse(403, "權限不足")]
        [SwaggerResponse(404, "商品不存在")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
    }
}