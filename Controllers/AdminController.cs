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

        public AdminController(ConfigService configService)
        {
            _configService = configService;
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

            _configService.AddUser(new User
            {
                Username = username,
                Password = password,
                Role = role
            });

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
            return View();
        }

        // 新增資料夾(表單POST)
        [HttpPost]
        public IActionResult AddFolder(string folderName, string path, string allowedUsers)
        {
            if (string.IsNullOrWhiteSpace(folderName) || string.IsNullOrWhiteSpace(path))
            {
                TempData["ErrorMessage"] = "資料夾名稱與路徑不可空白";
                return RedirectToAction("AddFolder");
            }

            var userList = string.IsNullOrWhiteSpace(allowedUsers)
                ? new List<string>()
                : allowedUsers.Split(',').Select(u => u.Trim()).ToList();

            _configService.AddFolder(new SharedFolder
            {
                Name = folderName,
                Path = path,
                AllowedUsers = userList
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

            return View(folder);
        }

        // 修改資料夾授權(表單POST)
        [HttpPost]
        public IActionResult EditFolder(string folderName, string allowedUsers)
        {
            var userList = string.IsNullOrWhiteSpace(allowedUsers)
                ? new List<string>()
                : allowedUsers.Split(',').Select(u => u.Trim()).ToList();

            var success = _configService.UpdateFolderAllowedUsers(folderName, userList);
            if (!success)
            {
                TempData["ErrorMessage"] = "找不到資料夾";
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
    }
}
