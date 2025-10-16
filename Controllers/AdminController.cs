//Controllers\AdminController.cs
using HttpFileServer.Models;
using HttpFileServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpFileServer.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ConfigService _configService;
        private readonly IHostApplicationLifetime _lifetime;

        public AdminController(ConfigService configService, IHostApplicationLifetime lifetime)
        {
            _configService = configService;
            _lifetime = lifetime;
        }
        // Admin首頁，列出用戶和資料夾
        public IActionResult Index()
        {
            var users = _configService.GetUsers();
            var folders = _configService.GetFolders();
            var model = new AdminViewModel
            {
                Users = users,
                Folders = folders
            };
            return View(model);
        }

        // 新增使用者(表單GET)
        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        // 新增使用者(表單POST)
        [HttpPost]
        public IActionResult AddUser(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                TempData["ErrorMessage"] = "請完整輸入帳號、密碼和角色";
                return RedirectToAction("AddUser");
            }

            if (_configService.UserExists(username))
            {
                TempData["ErrorMessage"] = "帳號已存在";
                return RedirectToAction("AddUser");
            }

            var newUser = new User
            {
                Username = username,
                Password = password,
                Role = role,
                Permission = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                    ? ((int)PermissionLevel.Admin).ToString()
                    : null // 或預設為 null，視你是否有需要 User 給預設值
            };

            _configService.AddUser(newUser);

            TempData["SuccessMessage"] = "新增使用者成功";
            return RedirectToAction("Index");
        }


        // 修改使用者密碼(表單GET)
        [HttpGet]
        public IActionResult EditUser(string username)
        {
            var user = _configService.GetUser(username);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // 修改使用者密碼(表單POST)
        [HttpPost]
        public IActionResult EditUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "密碼不可空白";
                return RedirectToAction("EditUser", new { username });
            }

            var success = _configService.UpdateUserPassword(username, password);
            if (!success)
            {
                TempData["ErrorMessage"] = "找不到該帳號";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "更新密碼成功";
            return RedirectToAction("Index");
        }

        // 刪除使用者
        [HttpPost]
        public IActionResult DeleteUser(string username)
        {
            if (username == User.Identity!.Name)
            {
                TempData["ErrorMessage"] = "無法刪除自己";
                return RedirectToAction("Index");
            }

            var success = _configService.DeleteUser(username);
            if (!success)
            {
                TempData["ErrorMessage"] = "找不到該帳號";
            }
            else
            {
                TempData["SuccessMessage"] = "刪除成功";
            }
            return RedirectToAction("Index");
        }

        // 共享資料夾管理區，新增資料夾(表單GET)
        [HttpGet]
        public IActionResult AddFolder()
        {
            var basePath = _configService.GetBaseFolderPath();

            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                TempData["ErrorMessage"] = "⚠ 無法讀取 BaseFolderPath，請檢查 config.json";
                return RedirectToAction("Index");
            }

            // 取得 base 目錄下的子資料夾清單
            var subDirs = Directory.GetDirectories(basePath, "*", SearchOption.TopDirectoryOnly)
                .Select(d => Path.GetRelativePath(basePath, d))
                .ToList();

            ViewBag.BaseFolder = basePath;
            ViewBag.SubFolders = subDirs;

            return View();
        }


        // 新增資料夾(表單POST)
        [HttpPost]
        public IActionResult AddFolder(string folderName, string path)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(path))
            {
                TempData["ErrorMessage"] = "資料夾名稱與路徑不可空白";
                return RedirectToAction("AddFolder");
            }

            var basePath = _configService.GetBaseFolderPath();

            if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            {
                TempData["ErrorMessage"] = "系統尚未正確設定 BaseFolderPath，請聯絡管理者";
                return RedirectToAction("AddFolder");
            }

            // 組合實體路徑
            var combinedPath = Path.GetFullPath(Path.Combine(basePath, path, folderName));

            // 安全檢查：防止目錄穿越
            if (!combinedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "非法路徑，請確認輸入是否正確";
                return RedirectToAction("AddFolder");
            }

            try
            {
                if (!Directory.Exists(combinedPath))
                    Directory.CreateDirectory(combinedPath); // 建立實體資料夾
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"建立資料夾失敗: {ex.Message}";
                return RedirectToAction("AddFolder");
            }

            _configService.AddFolder(new SharedFolder
            {
                Name = folderName,
                Path = Path.Combine(path, folderName), // 儲存在 config 中的「相對路徑」
                AccessList = new List<FolderAccess>()
            });

            TempData["SuccessMessage"] = "新增資料夾成功";
            return RedirectToAction("Index");
        }


        // 修改資料夾授權(表單GET)
        [HttpGet]
        public IActionResult EditFolder(string folderName)
        {
            var folder = _configService.GetFolderByName(folderName);
            if (folder == null)
                return NotFound();

            var users = _configService.GetUsers();

            // 這裡將使用 ViewBag 傳送所有使用者資料給 View
            ViewBag.AllUsers = users;

            return View(folder);
        }

        // 修改資料夾授權(表單POST)
        //[HttpPost]
        //public IActionResult EditFolder(string folderName, Dictionary<string, string> userPermissions)
        //{
        //    // userPermissions: key = username, value = permission (string)

        //    var folder = _configService.GetFolderByName(folderName);
        //    if (folder == null)
        //    {
        //        TempData["ErrorMessage"] = "找不到資料夾";
        //        return RedirectToAction("Index");
        //    }

        //    var newAccessList = new List<FolderAccess>();

        //    foreach (var kvp in userPermissions)
        //    {
        //        var username = kvp.Key;
        //        var permStr = kvp.Value;

        //        if (System.Enum.TryParse<PermissionLevel>(permStr, out var permission))
        //        {
        //            if (permission != PermissionLevel.None) // 如果是 None 就不加入清單，表示沒權限
        //            {
        //                newAccessList.Add(new FolderAccess
        //                {
        //                    Username = username,
        //                    Permission = permission
        //                });
        //            }
        //        }
        //    }

        //    folder.AccessList = newAccessList;
        //    var success = _configService.UpdateFolderAccessList(folderName, newAccessList);

        //    if (!success)
        //    {
        //        TempData["ErrorMessage"] = "更新資料夾授權失敗";
        //        return RedirectToAction("Index");
        //    }

        //    TempData["SuccessMessage"] = "更新授權成功";
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public IActionResult EditFolder(string folderName, List<FolderAccess> accessList)
        {
            var folder = _configService.GetFolderByName(folderName);
            if (folder == null)
            {
                TempData["ErrorMessage"] = "找不到資料夾";
                return RedirectToAction("Index");
            }

            // 過濾掉 PermissionLevel.None 或空白使用者的項目
            var newAccessList = accessList
                .Where(a => !string.IsNullOrWhiteSpace(a.Username) && a.Permission != PermissionLevel.None)
                .ToList();

            var success = _configService.UpdateFolderAccessList(folderName, newAccessList);

            if (!success)
            {
                TempData["ErrorMessage"] = "更新資料夾授權失敗";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "更新授權成功";
            return RedirectToAction("Index");
        }


        // 刪除資料夾設定(表單POST)
        [HttpPost]
        public IActionResult DeleteFolder(string folderName)
        {
            var success = _configService.DeleteFolder(folderName);
            if (!success)
            {
                TempData["ErrorMessage"] = "找不到資料夾";
            }
            else
            {
                TempData["SuccessMessage"] = "刪除資料夾成功";
            }
            return RedirectToAction("Index");
        }

        [HttpPost("shutdown")]
        public IActionResult Shutdown()
        {
            // 取得目前使用者的 PermissionLevel
            var permissionClaim = User.Claims.FirstOrDefault(c => c.Type == "PermissionLevel");
            if (permissionClaim == null || !int.TryParse(permissionClaim.Value, out int level))
            {
                return Unauthorized("使用者無權限資訊");
            }

            if (level < (int)PermissionLevel.Admin)
            {
                return Forbid("權限不足，只有 Admin 可關閉伺服器");
            }

            // 開啟背景執行緒進行關閉
            new Thread(() =>
            {
                Thread.Sleep(1000); // 等待 1 秒讓回應能傳回前端
                _lifetime.StopApplication();
            }).Start();

            return Ok(new { message = "✅ 伺服器即將關閉..." });
        }

    }
}
