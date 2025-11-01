# Test script for user-service
$userId = "9f2ae186-acfb-4604-aacd-6c57daedd5d4"

Write-Host "Testing user-service endpoints..."
Write-Host ""

# Test 1: Get all users
Write-Host "1. Testing GET /api/users (all users)..."
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5020/api/users" -Method Get
    Write-Host "Success! Found $($response.Count) users"
    $response | ForEach-Object { Write-Host "  - User: $($_.username) ($($_.id))" }
} catch {
    Write-Host "Error: $_"
}

Write-Host ""

# Test 2: Get user by ID
Write-Host "2. Testing GET /api/users/$userId (specific user)..."
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5020/api/users/$userId" -Method Get
    Write-Host "Success!"
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Error: $_"
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
        $reader.Close()
    }
}

Write-Host ""

# Test 3: Through gateway
Write-Host "3. Testing GET through gateway (http://localhost:5000/api/users/$userId)..."
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/users/$userId" -Method Get
    Write-Host "Success!"
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Error: $_"
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
        $reader.Close()
    }
}

