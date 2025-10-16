# 整合測試說明

## 目前狀態

✅ **已完成**：
- 測試基礎設施（CustomWebApplicationFactory）
- 基本的應用程式啟動測試
- 效能測試框架

⚠️ **待完成**（需要實際 Controller 程式碼）：
- 完整的認證流程測試
- API 端點測試
- 使用者工作流程測試

## 下一步

請提供以下 Controller 的程式碼：
1. AccountController.cs - 處理登入/登出
2. AdminController.cs - 管理介面
3. DashboardController.cs - 使用者儀表板
4. FileController.cs - 檔案操作（如果有）

提供後，我會補充完整的整合測試。

## 執行測試
```powershell
# 只執行整合測試
dotnet test HttpFileServer.IntegrationTests/HttpFileServer.IntegrationTests.csproj

# 執行所有測試
dotnet test
```

## 測試原理

整合測試使用 WebApplicationFactory 建立測試用的應用程式實例：
- 使用臨時目錄作為測試環境
- 預先建立測試用的設定檔
- 在測試結束後自動清理