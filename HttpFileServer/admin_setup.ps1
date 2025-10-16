
# \HttpFileServer\admin_setup.ps1
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# 建立表單
$form = New-Object System.Windows.Forms.Form
$form.Text = 'Admin 管理者初始設定'
$form.Width = 420
$form.Height = 250
$form.StartPosition = 'CenterScreen'
$form.Font = New-Object System.Drawing.Font("Segoe UI", 12)
$form.KeyPreview = $true

# 定義共用的 KeyDown 處理器：Enter 鍵當作 Tab 鍵
$handleEnterAsTab = {
    if ($_.KeyCode -eq 'Enter') {
        $_.SuppressKeyPress = $true  # 防止叮咚聲
        $form.SelectNextControl($form.ActiveControl, $true, $true, $true, $false)
    }
}

# Admin Username Label
$labelUser = New-Object System.Windows.Forms.Label
$labelUser.Text = '管理者帳號:'
$labelUser.Top = 30
$labelUser.Left = 20
$labelUser.Width = 150
$form.Controls.Add($labelUser)

# Admin Username TextBox
$textUser = New-Object System.Windows.Forms.TextBox
$textUser.Top = 25
$textUser.Left = 180
$textUser.Width = 200
$textUser.Font = $form.Font
$textUser.Add_KeyDown($handleEnterAsTab)
$form.Controls.Add($textUser)

# Admin Password Label
$labelPass = New-Object System.Windows.Forms.Label
$labelPass.Text = '管理者密碼:'
$labelPass.Top = 80
$labelPass.Left = 20
$labelPass.Width = 150
$form.Controls.Add($labelPass)

# Admin Password TextBox
$textPass = New-Object System.Windows.Forms.TextBox
$textPass.Top = 75
$textPass.Left = 180
$textPass.Width = 200
$textPass.UseSystemPasswordChar = $true
$textPass.Font = $form.Font
$textPass.Add_KeyDown($handleEnterAsTab)
$form.Controls.Add($textPass)

# 建立按鈕
$btnOK = New-Object System.Windows.Forms.Button
$btnOK.Text = '建立'
$btnOK.Top = 130
$btnOK.Left = 180
$btnOK.Width = 100
$btnOK.Height = 40
$btnOK.Font = $form.Font
$btnOK.Add_KeyDown($handleEnterAsTab)
$form.Controls.Add($btnOK)

# Click 行為
$btnOK.Add_Click({
    if (-not $textUser.Text -or -not $textPass.Text) {
        [System.Windows.Forms.MessageBox]::Show('請輸入帳號與密碼', '錯誤', 'OK', 'Error')
        return
    }

    $json = @{
        Users = @(
            @{
                Username   = $textUser.Text
                Password   = $textPass.Text
                Role       = 'Admin'
                Permission = ''
            }
        )
    } | ConvertTo-Json -Depth 3

    Set-Content -Path 'user_settings.json' -Value $json -Encoding UTF8
    [System.Windows.Forms.MessageBox]::Show("按【確定】鍵,程式將自動重新執行！", "✅ 帳號設定完成！", "OK", "Information")
    $form.Close()
})

$form.Topmost = $true
[void]$form.ShowDialog()
# === 關閉對話框後：延遲3秒並啟動 HttpFileServer.exe ===
Start-Sleep -Seconds 3

$exePath = Join-Path -Path $PSScriptRoot -ChildPath "HttpFileServer.exe"
if (Test-Path $exePath) {
    Start-Process -FilePath $exePath
} else {
    [System.Windows.Forms.MessageBox]::Show("⚠ 找不到 HttpFileServer.exe", "啟動失敗", "OK", "Warning")
}
