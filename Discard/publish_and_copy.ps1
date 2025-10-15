
# C:\Projects\HttpFileServer\publish_and_copy.ps1
# 功能：發行 HttpFileServer 為 .exe，並自動部署到 deliver（包含設定檔 + 靜態資源 + 自動殺掉執行中的舊程式）

$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$publishDir = "$projectPath\_publish_tmp"
$deliverDir = "$projectPath\deliver"
$runtime = "win-x64"
$configuration = "Release"
$projectFile = "$projectPath\HttpFileServer.csproj"
$exeName = "HttpFileServer.exe"
$finalExePath = "$publishDir\$exeName"
$deliverExePath = "$deliverDir\$exeName"

Write-Host "📦 開始發行 HttpFileServer..."

# 驗證專案檔存在
if (!(Test-Path $projectFile)) {
    Write-Error "❌ 找不到 $projectFile，請確認你在專案根目錄中。"
    exit 1
}

# 安全清除 deliver 目錄，處理檔案被鎖定的情況
if (Test-Path $deliverDir) {
    $exePath = Join-Path $deliverDir $exeName

    # 尋找正在執行的 HttpFileServer.exe
    $runningProcs = Get-Process -ErrorAction SilentlyContinue | Where-Object {
        $_.Path -eq $exePath
    }

    if ($runningProcs.Count -gt 0) {
        Write-Warning "⚠️ 偵測到已有執行中的 $exeName，將自動終止它..."

        foreach ($proc in $runningProcs) {
            try {
                Stop-Process -Id $proc.Id -Force
                Write-Host "✅ 已終止 PID $($proc.Id)"
            } catch {
                Write-Warning "❌ 無法終止 PID $($proc.Id): $_"
            }
        }

        Start-Sleep -Seconds 1 # 短暫等待資源釋放
    }

    # 確保已釋放檔案，才能安全刪除
    try {
        Remove-Item -Recurse -Force $deliverDir
    } catch {
        Write-Error "❌ 無法刪除 deliver 資料夾：$($_.Exception.Message)"
        exit 1
    }
}

# 建立新的發行資料夾（如果尚未存在）
if (!(Test-Path $publishDir)) {
    New-Item -ItemType Directory -Path $publishDir | Out-Null
}
if (!(Test-Path $deliverDir)) {
    New-Item -ItemType Directory -Path $deliverDir | Out-Null
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
    Write-Error "❌ 發行失敗，請檢查錯誤訊息。"
    exit 1
}

# 檢查發行成功與否
if (!(Test-Path $finalExePath)) {
    Write-Error "❌ 未找到產生的 $exeName，請確認發行是否成功。"
    exit 1
}

# 複製 .exe 到 deliver
Copy-Item -Path $finalExePath -Destination $deliverExePath

# 複製必要設定檔
$settings = @("user_settings.json", "folder_settings.json")
foreach ($f in $settings) {
    $src = Join-Path $projectPath $f
    $dst = Join-Path $deliverDir $f
    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $dst
    } else {
        Write-Warning "⚠️ 缺少設定檔：$f"
    }
}

# ✅ 自動判斷並複製 wwwroot（靜態檔）
$wwwrootSrc = Join-Path $projectPath "wwwroot"
$wwwrootDst = Join-Path $deliverDir "wwwroot"
if (Test-Path $wwwrootSrc) {
    Write-Host "📁 偵測到 wwwroot，自動部署靜態檔..."
    Copy-Item -Recurse -Force -Path $wwwrootSrc -Destination $wwwrootDst
}

Write-Host ""
Write-Host "✅ 發行完成！已複製至交付資料夾："
Write-Host "   $deliverDir"
Write-Host ""

Start-Process explorer.exe $deliverDir