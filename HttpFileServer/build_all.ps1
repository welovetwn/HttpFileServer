# C:\Projects\HttpFileServer\build_all.ps1
# 一鍵發行、部署：只複製 HttpFileServer.exe（無打包 7z、自解壓等）

$projectPath   = Split-Path -Parent $MyInvocation.MyCommand.Definition
$publishDir    = "$projectPath\_publish_tmp"
$releaseDir    = "$projectPath\release"
$exeName       = "HttpFileServer.exe"
$projectFile   = "$projectPath\HttpFileServer.csproj"
$runtime       = "win-x64"
$configuration = "Release"
$finalExePath  = "$publishDir\$exeName"
$releaseExePath = "$releaseDir\$exeName"

# --- 驗證必要檔案 ---
if (!(Test-Path $projectFile)) {
    Write-Error "❌ 找不到 $projectFile"
    exit 1
}

# --- 終止舊程式並清理 release ---
if (Test-Path $releaseDir) {
    $exePath = Join-Path $releaseDir $exeName
    $runningProcs = Get-Process -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exePath }
    foreach ($proc in $runningProcs) {
        try { Stop-Process -Id $proc.Id -Force } catch {}
    }
    Start-Sleep -Milliseconds 500
    try { Remove-Item -Recurse -Force $releaseDir } catch {
        Write-Error "❌ 無法刪除 release 目錄：$($_.Exception.Message)"
        exit 1
    }
}
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null

# --- 發行 .exe ---
dotnet publish "$projectFile" `
    -c $configuration `
    -r $runtime `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeAllContentForSelfExtract=true `
    -o $publishDir

if ($LASTEXITCODE -ne 0 -or !(Test-Path $finalExePath)) {
    Write-Error "❌ 發行失敗或找不到可執行檔 $exeName"
    exit 1
}

# --- 複製 .exe ---
Copy-Item -Path $finalExePath -Destination $releaseExePath -Force

# --- 完成 ---
Start-Process explorer.exe $releaseDir
Write-Host "`n✅ 已完成發行並部署：" -ForegroundColor Green
Write-Host "👉 $releaseExePath" -ForegroundColor Yellow
