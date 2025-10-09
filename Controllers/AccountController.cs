//Controllers\AccountController.cs
using HttpFileServer.Models;
using HttpFileServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

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

            // 建立 Claims，角色用 Role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
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

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            
            _sessionTracker.SetSession(user.Username, sessionId);

            return RedirectToAction("Index", "Home"); // 登入成功，導回首頁或其他頁
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