# HttpFileServer

## 🧩 ToDo Issue
- 其他的有空再想

一個基於 .NET 的簡易 HTTP 檔案伺服器 / 共用平台，支援權限管理、靜態檔案分享、管理界面與伺服器關閉功能。

## 🧩 功能特色

- 支援多使用者帳號管理，根據不同權限層級存取資料夾
- 管理介面可新增/編輯使用者、設定資料夾授權
- 靜態資源（wwwroot）支援
- 支援透過 Web UI 安全關閉整個伺服器程式
- 自動綁定所有 IP（0.0.0.0），使局網中的其他裝置可透過 IP 存取
- 可做為單檔 Self-contained .exe 發行（無需安裝 .NET Runtime）

## ⚙ 架構說明

| 元件 | 說明 |
|---|---|
| `ConfigService` | 負責讀取 / 儲存 `user_settings.json`、`folder_settings.json` |
| `User` 模型 | 含 `Username`、`Password`、`Permission` 欄位（字串型態） |
| 權限判斷 | 使用者登入時將 `Permission` 放入 Claim（`PermissionLevel` Claim） |
| `AdminController` | 管理端 UI／API，允許擁有 Admin 權限的使用者操作 |
| `DashboardController` | 使用者登入後進入的資料夾總覽頁 |
| Shutdown API | Admin 可經由 `/admin/shutdown` 呼叫，以程式方式關閉伺服器 |
| `DebugController` | 用於顯示目前使用者的 Claims（開發 / 偵錯用途） |

## 📦 安裝 / 發行與部署

### 1. 專案發行

```powershell
dotnet publish HttpFileServer.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeAllContentForSelfExtract=true `
    -o ./publish
```

### 2. 一鍵發行

執行 
```powershell
run_gui.bat
```

### 3. 啟動服務

```powershell
.\HttpFileServer.exe
```

### 4. 管理員登入

初次啟動若 `user_settings.json` 不存在，系統會要求輸入 admin 資訊後建立設定檔。

## 🔐 權限設計

- `Permission` 為字串型態，例如 "99" 表示最高權限
- 登入後系統會將 `Permission` 加入使用者 Claims 中的 `PermissionLevel` 欄位
- 控制器與頁面使用 Claim 判斷使用者權限層級是否足夠

## 🧪 偵錯：檢視 Claims

登入後可使用以下網址檢查目前使用者的 Claim 資訊：

```
/debug/claims
```

## 📋 設定檔範例

### `user_settings.json`
```json
{
  "Users": [
    {
      "Username": "admin",
      "Password": "0000",
      "Role": "Admin",
      "Permission": "99"
    }
  ]
}
```

### `folder_settings.json`
```json
{
  "SharedFolders": [
    {
      "Name": "Public",
      "Path": "D:\\Shared\\Public",
      "AccessList": [
        { "Username": "admin", "Permission": "99" },
        { "Username": "guest", "Permission": "1" }
      ]
    }
  ]
}
```

## 🛠 可擴充功能

- 使用密碼雜湊儲存
- 加入 HTTPS 支援
- 操作紀錄（Log）
- 檔案上傳功能
- 檔案搜尋與篩選功能

