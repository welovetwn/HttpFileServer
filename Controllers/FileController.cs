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
        private readonly long _maxFileSize = 5L * 1024 * 1024 * 1024; // 5GB

        public FileController(ConfigService configService)
        {
            _configService = configService;
        }

        // 顯示檔案清單頁面
        [HttpGet("/files/{folderName}")]
        public IActionResult Index(string folderName)
        {
            var user = User.Identity?.Name ?? "";
            var role = User.IsInRole("Admin") ? "Admin" : "User";

            var folder = _configService.GetFolderByName(folderName);
            if (folder == null) return NotFound("Folder not found");

            if (role != "Admin" && !folder.AllowedUsers.Contains(user, StringComparer.OrdinalIgnoreCase))
                return Forbid();

            var files = Directory.Exists(folder.Path)
                ? new DirectoryInfo(folder.Path).GetFiles()
                : Array.Empty<FileInfo>();

            var canDownload = role == "Admin" || User.HasClaim("Permission", "DownloadOnly") || User.HasClaim("Permission", "UploadAndDownload");
            var canUpload = role == "Admin" || User.HasClaim("Permission", "UploadAndDownload");

            var model = new FolderViewModel
            {
                FolderName = folder.Name,
                Files = files,
                CanDownload = canDownload,
                CanUpload = canUpload
            };

            return View("Folder", model);
        }

        // 📤 加在這裡：處理檔案上傳
        [HttpPost("/files/{folderName}/upload")]
        public async Task<IActionResult> Upload(string folderName, IFormFile file)
        {
            var user = User.Identity?.Name ?? "";
            var role = User.IsInRole("Admin") ? "Admin" : "User";

            var folder = _configService.GetFolderByName(folderName);
            if (folder == null)
            {
                TempData["ErrorMessage"] = "資料夾找不到！";
                return RedirectToAction("Index", new { folderName });
            }

            if (role != "Admin" && !folder.AllowedUsers.Contains(user, StringComparer.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "您沒有權限上傳檔案！";
                return RedirectToAction("Index", new { folderName });
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "未選擇檔案！";
                return RedirectToAction("Index", new { folderName });
            }

            if (file.Length > _maxFileSize)
            {
                TempData["ErrorMessage"] = "檔案大小超過最大限制（5GB）！";
                return RedirectToAction("Index", new { folderName });
            }

            // 檢查並創建資料夾（如果不存在）
            if (!Directory.Exists(folder.Path))
            {
                Directory.CreateDirectory(folder.Path); // 如果資料夾不存在，則創建
            }

            // 檢查是否有相同檔名的檔案
            var filePath = Path.Combine(folder.Path, file.FileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);

            int counter = 1;
            while (System.IO.File.Exists(filePath))  // 檢查檔案是否已經存在
            {
                var newFileName = $"{fileNameWithoutExtension}_{counter}{extension}";
                filePath = Path.Combine(folder.Path, newFileName);  // 修改檔名並重新檢查
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

    }
}