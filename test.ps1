# Test Runner - Displays test results clearly
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Running Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$startTime = Get-Date

# Run tests and capture output
$output = dotnet test AvaloniaApplication2.Tests --logger "console;verbosity=normal" 2>&1

# Display the output
$output | ForEach-Object { Write-Host $_ }

$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

# Parse test results
$totalTests = 0
$passedTests = 0
$failedTests = 0

foreach ($line in $output) {
    if ($line -match "测试总数[^\d]*(\d+)") {
        $totalTests = [int]$matches[1]
    }
    elseif ($line -match "通过数[^\d]*(\d+)") {
        $passedTests = [int]$matches[1]
    }
    elseif ($line -match "失败数[^\d]*(\d+)") {
        $failedTests = [int]$matches[1]
    }
}

# Calculate pass rate
if ($totalTests -gt 0) {
    $passRate = [math]::Round(($passedTests / $totalTests) * 100, 2)
} else {
    $passRate = 0
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total Tests:   $totalTests" -ForegroundColor White
Write-Host "Passed:        $passedTests" -ForegroundColor Green

if ($failedTests -eq 0) {
    Write-Host "Failed:        $failedTests" -ForegroundColor Green
} else {
    Write-Host "Failed:        $failedTests" -ForegroundColor Red
}

if ($passRate -ge 90) {
    Write-Host "Pass Rate:     ${passRate}%" -ForegroundColor Green
} elseif ($passRate -ge 70) {
    Write-Host "Pass Rate:     ${passRate}%" -ForegroundColor Yellow
} else {
    Write-Host "Pass Rate:     ${passRate}%" -ForegroundColor Red
}

Write-Host "Duration:      $([math]::Round($duration, 2)) seconds" -ForegroundColor Cyan
Write-Host ""

if ($failedTests -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "⚠️  $failedTests test(s) failed" -ForegroundColor Yellow
}

Write-Host ""
