# Test Refresh Token Functionality

$gatewayUrl = "http://localhost:5000"
$apiBase = "$gatewayUrl/api/auth"

Write-Host "=== Testing Refresh Token Functionality ===" -ForegroundColor Cyan

# Step 1: Login
Write-Host "`n1. Logging in..." -ForegroundColor Yellow
$loginBody = @{
    credential = "admin"
    password = "Admin123!"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$apiBase/login" -Method Post -Body $loginBody -ContentType "application/json" -ErrorAction Stop
    
    $token = $loginResponse.token
    $refreshToken = $loginResponse.refreshToken
    
    Write-Host "Login successful!" -ForegroundColor Green
    Write-Host "Access Token: $($token.Substring(0, [Math]::Min(50, $token.Length)))..." -ForegroundColor Gray
    Write-Host "Refresh Token: $($refreshToken.Substring(0, [Math]::Min(50, $refreshToken.Length)))..." -ForegroundColor Gray
    
    # Step 2: Test Refresh Token
    Write-Host "`n2. Testing refresh token..." -ForegroundColor Yellow
    
    $refreshBody = @{
        refreshToken = $refreshToken
    } | ConvertTo-Json
    
    Write-Host "Sending refresh request with body: $refreshBody" -ForegroundColor Gray
    
    $refreshResponse = Invoke-RestMethod -Uri "$apiBase/refresh" -Method Post -Body $refreshBody -ContentType "application/json" -ErrorAction Stop
    
    $newToken = $refreshResponse.token
    $newRefreshToken = $refreshResponse.refreshToken
    
    Write-Host "Refresh successful!" -ForegroundColor Green
    Write-Host "New Access Token: $($newToken.Substring(0, [Math]::Min(50, $newToken.Length)))..." -ForegroundColor Gray
    Write-Host "New Refresh Token: $($newRefreshToken.Substring(0, [Math]::Min(50, $newRefreshToken.Length)))..." -ForegroundColor Gray
    
    # Step 3: Test with new token
    Write-Host "`n3. Testing profile endpoint with new token..." -ForegroundColor Yellow
    
    $headers = @{
        Authorization = "Bearer $newToken"
    }
    
    $profileResponse = Invoke-RestMethod -Uri "$apiBase/profile" -Method Get -Headers $headers -ErrorAction Stop
    
    Write-Host "Profile request successful!" -ForegroundColor Green
    Write-Host "User: $($profileResponse.user.username)" -ForegroundColor Gray
    
    Write-Host "`n=== All tests passed! ===" -ForegroundColor Green
    
} catch {
    Write-Host "`nError occurred:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.ErrorDetails.Message) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
    
    Write-Host "`nFull error:" -ForegroundColor Red
    Write-Host $_ -ForegroundColor Red
}

