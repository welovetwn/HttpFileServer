//Controllers/DashboardController.cs
using HttpFileServer.Models;
using HttpFileServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HttpFileServer.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ConfigService _configService;

        public DashboardController(ConfigService configService)
        {
            _configService = configService;
        }

        [HttpGet("/dashboard")]
        public IActionResult Index()
        {
            var username = User.Identity?.Name ?? "";

            // ✅ 從 Claims 取出 PermissionLevel
            var permissionStr = User.FindFirst("PermissionLevel")?.Value ?? "0";
            int permissionLevel = int.TryParse(permissionStr, out var level) ? level : 0;

            // ✅ 使用 int 權限呼叫 ConfigService
            var folders = _configService.GetAccessibleFolders(username, permissionLevel);

            var model = new DashboardViewModel
            {
                Username = username,
                PermissionLevel = permissionLevel,
                AccessibleFolders = folders
            };

            return View(model);
        }
    }
}