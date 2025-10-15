# C:\Projects\HttpFileServer\pack_to_portable.ps1
# 功能：將 deliver 資料夾打包為自解壓執行檔 HttpFileServer_Portable.exe（使用 7z.sfx）

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$deliverDir = Join-Path $projectRoot "deliver"
$portableDir = Join-Path $projectRoot "portable"
$temp7z = Join-Path $projectRoot "deliver.7z"
$configFile = Join-Path $projectRoot "config.txt"
$outputFile = Join-Path $portableDir "HttpFileServer_Portable.exe"

# 系統安裝的 7-Zip 工具
$sevenZip = "C:\Program Files\7-Zip\7z.exe"
$sfxModule = "C:\Program Files\7-Zip\7z.sfx"

# 驗證工具存在
if (!(Test-Path $sevenZip)) {
    Write-Error "❌ 找不到 7z.exe：$sevenZip"
    exit 1
}
if (!(Test-Path $sfxModule)) {
    Write-Error "❌ 找不到 7z.sfx：$sfxModule"
    exit 1
}
if (!(Test-Path $deliverDir)) {
    Write-Error "❌ 找不到 deliver 資料夾，請先執行 publish_and_copy.ps1"
    exit 1
}

# 建立 portable 輸出資料夾
if (!(Test-Path $portableDir)) {
    New-Item -ItemType Directory -Path $portableDir | Out-Null
}

# 建立 config.txt（如尚未存在）
if (!(Test-Path $configFile)) {
    Set-Content -Encoding UTF8 -Path $configFile @"
;!@Install@!UTF-8!
Title="HttpFileServer Portable"
RunProgram="HttpFileServer.exe"
GUIMode="1"
;!@InstallEnd@!
"@
}

# 壓縮 deliver 為 deliver.7z
& "$sevenZip" a -r -t7z "`"$temp7z`"" "$deliverDir\*" | Out-Null

# 合併為 HttpFileServer_Portable.exe
$parts = @($sfxModule, $configFile, $temp7z)
$fsOut = [System.IO.File]::Create($outputFile)
foreach ($part in $parts) {
    $bytes = [System.IO.File]::ReadAllBytes($part)
    $fsOut.Write($bytes, 0, $bytes.Length)
}
$fsOut.Close()

# 刪除暫時壓縮檔
Remove-Item $temp7z -Force

# 清除 config.txt（若你想保留，可註解掉）
Remove-Item $configFile -Force

Write-Host "`n✅ 打包完成：" -ForegroundColor Green
Write-Host "👉 $outputFile" -ForegroundColor Yellow