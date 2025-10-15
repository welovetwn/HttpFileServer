// 檔案路徑：HttpFileServer\Controllers\Api\FolderApiController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;

namespace HttpFileServer.Controllers.Api
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/folders")]
    public class FolderApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSubFolders([FromQuery] string? path)
        {
            string root = Directory.GetCurrentDirectory();
            string target = string.IsNullOrWhiteSpace(path)
                ? root
                : Path.GetFullPath(Path.Combine(root, path));

            if (!target.StartsWith(root))
                return BadRequest(new { success = false, error = "Invalid path" });

            if (!Directory.Exists(target))
                return NotFound(new { success = false, error = "Directory not found" });

            var folders = Directory.GetDirectories(target)
                .Select(d => new
                {
                    name = Path.GetFileName(d),
                    relativePath = Path.GetRelativePath(root, d),
                    hasSubfolders = Directory.GetDirectories(d).Any()
                }).ToList();

            return Ok(folders);
        }
    }
}
