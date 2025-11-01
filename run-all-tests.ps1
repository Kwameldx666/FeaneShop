# Script to run all unit tests for microservices

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Running Unit Tests for All Services  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$testProjects = @(
    "services\user-service\UserService.Tests",
    "services\product-service\ProductService.Tests",
    "services\book-service\BookService.Tests",
    "services\reservation-service\ReservationService.Tests",
    "services\cart-service\CartService.Tests",
    "services\OrderService.Tests",
    "services\AnalyticsService.Tests"
)

$rootPath = "C:\Users\asologan\OneDrive - ENDAVA\Desktop\Workspace\FeaneShop"
$totalTests = 0
$passedTests = 0
$failedTests = 0

foreach ($project in $testProjects) {
    $projectPath = Join-Path $rootPath $project
    $serviceName = ($project -split '\\')[-1] -replace '.Tests', ''
    
    Write-Host "Running tests for $serviceName..." -ForegroundColor Yellow
    Write-Host "Path: $projectPath" -ForegroundColor Gray
    Write-Host ""
    
    if (Test-Path $projectPath) {
        Set-Location $projectPath
        
        # Run tests
        $output = dotnet test --verbosity normal 2>&1
        
        # Parse results
        if ($output -match "Passed!.*\s+(\d+) passed") {
            $passed = [int]$matches[1]
            $passedTests += $passed
            $totalTests += $passed
            Write-Host "✓ $serviceName: $passed tests passed" -ForegroundColor Green
        }
        elseif ($output -match "Failed!.*\s+(\d+) failed") {
            $failed = [int]$matches[1]
            $failedTests += $failed
            $totalTests += $failed
            Write-Host "✗ $serviceName: $failed tests failed" -ForegroundColor Red
        }
        else {
            Write-Host "⚠ $serviceName: No tests found or error occurred" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "⚠ Test project not found: $projectPath" -ForegroundColor Yellow
    }
    
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "          Test Summary                  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total Tests:  $totalTests" -ForegroundColor White
Write-Host "Passed:       $passedTests" -ForegroundColor Green
Write-Host "Failed:       $failedTests" -ForegroundColor Red
Write-Host ""

if ($failedTests -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
}
else {
    Write-Host "✗ Some tests failed. Please review the output above." -ForegroundColor Red
}

# Return to root
Set-Location $rootPath

