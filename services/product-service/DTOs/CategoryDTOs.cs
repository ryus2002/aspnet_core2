using System.ComponentModel.DataAnnotations;
using ProductService.Models;

namespace ProductService.DTOs
{
    /// <summary>
    /// 創建分類請求
    /// </summary>
    public class CreateCategoryRequest
    {
        /// <summary>
        /// 分類名稱
        /// </summary>
        [Required(ErrorMessage = "分類名稱為必填")]
        [StringLength(50, ErrorMessage = "分類名稱不能超過50個字符")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// 分類描述
        /// </summary>
        [StringLength(500, ErrorMessage = "分類描述不能超過500個字符")]
        public string? Description { get; set; }

        /// <summary>
        /// URL友好的標識符
        /// </summary>
        [Required(ErrorMessage = "Slug為必填")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug只能包含小寫字母、數字和連字符")]
        [StringLength(50, ErrorMessage = "Slug不能超過50個字符")]
        public string Slug { get; set; } = null!;

        /// <summary>
        /// 父分類ID (頂級分類為null)
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// 分類狀態: active, inactive
        /// </summary>
        public string? Status { get; set; } = "active";

        /// <summary>
        /// 排序順序
        /// </summary>
        public int? SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 分類圖片
        /// </summary>
        public ProductImageDto? Image { get; set; }
    }

    /// <summary>
    /// 更新分類請求
    /// </summary>
    public class UpdateCategoryRequest
    {
        /// <summary>
        /// 分類名稱
        /// </summary>
        [StringLength(50, ErrorMessage = "分類名稱不能超過50個字符")]
        public string? Name { get; set; }

        /// <summary>
        /// 分類描述
        /// </summary>
        [StringLength(500, ErrorMessage = "分類描述不能超過500個字符")]
        public string? Description { get; set; }

        /// <summary>
        /// URL友好的標識符
        /// </summary>
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug只能包含小寫字母、數字和連字符")]
        [StringLength(50, ErrorMessage = "Slug不能超過50個字符")]
        public string? Slug { get; set; }

        /// <summary>
        /// 父分類ID
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// 分類狀態: active, inactive
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// 排序順序
        /// </summary>
        public int? SortOrder { get; set; }

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 分類圖片
        /// </summary>
        public ProductImageDto? Image { get; set; }
    }

    /// <summary>
    /// 分類樹節點
    /// </summary>
    public class CategoryTreeNode
    {
        /// <summary>
        /// 分類ID
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// 分類名稱
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 分類描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// URL友好的標識符
        /// </summary>
        public string Slug { get; set; } = null!;

        /// <summary>
        /// 分類層級
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 分類狀態: active, inactive
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// 排序順序
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否啟用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 分類圖片
        /// </summary>
        public ProductImageDto? Image { get; set; }

        /// <summary>
        /// 子分類
        /// </summary>
        public List<CategoryTreeNode> Children { get; set; } = new List<CategoryTreeNode>();
    }
}