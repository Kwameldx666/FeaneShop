# Test Order Service API
# Make sure the gateway is running on localhost:5000

$gatewayUrl = "http://localhost:5000"
$token = ""  # Add your JWT token here

Write-Host "=== Order Service Test Script ===" -ForegroundColor Cyan
Write-Host ""

# Function to make authenticated requests
function Invoke-AuthRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null
    )
    
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    
    try {
        if ($Body) {
            $response = Invoke-RestMethod -Method $Method -Uri $Uri -Headers $headers -Body ($Body | ConvertTo-Json -Depth 10)
        } else {
            $response = Invoke-RestMethod -Method $Method -Uri $Uri -Headers $headers
        }
        return $response
    }
    catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $reader.BaseStream.Position = 0
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response: $responseBody" -ForegroundColor Yellow
        }
        return $null
    }
}

# Check if token is set
if ([string]::IsNullOrEmpty($token)) {
    Write-Host "WARNING: JWT token is not set. Please update the script with a valid token." -ForegroundColor Yellow
    Write-Host "To get a token, login through the gateway authentication endpoint." -ForegroundColor Yellow
    Write-Host ""
    exit
}

# Test 1: Get all orders (should be empty initially)
Write-Host "Test 1: Getting all orders..." -ForegroundColor Green
$orders = Invoke-AuthRequest -Method GET -Uri "$gatewayUrl/api/orders"
if ($orders) {
    Write-Host "Success! Found $($orders.orders.Count) orders" -ForegroundColor Green
    $orders | ConvertTo-Json -Depth 5
} else {
    Write-Host "Failed to get orders" -ForegroundColor Red
}
Write-Host ""

# Test 2: Create a new order
Write-Host "Test 2: Creating a new order..." -ForegroundColor Green
$newOrder = @{
    deliveryAddress = "123 Test Street, Test City, 12345"
    notes = "Test order - Please handle with care"
    items = @(
        @{
            productId = "11111111-1111-1111-1111-111111111111"
            productName = "Test Pizza Margherita"
            productImageUrl = "/images/pizza.jpg"
            unitPrice = 12.99
            quantity = 2
            notes = "Extra cheese"
        },
        @{
            productId = "22222222-2222-2222-2222-222222222222"
            productName = "Test Caesar Salad"
            productImageUrl = "/images/salad.jpg"
            unitPrice = 8.50
            quantity = 1
        }
    )
}

$createdOrder = Invoke-AuthRequest -Method POST -Uri "$gatewayUrl/api/orders" -Body $newOrder
if ($createdOrder -and $createdOrder.success) {
    Write-Host "Success! Order created with ID: $($createdOrder.order.id)" -ForegroundColor Green
    Write-Host "Total Amount: $($createdOrder.order.totalAmount)" -ForegroundColor Cyan
    $orderId = $createdOrder.order.id
    $createdOrder | ConvertTo-Json -Depth 5
} else {
    Write-Host "Failed to create order" -ForegroundColor Red
    exit
}
Write-Host ""

# Test 3: Get order by ID
Write-Host "Test 3: Getting order by ID..." -ForegroundColor Green
$order = Invoke-AuthRequest -Method GET -Uri "$gatewayUrl/api/orders/$orderId"
if ($order -and $order.success) {
    Write-Host "Success! Retrieved order: $($order.order.id)" -ForegroundColor Green
    Write-Host "Status: $($order.order.status)" -ForegroundColor Cyan
    Write-Host "Items: $($order.order.items.Count)" -ForegroundColor Cyan
    $order | ConvertTo-Json -Depth 5
} else {
    Write-Host "Failed to get order by ID" -ForegroundColor Red
}
Write-Host ""

# Test 4: Get all orders again (should show the created order)
Write-Host "Test 4: Getting all orders again..." -ForegroundColor Green
$allOrders = Invoke-AuthRequest -Method GET -Uri "$gatewayUrl/api/orders"
if ($allOrders) {
    Write-Host "Success! Found $($allOrders.orders.Count) orders" -ForegroundColor Green
    foreach ($o in $allOrders.orders) {
        Write-Host "  - Order $($o.id.Substring(0,8)): $($o.status) - $$($o.totalAmount)" -ForegroundColor Cyan
    }
} else {
    Write-Host "Failed to get orders" -ForegroundColor Red
}
Write-Host ""

# Test 5: Update order status (if you have admin privileges)
Write-Host "Test 5: Updating order status..." -ForegroundColor Green
$statusUpdate = @{
    status = "Confirmed"
}
$updated = Invoke-AuthRequest -Method PATCH -Uri "$gatewayUrl/api/orders/$orderId/status" -Body $statusUpdate
if ($updated -and $updated.success) {
    Write-Host "Success! Order status updated to Confirmed" -ForegroundColor Green
} else {
    Write-Host "Note: Status update may require admin privileges" -ForegroundColor Yellow
}
Write-Host ""

# Test 6: Cancel order
Write-Host "Test 6: Cancelling order..." -ForegroundColor Green
$cancelled = Invoke-AuthRequest -Method DELETE -Uri "$gatewayUrl/api/orders/$orderId"
if ($cancelled -and $cancelled.success) {
    Write-Host "Success! Order cancelled" -ForegroundColor Green
} else {
    Write-Host "Failed to cancel order" -ForegroundColor Red
}
Write-Host ""

# Final verification
Write-Host "Test 7: Verifying order is cancelled..." -ForegroundColor Green
$finalOrder = Invoke-AuthRequest -Method GET -Uri "$gatewayUrl/api/orders/$orderId"
if ($finalOrder -and $finalOrder.success) {
    Write-Host "Order status: $($finalOrder.order.status)" -ForegroundColor Cyan
    if ($finalOrder.order.status -eq "Cancelled") {
        Write-Host "✓ Order successfully cancelled!" -ForegroundColor Green
    }
}
Write-Host ""

Write-Host "=== All Tests Completed ===" -ForegroundColor Cyan

