//Models/AdminViewModel.cs
using System.Collections.Generic;

namespace HttpFileServer.Models
{
    public class AdminViewModel
    {
        // 系統中的所有使用者帳號
        public List<User> Users { get; set; } = new List<User>();		
        public List<SharedFolder> Folders { get; set; } = new List<SharedFolder>();
    }
}