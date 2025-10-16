# C:\Projects\HttpFileServer\gui_wrapper.ps1
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$form = New-Object Windows.Forms.Form
$form.Text = "HttpFileServer 發行器"
$form.Size = New-Object Drawing.Size(400,200)
$form.StartPosition = "CenterScreen"

$button = New-Object Windows.Forms.Button
$button.Text = "一鍵發行"
$button.Size = New-Object Drawing.Size(200,40)
$button.Location = New-Object Drawing.Point(100,50)
$button.Font = New-Object Drawing.Font("Microsoft JhengHei",12)

$button.Add_Click({
    $psScript = Join-Path $PSScriptRoot "build_all.ps1"
    if (Test-Path $psScript) {
        Start-Process powershell.exe "-ExecutionPolicy Bypass -File `"$psScript`""
    } else {
        [Windows.Forms.MessageBox]::Show("找不到 build_all.ps1", "錯誤", 0, 16)
    }
})

$form.Controls.Add($button)
$form.Topmost = $true
$form.ShowDialog()
