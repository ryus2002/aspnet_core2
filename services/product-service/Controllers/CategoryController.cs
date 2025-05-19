using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers
{
    /// <summary>
    /// 分類控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        /// <summary>
        /// 構造函數
        /// </summary>
        /// <param name="categoryService">分類服務</param>
        /// <param name="logger">日誌記錄器</param>
        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 創建分類
        /// </summary>
        /// <param name="request">創建分類請求</param>
        /// <returns>創建的分類</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                _logger.LogInformation("接收到創建分類請求: {Name}", request.Name);

                var category = await _categoryService.CreateCategoryAsync(request);

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "創建分類操作錯誤: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "創建分類找不到相關資源: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "創建分類時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "創建分類時發生錯誤" });
            }
        }

        /// <summary>
        /// 更新分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <param name="request">更新分類請求</param>
        /// <returns>更新後的分類</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                _logger.LogInformation("接收到更新分類請求: ID={Id}", id);

                var category = await _categoryService.UpdateCategoryAsync(id, request);

                return Ok(category);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "更新分類時找不到分類: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "更新分類操作錯誤: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分類時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "更新分類時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <returns>分類</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(string id)
        {
            try
            {
                _logger.LogInformation("接收到獲取分類請求: ID={Id}", id);

                var category = await _categoryService.GetCategoryByIdAsync(id);

                if (category == null)
                {
                    return NotFound(new { message = $"分類不存在: {id}" });
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取分類時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取分類時發生錯誤" });
            }
        }

        /// <summary>
        /// 根據Slug獲取分類
        /// </summary>
        /// <param name="slug">Slug</param>
        /// <returns>分類</returns>
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            try
            {
                _logger.LogInformation("接收到根據Slug獲取分類請求: Slug={Slug}", slug);

                var category = await _categoryService.GetCategoryBySlugAsync(slug);

                if (category == null)
                {
                    return NotFound(new { message = $"分類不存在: {slug}" });
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根據Slug獲取分類時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "根據Slug獲取分類時發生錯誤" });
            }
        }

        /// <summary>
        /// 刪除分類
        /// </summary>
        /// <param name="id">分類ID</param>
        /// <returns>操作結果</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                _logger.LogInformation("接收到刪除分類請求: ID={Id}", id);

                var result = await _categoryService.DeleteCategoryAsync(id);

                if (!result)
                {
                    return NotFound(new { message = $"分類不存在: {id}" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "刪除分類操作錯誤: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除分類時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "刪除分類時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取所有分類
        /// </summary>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>分類列表</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllCategories([FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("接收到獲取所有分類請求: 包含未啟用={IncludeInactive}", includeInactive);

                var categories = await _categoryService.GetAllCategoriesAsync(includeInactive);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有分類時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取所有分類時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取子分類
        /// </summary>
        /// <param name="parentId">父分類ID</param>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>子分類列表</returns>
        [HttpGet("children/{parentId}")]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetChildCategories(string parentId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("接收到獲取子分類請求: 父分類ID={ParentId}, 包含未啟用={IncludeInactive}", parentId, includeInactive);

                var categories = await _categoryService.GetChildCategoriesAsync(parentId, includeInactive);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取子分類時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取子分類時發生錯誤" });
            }
        }

        /// <summary>
        /// 獲取分類樹
        /// </summary>
        /// <param name="includeInactive">是否包含未啟用的分類</param>
        /// <returns>分類樹</returns>
        [HttpGet("tree")]
        [ProducesResponseType(typeof(List<CategoryTreeNode>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryTree([FromQuery] bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("接收到獲取分類樹請求: 包含未啟用={IncludeInactive}", includeInactive);

                var categoryTree = await _categoryService.GetCategoryTreeAsync(includeInactive);

                return Ok(categoryTree);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取分類樹時發生錯誤: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "獲取分類樹時發生錯誤" });
            }
        }
    }
}