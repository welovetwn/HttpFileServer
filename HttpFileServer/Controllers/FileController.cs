// Controllers/FileController.cs
using HttpFileServer.Models;
using HttpFileServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HttpFileServer.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly ConfigService _configService;
        //private readonly long _maxFileSize = 5L * 1024 * 1024 * 1024; // 5GB

        public FileController(ConfigService configService)
        {
            _configService = configService;
        }

        [HttpGet("/files/{folderName}")]
        public IActionResult Index(string folderName)
        {
            var username = User.Identity?.Name ?? "";
            var role = User.IsInRole("Admin") ? "Admin" : "User";

            var folder = _configService.GetFolderByName(folderName);
            if (folder == null) return NotFound("Folder not found");

            // Admin 可以略過檢查
            if (role != "Admin")
            {
                var access = folder.AccessList.FirstOrDefault(a =>
                    a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (access == null || access.Permission == PermissionLevel.None)
                {
                    return Forbid();
                }
            }

            var files = Directory.Exists(folder.Path)
                ? new DirectoryInfo(folder.Path).GetFiles()
                : Array.Empty<FileInfo>();

            // 權限計算
            var permission = role == "Admin"
                ? PermissionLevel.FullAccess
                : folder.AccessList
                    .FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                    ?.Permission ?? PermissionLevel.None;

            var model = new FolderViewModel
            {
                FolderName = folder.Name,
                Files = files,
                CanDownload = permission == PermissionLevel.FullAccess || permission == PermissionLevel.DownloadOnly,
                CanUpload = permission == PermissionLevel.FullAccess
            };

            return View("Folder", model);
        }


        [HttpPost("/files/{folderName}/upload")]
        public async Task<IActionResult> Upload(string folderName, IFormFile file)
        {
            var username = User.Identity?.Name ?? "";
            var role = User.IsInRole("Admin") ? "Admin" : "User";

            var folder = _configService.GetFolderByName(folderName);
            if (folder == null)
            {
                TempData["ErrorMessage"] = "資料夾找不到！";
                return RedirectToAction("Index", new { folderName });
            }

            // 權限檢查
            PermissionLevel permission = PermissionLevel.None;
            if (role == "Admin")
            {
                permission = PermissionLevel.FullAccess;
            }
            else
            {
                var access = folder.AccessList.FirstOrDefault(a =>
                    a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                permission = access?.Permission ?? PermissionLevel.None;
            }

            if (permission != PermissionLevel.FullAccess)
            {
                TempData["ErrorMessage"] = "您沒有權限上傳檔案！";
                return RedirectToAction("Index", new { folderName });
            }

            // 驗證檔案
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "未選擇檔案！";
                return RedirectToAction("Index", new { folderName });
            }

            const long _maxFileSize = 5L * 1024 * 1024 * 1024; // 5 GB
            if (file.Length > _maxFileSize)
            {
                TempData["ErrorMessage"] = "檔案大小超過最大限制（5GB）！";
                return RedirectToAction("Index", new { folderName });
            }

            // 確保資料夾存在
            if (!Directory.Exists(folder.Path))
            {
                Directory.CreateDirectory(folder.Path);
            }

            // 處理重複檔名
            var filePath = Path.Combine(folder.Path, file.FileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);

            int counter = 1;
            while (System.IO.File.Exists(filePath))
            {
                var newFileName = $"{fileNameWithoutExtension}_{counter}{extension}";
                filePath = Path.Combine(folder.Path, newFileName);
                counter++;
            }

            // 儲存檔案
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            TempData["SuccessMessage"] = "檔案上傳成功！";
            return RedirectToAction("Index", new { folderName });
        }


        [HttpPost("/files/{folderName}/delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteFile(string folderName, string fileName)
        {
            // 取得對應資料夾
            var folder = _configService.GetFolderByName(folderName);
            if (folder == null)
            {
                TempData["ErrorMessage"] = "資料夾找不到！";
                return RedirectToAction("Index", new { folderName });
            }

            // 構建檔案的路徑
            var filePath = Path.Combine(folder.Path, fileName);
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    // 刪除檔案
                    System.IO.File.Delete(filePath);
                    TempData["SuccessMessage"] = "檔案刪除成功！";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"刪除檔案時發生錯誤：{ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "檔案不存在！";
            }

            // 刪除完成後重新導向回資料夾頁面
            return RedirectToAction("Index", new { folderName });
        }

        [HttpGet("/files/{folderName}/download")]
        public IActionResult Download(string folderName, string file)
        {
            var username = User.Identity?.Name ?? "";

            var folder = _configService.GetFolderByName(folderName);
            if (folder == null)
                return NotFound("資料夾不存在。");

            var access = folder.AccessList
                .FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            var isAdmin = User.IsInRole("Admin");
            var hasDownloadPermission = isAdmin ||
                (access != null &&
                    (access.Permission == PermissionLevel.FullAccess ||
                        access.Permission == PermissionLevel.DownloadOnly));

            if (!hasDownloadPermission)
                return Forbid("您沒有下載此檔案的權限。");

            var fullPath = Path.Combine(folder.Path, file);
            if (!System.IO.File.Exists(fullPath))
                return NotFound("檔案不存在。");

            var contentType = "application/octet-stream";
            return PhysicalFile(fullPath, contentType, file);
        }

    }

}