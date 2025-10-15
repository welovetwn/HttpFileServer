# C:\Projects\HttpFileServer\publish_net8.ps1
# PowerShell script - 使用 .NET 發行為 self-contained 單一 exe

$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$publishDir = "$projectPath\publish"
$runtime = "win-x64"
$configuration = "Release"
$zipFile = "$projectPath\HttpFileServer_publish.zip"
$projectFile = "$projectPath\HttpFileServer.csproj"

Write-Host "🔧 正在使用 .NET 發行 HttpFileServer 專案..."

# 確認 csproj 是否存在
if (!(Test-Path $projectFile)) {
    Write-Error "❌ 找不到 $projectFile，請確保此 script 放在專案根目錄。"
    exit 1
}

# 清空原有 publish 資料夾
if (Test-Path $publishDir) {
    Remove-Item -Recurse -Force -Path $publishDir
}

# 執行 dotnet publish
dotnet publish "$projectFile" `
    -c $configuration `
    -r $runtime `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeAllContentForSelfExtract=true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ dotnet publish 發行失敗"
    exit 1
}

# 壓縮 zip（可選）
if (Test-Path $zipFile) {
    Remove-Item -Force $zipFile
}
Compress-Archive -Path "$publishDir\*" -DestinationPath $zipFile

Write-Host "`n✅ 發行完成！輸出資料夾："
Write-Host "   $publishDir"
Write-Host "`n📦 已打包為：$zipFile"
Write-Host "`n📂 開啟輸出資料夾..."

Start-Process explorer.exe "$publishDir"
