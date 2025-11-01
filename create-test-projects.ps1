# Script to create test projects for all microservices

$services = @(
    @{Name="ProductService"; Path="product-service/src"},
    @{Name="BookService"; Path="book-service/src"},
    @{Name="ReservationService"; Path="reservation-service/src"},
    @{Name="CartService"; Path="cart-service/src"},
    @{Name="OrderService"; Path="OrderService"},
    @{Name="AnalyticsService"; Path="AnalyticsService"}
)

$rootPath = "C:\Users\asologan\OneDrive - ENDAVA\Desktop\Workspace\FeaneShop\services"

foreach ($service in $services) {
    $serviceName = $service.Name
    $servicePath = $service.Path
    $testProjectName = "$serviceName.Tests"
    
    Write-Host "Creating test project for $serviceName..." -ForegroundColor Green
    
    # Navigate to service directory
    $serviceDir = Join-Path $rootPath (Split-Path $servicePath -Parent)
    Set-Location $serviceDir
    
    # Create test project
    dotnet new xunit -n $testProjectName
    
    # Add reference and packages
    Set-Location $testProjectName
    dotnet add reference "..\$(Split-Path $servicePath -Leaf)\$serviceName.csproj"
    dotnet add package Moq
    dotnet add package FluentAssertions
    dotnet add package Microsoft.EntityFrameworkCore.InMemory
    
    Write-Host "Test project for $serviceName created successfully!" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "All test projects created!" -ForegroundColor Yellow

