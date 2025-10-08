//Services\ConfigService.cs
using HttpFileServer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HttpFileServer.Services
{
    public class ConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly string _appSettingsPath;
        private ConfigRoot? _config;

        public ConfigService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            // 取得 appsettings.json 實體路徑
            _appSettingsPath = Path.Combine(env.ContentRootPath, "appsettings.json");

            LoadConfig();
        }

        private void LoadConfig()
        {
            // 先用 Configuration 讀取
            _config = _configuration.Get<ConfigRoot>();

            // 也可以考慮直接從檔案反序列化，但一般用 Configuration 就好
            if (_config == null)
            {
                _config = new ConfigRoot
                {
                    Users = new List<User>(),
                    SharedFolders = new List<SharedFolder>()
                };
            }
        }

        private void SaveConfig()
        {
            if (_config == null) return;

            // 讀取整個 appsettings.json 的 JSON 結構
            var jsonString = File.ReadAllText(_appSettingsPath);
            using var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement.Clone();

            // 將 Users 與 SharedFolders 替換成新資料
            var options = new JsonSerializerOptions { WriteIndented = true };

            var jsonObj = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString) ?? new Dictionary<string, object>();

            // 我們手動序列化 Users 與 SharedFolders 到 JSON，再替換原本字串
            // 這邊簡單用 Dictionary 重新構建

            // 用一個中繼物件替換 Users、SharedFolders
            jsonObj["Users"] = _config.Users;
            jsonObj["SharedFolders"] = _config.SharedFolders;

            // 將整個物件序列化回字串
            var newJsonString = JsonSerializer.Serialize(jsonObj, options);

            // 寫回 appsettings.json
            File.WriteAllText(_appSettingsPath, newJsonString);
        }

        public List<User> GetUsers()
        {
            return _config?.Users ?? new List<User>();
        }

        public User? GetUser(string username)
        {
            return GetUsers().FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public bool UserExists(string username)
        {
            return GetUser(username) != null;
        }

        public void AddUser(User user)
        {
            if (_config == null) return;
            _config.Users.Add(user);
            SaveConfig();
        }

        public bool UpdateUserPassword(string username, string newPassword)
        {
            var user = GetUser(username);
            if (user == null) return false;
            user.Password = newPassword;
            SaveConfig();
            return true;
        }

        public bool DeleteUser(string username)
        {
            if (_config == null) return false;
            var user = GetUser(username);
            if (user == null) return false;
            _config.Users.Remove(user);
            SaveConfig();
            return true;
        }

        public List<SharedFolder> GetFolders()
        {
            return _config?.SharedFolders ?? new List<SharedFolder>();
        }

        public SharedFolder? GetFolderByName(string name)
        {
            return GetFolders().FirstOrDefault(f =>
                f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void AddFolder(SharedFolder folder)
        {
            if (_config == null) return;
            _config.SharedFolders.Add(folder);
            SaveConfig();
        }

        public bool UpdateFolderAllowedUsers(string folderName, List<string> allowedUsers)
        {
            var folder = GetFolderByName(folderName);
            if (folder == null) return false;
            folder.AllowedUsers = allowedUsers;
            SaveConfig();
            return true;
        }

        public bool DeleteFolder(string folderName)
        {
            if (_config == null) return false;
            var folder = GetFolderByName(folderName);
            if (folder == null) return false;
            _config.SharedFolders.Remove(folder);
            SaveConfig();
            return true;
        }

        public List<SharedFolder> GetAccessibleFolders(string username, string role)
        {
            if (_config == null) return new List<SharedFolder>();

            if (role == "Admin")
            {
                return _config.SharedFolders;
            }

            return _config.SharedFolders
                .Where(f => f.AllowedUsers.Contains(username, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        public User? ValidateUser(string username, string password)
        {
            return GetUsers().FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)
                && u.Password == password);
        }
    }
}