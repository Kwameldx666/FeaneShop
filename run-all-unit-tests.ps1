# Test All Projects - Comprehensive Test Suite

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Running All Unit Tests" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$testProjects = @(
    "services\gateway\FeaneGateway.Tests\FeaneGateway.Tests.csproj",
    "services\user-service\UserService.Tests\UserService.Tests.csproj",
    "services\OrderService.Tests\OrderService.Tests.csproj",
    "services\product-service\ProductService.Tests\ProductService.Tests.csproj",
    "services\reservation-service\ReservationService.Tests\ReservationService.Tests.csproj",
    "services\cart-service\CartService.Tests\CartService.Tests.csproj",
    "services\book-service\BookService.Tests\BookService.Tests.csproj",
    "services\AnalyticsService.Tests\AnalyticsService.Tests.csproj"
)

$totalPassed = 0
$totalFailed = 0
$totalSkipped = 0
$projectResults = @()

foreach ($project in $testProjects) {
    $projectPath = Join-Path $PSScriptRoot $project
    
    if (Test-Path $projectPath) {
        $projectName = Split-Path $project -Leaf
        Write-Host "Testing: $projectName" -ForegroundColor Yellow
        Write-Host "Path: $project" -ForegroundColor Gray
        
        $output = dotnet test $projectPath --verbosity quiet --nologo 2>&1 | Out-String
        
        # Parse results
        if ($output -match "Passed!\s+-\s+Failed:\s+(\d+),\s+Passed:\s+(\d+),\s+Skipped:\s+(\d+)") {
            $failed = [int]$matches[1]
            $passed = [int]$matches[2]
            $skipped = [int]$matches[3]
            
            $totalPassed += $passed
            $totalFailed += $failed
            $totalSkipped += $skipped
            
            $status = if ($failed -eq 0) { "✅ PASSED" } else { "❌ FAILED" }
            
            Write-Host "  Result: $status - Passed: $passed, Failed: $failed, Skipped: $skipped" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
            
            $projectResults += [PSCustomObject]@{
                Project = $projectName
                Passed = $passed
                Failed = $failed
                Skipped = $skipped
                Status = if ($failed -eq 0) { "PASSED" } else { "FAILED" }
            }
        }
        elseif ($output -match "Failed!\s+-\s+Failed:\s+(\d+),\s+Passed:\s+(\d+)") {
            $failed = [int]$matches[1]
            $passed = [int]$matches[2]
            $skipped = 0
            
            $totalPassed += $passed
            $totalFailed += $failed
            
            Write-Host "  Result: ❌ FAILED - Passed: $passed, Failed: $failed" -ForegroundColor Red
            
            $projectResults += [PSCustomObject]@{
                Project = $projectName
                Passed = $passed
                Failed = $failed
                Skipped = $skipped
                Status = "FAILED"
            }
        }
        else {
            Write-Host "  Result: ⚠️  Could not parse results" -ForegroundColor Yellow
            Write-Host "  Output: $output" -ForegroundColor Gray
            
            $projectResults += [PSCustomObject]@{
                Project = $projectName
                Passed = 0
                Failed = 0
                Skipped = 0
                Status = "UNKNOWN"
            }
        }
        
        Write-Host ""
    }
    else {
        Write-Host "⚠️  Project not found: $project" -ForegroundColor Yellow
        Write-Host ""
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Display summary table
Write-Host "Project Results:" -ForegroundColor White
Write-Host ""
$projectResults | Format-Table -AutoSize

Write-Host ""
Write-Host "Total Statistics:" -ForegroundColor White
Write-Host "  Total Passed:  $totalPassed" -ForegroundColor Green
Write-Host "  Total Failed:  $totalFailed" -ForegroundColor $(if ($totalFailed -eq 0) { "Green" } else { "Red" })
Write-Host "  Total Skipped: $totalSkipped" -ForegroundColor Yellow
Write-Host "  Total Tests:   $($totalPassed + $totalFailed + $totalSkipped)" -ForegroundColor Cyan
Write-Host ""

$successRate = if (($totalPassed + $totalFailed) -gt 0) { 
    [math]::Round(($totalPassed / ($totalPassed + $totalFailed)) * 100, 2) 
} else { 
    0 
}

Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 90) { "Green" } elseif ($successRate -ge 70) { "Yellow" } else { "Red" })
Write-Host ""

if ($totalFailed -eq 0) {
    Write-Host "🎉 All tests passed!" -ForegroundColor Green
}
else {
    Write-Host "⚠️  Some tests failed. Please review the results above." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

