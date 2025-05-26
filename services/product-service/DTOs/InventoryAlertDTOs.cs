using ProductService.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs
{
    /// <summary>
    /// 庫存預警響應
    /// </summary>
    public class InventoryAlertResponse
    {
        /// <summary>
        /// 預警ID
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// 商品ID
        /// </summary>
        public string ProductId { get; set; } = null!;

        /// <summary>
        /// 商品名稱
        /// </summary>
        public string ProductName { get; set; } = null!;

        /// <summary>
        /// 變體ID (如果適用)
        /// </summary>
        public string? VariantId { get; set; }

        /// <summary>
        /// 變體名稱 (如果適用)
        /// </summary>
        public string? VariantName { get; set; }

        /// <summary>
        /// 預警類型
        /// </summary>
        public string AlertType { get; set; } = null!;

        /// <summary>
        /// 預警嚴重程度
        /// </summary>
        public string Severity { get; set; } = null!;

        /// <summary>
        /// 預警狀態
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// 當前庫存數量
        /// </summary>
        public int CurrentStock { get; set; }

        /// <summary>
        /// 庫存閾值
        /// </summary>
        public int Threshold { get; set; }

        /// <summary>
        /// 預警訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 建議行動
        /// </summary>
        public string SuggestedAction { get; set; } = string.Empty;

        /// <summary>
        /// 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新時間
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 通知時間
        /// </summary>
        public DateTime? NotifiedAt { get; set; }

        /// <summary>
        /// 解決時間
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// 處理者ID
        /// </summary>
        public string? ResolvedBy { get; set; }

        /// <summary>
        /// 解決備註
        /// </summary>
        public string? ResolutionNotes { get; set; }
    }

    /// <summary>
    /// 解決預警請求
    /// </summary>
    public class ResolveAlertRequest
    {
        /// <summary>
        /// 處理者ID
        /// </summary>
        [Required]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// 解決備註
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// 設定低庫存閾值請求
    /// </summary>
    public class SetLowStockThresholdRequest
    {
        /// <summary>
        /// 閾值
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int Threshold { get; set; }
    }
}