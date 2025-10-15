// Controllers/DebugController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HttpFileServer.Controllers
{
    [Route("debug")]
    public class DebugController : Controller
    {
        [HttpGet("claims")]
        [Authorize] // ✅ 必須登入才能看到 Claims
        public IActionResult Claims()
        {
            var claims = User.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value
            });

            return Json(claims);
        }
    }
}
