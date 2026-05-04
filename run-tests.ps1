# Simple and clear test runner
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Running Tests" -ForegroundColor Cyan  
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$startTime = Get-Date

# Run tests directly - let dotnet handle the output
dotnet test AvaloniaApplication2.Tests --logger "console;verbosity=normal"
$exitCode = $LASTEXITCODE

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Duration: $([math]::Round($duration, 2)) seconds" -ForegroundColor Yellow
Write-Host ""

exit $exitCode
