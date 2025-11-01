$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjlmMmFlMTg2LWFjZmItNDYwNC1hYWNkLTZjNTdkYWVkZDVkNCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJhZG1pbjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJhZG1pbkBtYWlsLnJ1IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3NjIwMDkyNTQsImlzcyI6IkZlYW5lTVZDIiwiYXVkIjoiRmVhbmVNVkNVc2VycyJ9"
$headers = @{
    Authorization = "Bearer $token"
}

Write-Host "Testing /api/users endpoint to list all users..."
try {
    $result = Invoke-RestMethod -Uri "http://localhost:5000/api/users" -Method Get -Headers $headers
    Write-Host "Success!"
    Write-Host "Number of users: $($result.Count)"
    $result | ForEach-Object {
        Write-Host "---"
        Write-Host "ID: $($_.id)"
        Write-Host "Username: $($_.username)"
        Write-Host "Email: $($_.email)"
    }
} catch {
    Write-Host "Error: $_"
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response: $responseBody"
        $reader.Close()
    }
}

