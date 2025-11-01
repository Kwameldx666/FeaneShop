# Script to fix project references for test projects

$rootPath = "C:\Users\asologan\OneDrive - ENDAVA\Desktop\Workspace\FeaneShop\services"

# ProductService
Write-Host "Fixing ProductService references..." -ForegroundColor Yellow
Set-Location "$rootPath\product-service\ProductService.Tests"
dotnet remove reference ..\src\ProductService.csproj 2>$null
dotnet add reference ..\src\ProductService\ProductService.csproj

# BookService
Write-Host "Fixing BookService references..." -ForegroundColor Yellow
Set-Location "$rootPath\book-service\BookService.Tests"
dotnet remove reference ..\src\BookService.csproj 2>$null
dotnet add reference ..\src\BookService\BookService.csproj

# ReservationService
Write-Host "Fixing ReservationService references..." -ForegroundColor Yellow
Set-Location "$rootPath\reservation-service\ReservationService.Tests"
dotnet remove reference ..\src\ReservationService.csproj 2>$null
dotnet add reference ..\src\ReservationService\ReservationService.csproj

# CartService
Write-Host "Fixing CartService references..." -ForegroundColor Yellow
Set-Location "$rootPath\cart-service\CartService.Tests"
dotnet remove reference ..\src\CartService.csproj 2>$null
dotnet add reference ..\src\CartService\CartService.csproj

Write-Host "All references fixed!" -ForegroundColor Green

