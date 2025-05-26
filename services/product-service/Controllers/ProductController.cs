using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Shared.Monitoring;
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
    private readonly IMetricsCollector _metricsCollector;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductService productService,
        IMetricsCollector metricsCollector,
        ILogger<ProductController> logger)
        {
            _productService = productService;
        _metricsCollector = metricsCollector;
        _logger = logger;
        }

        [HttpGet]
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
        // 使用指標收集器測量執行時間
        var result = await _metricsCollector.MeasureExecutionTimeAsync(
            "api.product.get_products",
            async () => await _productService.GetProductsAsync(
                page, pageSize, categoryId, status, minPrice, maxPrice, sortBy, sortDirection),
            ("page", page.ToString()), ("pageSize", pageSize.ToString()));

        // 記錄產品查詢事件
        _metricsCollector.IncrementCounter("business.product.query");
        return Ok(result);
            }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
        {
        var product = await _metricsCollector.MeasureExecutionTimeAsync(
            "api.product.get_product",
            async () => await _productService.GetProductByIdAsync(id),
            ("product_id", id));

        if (product == null)
        {
            return NotFound();
}

        // 記錄產品瀏覽事件
        _metricsCollector.RecordProductView(id, User.Identity?.Name);
            return Ok(product);
        }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
        var product = await _metricsCollector.MeasureExecutionTimeAsync(
            "api.product.create_product",
            async () => await _productService.CreateProductAsync(request));

        // 記錄產品創建事件
        _metricsCollector.IncrementCounter("business.product.created");

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
    {
        var product = await _metricsCollector.MeasureExecutionTimeAsync(
            "api.product.update_product",
            async () => await _productService.UpdateProductAsync(id, request),
            ("product_id", id));

        // 記錄產品更新事件
        _metricsCollector.IncrementCounter("business.product.updated");

        return Ok(product);
}

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var result = await _metricsCollector.MeasureExecutionTimeAsync(
            "api.product.delete_product",
            async () => await _productService.DeleteProductAsync(id),
            ("product_id", id));

        if (!result)
        {
            return NotFound();
        }

        // 記錄產品刪除事件
        _metricsCollector.IncrementCounter("business.product.deleted");

        return NoContent();
    }

    [HttpGet("search")]
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
        var result = await _metricsCollector.MeasureExecutionTimeAsync(
            "api.product.search_products",
            async () => await _productService.SearchProductsAsync(
                keyword, page, pageSize, categoryId, minPrice, maxPrice, sortBy, sortDirection),
            ("keyword", keyword), ("page", page.ToString()), ("pageSize", pageSize.ToString()));

        // 記錄產品搜尋事件
        _metricsCollector.RecordProductSearch(keyword, result.TotalItems);

        return Ok(result);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateProductStock(string id, [FromBody] UpdateStockRequest request)
    {
        var product = await _metricsCollector.MeasureExecutionTimeAsync(
            "api.product.update_stock",
            async () => await _productService.UpdateProductStockAsync(id, request),
            ("product_id", id));

        // 記錄庫存更新事件
        _metricsCollector.IncrementCounter("business.inventory.updated");

        return Ok(product);
    }

    [HttpGet("{id}/stock")]
    public async Task<IActionResult> GetProductStock(string id, [FromQuery] string? variantId = null)
    {
        var stock = await _metricsCollector.MeasureExecutionTimeAsync(
            "api.product.get_stock",
            async () => await _productService.GetProductStockAsync(id, variantId),
            ("product_id", id), ("variant_id", variantId ?? "default"));

        return Ok(stock);
    }
}