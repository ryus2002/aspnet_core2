using Microsoft.AspNetCore.Mvc;

namespace Shared.Logging.Controllers
{
    /// <summary>
    /// 日誌查看工具控制器
    /// </summary>
    [Route("logs")]
    public class LogViewerController : Controller
    {
        /// <summary>
        /// 顯示日誌查看工具主頁
        /// </summary>
        /// <returns>視圖結果</returns>
        [HttpGet]
        public IActionResult Index()
        {
            return File("~/index.html", "text/html");
        }
    }
}