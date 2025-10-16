// HttpFileServer.Tests/TestUtilities/FileSystemHelper.cs
namespace HttpFileServer.Tests.TestUtilities;

public class FileSystemHelper : IDisposable
{
    private readonly List<string> _createdDirectories = new();
    private readonly List<string> _createdFiles = new();

    public string CreateTestDirectory(string? basePath = null)
    {
		var path = basePath ?? Path.Combine(Path.GetTempPath(), $"HttpFileServerTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(path);
        _createdDirectories.Add(path);
        return path;
    }

    public string CreateTestFile(string directory, string fileName, string content = "test content")
    {
        var filePath = Path.Combine(directory, fileName);
        File.WriteAllText(filePath, content);
        _createdFiles.Add(filePath);
        return filePath;
    }

    public void CreateTestFiles(string directory, params string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            CreateTestFile(directory, fileName);
        }
    }

    public void Dispose()
    {
        // 清理測試產生的檔案
        foreach (var file in _createdFiles)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch { /* 忽略清理錯誤 */ }
        }

        // 清理測試產生的目錄
        foreach (var directory in _createdDirectories)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch { /* 忽略清理錯誤 */ }
        }
    }
}