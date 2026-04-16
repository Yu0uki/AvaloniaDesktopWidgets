# Simple test runner with clear output
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AvaloniaApplication2 Test Report" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$startTime = Get-Date

Write-Host "Running tests..." -ForegroundColor Yellow
Write-Host ""

# Run tests and capture output
dotnet test --logger "console;verbosity=normal" 2>&1 | Tee-Object -Variable testOutput

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Duration: $([math]::Round($duration, 2)) seconds" -ForegroundColor Cyan
Write-Host ""
