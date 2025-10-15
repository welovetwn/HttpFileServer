// Controllers\AccountController.cs

using HttpFileServer.Extensions;
using HttpFileServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HttpFileServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly ConfigService _configService;
        private readonly AuthSessionTracker _sessionTracker;

        public AccountController(ConfigService configService, AuthSessionTracker sessionTracker)
        {
            _configService = configService;
            _sessionTracker = sessionTracker;
        }

        [HttpGet("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
        {
            var user = _configService.ValidateUser(username, password);
            if (user == null)
            {
                ModelState.AddModelError("", "帳號或密碼錯誤");
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            string sessionId = Guid.NewGuid().ToString();

            // 建立 Claims：加入 Username 與 PermissionLevel（整數）
            var permission = int.TryParse(user.Permission, out var parsed) ? parsed.ToString() : "0";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("PermissionLevel", permission),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };



            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,  // 記住登入狀態
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _sessionTracker.SetSession(user.Username, sessionId);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost("/logout")]
        public async Task<IActionResult> Logout()
        {
            var username = User.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                _sessionTracker.RemoveSession(username);
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/login");
        }
    }
}