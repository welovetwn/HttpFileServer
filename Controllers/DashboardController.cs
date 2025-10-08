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
            var role = User.IsInRole("Admin") ? "Admin" : "User";

            var folders = _configService.GetAccessibleFolders(username, role);

            var model = new DashboardViewModel
            {
                Username = username,
                Role = role,
                AccessibleFolders = folders
            };

            return View(model);
        }
    }
}