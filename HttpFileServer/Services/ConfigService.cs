// Services\ConfigService.cs
using HttpFileServer.Models;
using System.Text.Json;

namespace HttpFileServer.Services
{
    public class ConfigService
    {
        private readonly string _userSettingsPath;
        private readonly string _folderSettingsPath;
        private readonly string _systemConfigPath;

        private UserSettings _userSettings;
        private FolderSettings _folderSettings;
        private Config _systemConfig;

        public ConfigService(IConfiguration configuration, IWebHostEnvironment env)
        {
            var root = env.ContentRootPath;
            _userSettingsPath = Path.Combine(root, "user_settings.json");
            _folderSettingsPath = Path.Combine(root, "folder_settings.json");
            _systemConfigPath = Path.Combine(root, "config.json");

            _userSettings = LoadFromFile<UserSettings>(_userSettingsPath) ?? new UserSettings();
            _folderSettings = LoadFromFile<FolderSettings>(_folderSettingsPath) ?? new FolderSettings();
            _systemConfig = LoadFromFile<Config>(_systemConfigPath) ?? new Config(); // 預設為空字串
                                                                                     // 若 config.json 沒有設定 BaseFolderPath，則使用當前執行目錄
            if (string.IsNullOrWhiteSpace(_systemConfig.BaseFolderPath))
            {
                Console.WriteLine("⚠️ 尚未設定 BaseFolderPath，將使用當前執行目錄作為預設路徑。");
                _systemConfig.BaseFolderPath = root;  // 使用當前執行目錄
                SaveToFile(_systemConfigPath, _systemConfig);  // 儲存到 config.json
            }
        }

        private T? LoadFromFile<T>(string path)
        {
            if (!File.Exists(path)) return default;
            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        private void SaveToFile<T>(string path, T data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(path, json);
        }

        // 讀取 BaseFolderPath，給其他類別使用
        public string GetBaseFolderPath()
        {
            // 檢查資料夾是否存在
            if (!Directory.Exists(_systemConfig.BaseFolderPath))
            {
                Console.WriteLine($"⚠️ 設定的資料夾路徑 {_systemConfig.BaseFolderPath} 不存在，請重新設定。");
                // 這裡可以提供進入設定頁面的鏈接或提示
                // 也可以進一步引導用戶去手動設定
            }

            return _systemConfig.BaseFolderPath;
        }

        // ========== 使用者操作 ==========
        public List<User> GetUsers() => _userSettings.Users;

        public User? GetUser(string username) =>
            _userSettings.Users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        public bool UserExists(string username) => GetUser(username) != null;

        public void AddUser(User user)
        {
            _userSettings.Users.Add(user);
            SaveToFile(_userSettingsPath, _userSettings);
        }

        public bool UpdateUserPassword(string username, string newPassword)
        {
            var user = GetUser(username);
            if (user == null) return false;

            user.Password = newPassword;
            SaveToFile(_userSettingsPath, _userSettings);
            return true;
        }

        public bool DeleteUser(string username)
        {
            var user = GetUser(username);
            if (user == null) return false;

            _userSettings.Users.Remove(user);
            SaveToFile(_userSettingsPath, _userSettings);
            return true;
        }

        public User? ValidateUser(string username, string password)
        {
            return _userSettings.Users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        // ========== 資料夾操作 ==========
        public List<SharedFolder> GetFolders() => _folderSettings.SharedFolders;

        public SharedFolder? GetFolderByName(string name) =>
            _folderSettings.SharedFolders.FirstOrDefault(f =>
                f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public void AddFolder(SharedFolder folder)
        {
            _folderSettings.SharedFolders.Add(folder);
            SaveToFile(_folderSettingsPath, _folderSettings);
        }

        public bool UpdateFolderAccessList(string folderName, List<FolderAccess> newAccessList)
        {
            var folder = _folderSettings.SharedFolders.FirstOrDefault(f =>
                f.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));

            if (folder == null) return false;

            folder.AccessList = newAccessList;

            SaveToFile(_folderSettingsPath, _folderSettings);

            return true;
        }

        public bool DeleteFolder(string folderName)
        {
            var folder = GetFolderByName(folderName);
            if (folder == null) return false;

            _folderSettings.SharedFolders.Remove(folder);
            SaveToFile(_folderSettingsPath, _folderSettings);
            return true;
        }

        public List<FolderAccessViewModel> GetAccessibleFolders(string username, int permissionLevel)
        {
            var folders = GetFolders();

            if (permissionLevel == (int)PermissionLevel.Admin)
            {
                return folders.Select(f => new FolderAccessViewModel
                {
                    Name = f.Name,
                    Path = f.Path,
                    Permission = PermissionLevel.Admin  // ✅ 修正：保留 Admin 權限，不要硬塞 FullAccess
                }).ToList();
            }

            return folders
                .Select(f =>
                {
                    var access = f.AccessList.FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                    if (access != null && access.Permission != PermissionLevel.None)
                    {
                        return new FolderAccessViewModel
                        {
                            Name = f.Name,
                            Path = f.Path,
                            Permission = access.Permission
                        };
                    }
                    return null;
                })
                .Where(f => f != null)
                .ToList()!;
        }

    }
}