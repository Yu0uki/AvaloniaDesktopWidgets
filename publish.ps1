$version = "4.0.1"
$output = "publish/release-$version"
$project = "AvaloniaApplication2/AvaloniaApplication2.csproj"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  效率工坊 Release $version 打包脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`n[1/5] 清理旧发布..." -ForegroundColor Yellow
Remove-Item -Recurse -Force $output -ErrorAction SilentlyContinue

Write-Host "[2/5] dotnet publish (self-contained win-x64)..." -ForegroundColor Yellow
dotnet publish $project -c Release -r win-x64 --self-contained true -p:Version=$version -o $output
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ERROR: 发布失败" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "[3/5] 创建目录结构..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "$output/Plugins", "$output/Data", "$output/Logs" | Out-Null

Write-Host "[4/5] 复制插件 DLL..." -ForegroundColor Yellow
$plugins = Get-ChildItem "Plugins/*.dll" -ErrorAction SilentlyContinue
foreach ($p in $plugins) {
    Copy-Item $p.FullName -Destination "$output/Plugins/" -Force
    Write-Host "  + $($p.Name)"
}
if ($plugins.Count -eq 0) { Write-Host "  (无插件)" }

Write-Host "[5/5] 清理调试符号..." -ForegroundColor Yellow
$pdbs = Get-ChildItem "$output/*.pdb" -ErrorAction SilentlyContinue
Remove-Item $pdbs -ErrorAction SilentlyContinue

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  打包完成!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "  输出: $output"
Write-Host "  大小: $([math]::Round((Get-ChildItem $output -Recurse | Measure-Object Length -Sum).Sum / 1MB, 1)) MB"
Write-Host "  EXE : $output/AvaloniaApplication2.exe"
