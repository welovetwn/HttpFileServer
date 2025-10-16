//Models\FolderViewModel.cs
using System.IO;

namespace HttpFileServer.Models
{
    public class FolderViewModel
    {
        public string FolderName { get; set; } = "";
        public FileInfo[] Files { get; set; } = Array.Empty<FileInfo>();
        public bool CanDownload { get; set; }
        public bool CanUpload { get; set; }  // 加入此欄位以控制是否顯示上傳表單
    }
}