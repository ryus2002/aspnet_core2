using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Logging.Models;
using Shared.Logging.Services;
using System.ComponentModel.DataAnnotations;

namespace Shared.Logging.Controllers
{
    /// <summary>
    /// 日誌查詢 API 控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogQueryService _logQueryService;
        private readonly ILogger<LogsController> _logger;

        /// <summary>
        /// 初始化日誌控制器
        /// </summary>
        /// <param name="logQueryService">日誌查詢服務</param>
        /// <param name="logger">日誌記錄器</param>
        public LogsController(ILogQueryService logQueryService, ILogger<LogsController> logger)
        {
            _logQueryService = logQueryService;
            _logger = logger;
        }

        /// <summary>
        /// 獲取日誌文件列表
        /// </summary>
        /// <param name="serviceName">服務名稱（可選）</param>
        /// <returns>日誌文件列表</returns>
        [HttpGet("files")]
        public async Task<ActionResult<IEnumerable<string>>> GetLogFiles([FromQuery] string? serviceName = null)
        {
            try
            {
                _logger.LogInformation("獲取日誌文件列表，服務名稱: {ServiceName}", serviceName ?? "所有服務");
                var files = await _logQueryService.GetLogFilesAsync(serviceName);
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取日誌文件列表時發生錯誤");
                return StatusCode(500, "獲取日誌文件列表時發生錯誤");
            }
        }

        /// <summary>
        /// 從指定文件讀取日誌事件
        /// </summary>
        /// <param name="filePath">文件路徑</param>
        /// <param name="skip">跳過的記錄數</param>
        /// <param name="take">獲取的記錄數</param>
        /// <returns>日誌事件列表</returns>
        [HttpGet("file")]
        public async Task<ActionResult<IEnumerable<LogEvent>>> GetLogEventsFromFile(
            [Required] string filePath,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            try
            {
                _logger.LogInformation("從文件讀取日誌事件，文件路徑: {FilePath}，跳過: {Skip}，獲取: {Take}", filePath, skip, take);
                var events = await _logQueryService.ReadLogEventsFromFileAsync(filePath, skip, take);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "從文件讀取日誌事件時發生錯誤");
                return StatusCode(500, "從文件讀取日誌事件時發生錯誤");
            }
        }

        /// <summary>
        /// 搜索日誌事件
        /// </summary>
        /// <param name="serviceName">服務名稱（可選）</param>
        /// <param name="startTime">開始時間（可選）</param>
        /// <param name="endTime">結束時間（可選）</param>
        /// <param name="level">日誌級別（可選）</param>
        /// <param name="searchText">搜索文本（可選）</param>
        /// <param name="skip">跳過的記錄數</param>
        /// <param name="take">獲取的記錄數</param>
        /// <returns>日誌事件列表</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<LogEvent>>> SearchLogEvents(
            [FromQuery] string? serviceName = null,
            [FromQuery] DateTimeOffset? startTime = null,
            [FromQuery] DateTimeOffset? endTime = null,
            [FromQuery] string? level = null,
            [FromQuery] string? searchText = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 100)
        {
            try
            {
                _logger.LogInformation(
                    "搜索日誌事件，服務名稱: {ServiceName}，開始時間: {StartTime}，結束時間: {EndTime}，級別: {Level}，搜索文本: {SearchText}，跳過: {Skip}，獲取: {Take}",
                    serviceName ?? "所有服務",
                    startTime,
                    endTime,
                    level ?? "所有級別",
                    searchText ?? "無",
                    skip,
                    take);

                var events = await _logQueryService.SearchLogEventsAsync(
                    serviceName,
                    startTime,
                    endTime,
                    level,
                    searchText,
                    skip,
                    take);

                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜索日誌事件時發生錯誤");
                return StatusCode(500, "搜索日誌事件時發生錯誤");
            }
        }

        /// <summary>
        /// 獲取日誌統計信息
        /// </summary>
        /// <param name="serviceName">服務名稱（可選）</param>
        /// <param name="startTime">開始時間（可選）</param>
        /// <param name="endTime">結束時間（可選）</param>
        /// <returns>日誌統計信息</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<LogStatistics>> GetLogStatistics(
            [FromQuery] string? serviceName = null,
            [FromQuery] DateTimeOffset? startTime = null,
            [FromQuery] DateTimeOffset? endTime = null)
        {
            try
            {
                _logger.LogInformation(
                    "獲取日誌統計信息，服務名稱: {ServiceName}，開始時間: {StartTime}，結束時間: {EndTime}",
                    serviceName ?? "所有服務",
                    startTime,
                    endTime);

                var statistics = await _logQueryService.GetLogStatisticsAsync(serviceName, startTime, endTime);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取日誌統計信息時發生錯誤");
                return StatusCode(500, "獲取日誌統計信息時發生錯誤");
            }
        }
    }
}