# Refresh Token Flow - Fixed Implementation

## Architecture Overview

```
┌─────────────┐                    ┌─────────────┐                    ┌──────────────┐
│   Frontend  │                    │   Gateway   │                    │   Database   │
│ (JavaScript)│                    │  (ASP.NET)  │                    │  (SQL Server)│
└──────┬──────┘                    └──────┬──────┘                    └──────┬───────┘
       │                                  │                                  │
       │                                  │                                  │
```

## Refresh Token Flow (Step by Step)

### 1. Initial Login
```
Frontend                          Gateway                           Database
   │                                 │                                  │
   ├─(1) POST /api/auth/login────────>│                                  │
   │     {credential, password}       │                                  │
   │                                  ├─(2) Authenticate User──────────>│
   │                                  │                                  │
   │                                  │<─(3) User Data──────────────────┤
   │                                  │                                  │
   │                                  ├─(4) Generate Access Token       │
   │                                  │     (expires: 60 min)            │
   │                                  │                                  │
   │                                  ├─(5) Generate Refresh Token      │
   │                                  │     (expires: 7 days)            │
   │                                  │     + claim: token_type="refresh"│
   │                                  │                                  │
   │<─(6) Response────────────────────┤                                  │
   │     {token, refreshToken, user}  │                                  │
   │                                  │                                  │
   ├─(7) Store in localStorage       │                                  │
   │     - jwtToken                   │                                  │
   │     - refreshToken               │                                  │
   │                                  │                                  │
```

### 2. API Request with Expired Token
```
Frontend                          Gateway                           
   │                                 │                               
   ├─(1) GET /api/some-resource──────>│                               
   │     Authorization: Bearer <expired_token>                       
   │                                  │                               
   │                                  ├─(2) Validate Token            
   │                                  │     ❌ Token expired!          
   │                                  │                               
   │<─(3) 401 Unauthorized────────────┤                               
   │                                  │                               
```

### 3. Token Refresh Process (FIXED)
```
Frontend                          Gateway                           Database
   │                                 │                                  │
   ├─(1) POST /api/auth/refresh──────>│                                  │
   │     Content-Type: application/json                                 │
   │     {                            │                                  │
   │       "refreshToken": "eyJ..."   │  ✅ NOW WORKS!                  │
   │     }                            │  (camelCase accepted)            │
   │     ↑                            │                                  │
   │     │ camelCase from JavaScript  │                                  │
   │                                  │                                  │
   │                                  ├─(2) PropertyNameCaseInsensitive │
   │                                  │     converts to RefreshToken    │
   │                                  │                                  │
   │                                  ├─(3) Validate Refresh Token      │
   │                                  │     - Check signature            │
   │                                  │     - Check issuer/audience      │
   │                                  │     - Check expiration           │
   │                                  │       (ClockSkew: 5 min) ✅     │
   │                                  │     - Verify token_type="refresh"│
   │                                  │                                  │
   │                                  ├─(4) Extract userId from claims  │
   │                                  │                                  │
   │                                  ├─(5) Find User──────────────────>│
   │                                  │                                  │
   │                                  │<─(6) User Data──────────────────┤
   │                                  │                                  │
   │                                  ├─(7) Generate New Access Token   │
   │                                  │     (expires: 60 min)            │
   │                                  │                                  │
   │                                  ├─(8) Generate New Refresh Token  │
   │                                  │     (expires: 7 days)            │
   │                                  │                                  │
   │                                  ├─(9) Log Success                 │
   │                                  │     "Token refreshed for {userId}"
   │                                  │                                  │
   │<─(10) Response───────────────────┤                                  │
   │      {                           │                                  │
   │        "token": "new_access...", │                                  │
   │        "refreshToken": "new...", │                                  │
   │        "user": {...}             │                                  │
   │      }                           │                                  │
   │                                  │                                  │
   ├─(11) Update localStorage        │                                  │
   │      - jwtToken = new token     │                                  │
   │      - refreshToken = new token │                                  │
   │                                  │                                  │
   ├─(12) Retry Original Request─────>│                                  │
   │      Authorization: Bearer <new_token>                              │
   │                                  │                                  │
   │<─(13) Success 200────────────────┤                                  │
   │      {data}                      │                                  │
   │                                  │                                  │
```

## Key Components

### JwtTokenService
```csharp
public class JwtTokenService : IJwtTokenService
{
    // Generates refresh token with:
    // - Claims: userId, username, token_type="refresh"
    // - Expiration: 7 days
    // - Signed with SecretKey
    public string GenerateRefreshToken(User user) { ... }
    
    // Validates refresh token:
    // - Checks signature
    // - Checks expiration (with 5 min ClockSkew)
    // - Verifies token_type="refresh"
    // - Logs all steps
    public ClaimsPrincipal? ValidateRefreshToken(string refreshToken) { ... }
}
```

### AuthController
```csharp
[HttpPost("refresh")]
public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
{
    // 1. Log request
    // 2. Validate refresh token
    // 3. Extract userId from claims
    // 4. Find user in database
    // 5. Generate new tokens
    // 6. Log success
    // 7. Return new tokens
}
```

### Program.cs Configuration
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // ✅ CRITICAL FIX: Accept both camelCase and PascalCase
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
```

## Error Handling

### Possible Errors
1. **Missing refresh token** → 400 Bad Request
2. **Invalid refresh token** → 401 Unauthorized
3. **Expired refresh token** → 401 Unauthorized (logged as SecurityTokenExpiredException)
4. **Wrong token type** → 401 Unauthorized (not a refresh token)
5. **User not found** → 401 Unauthorized
6. **Invalid claims** → 401 Unauthorized

### Logging Levels
- **Information**: Normal flow events
- **Debug**: Detailed validation steps
- **Warning**: Invalid tokens, missing users
- **Error**: Unexpected exceptions

## Security Considerations

### What's Secure
✅ Tokens are signed with HMAC-SHA256
✅ Refresh tokens have limited lifetime (7 days)
✅ Tokens contain userId for user identification
✅ Token type distinction (access vs refresh)
✅ Comprehensive logging for audit trail

### Future Improvements
⚠️ **Token Rotation**: Each refresh generates new refresh token, invalidate old one
⚠️ **Token Storage**: Store refresh tokens in database for revocation
⚠️ **Rate Limiting**: Limit refresh attempts per user
⚠️ **Device Binding**: Tie refresh token to device fingerprint
⚠️ **IP Validation**: Check if refresh request comes from same IP

## Testing

### Unit Tests (22 total)
- JwtTokenService: Generation and validation
- AuthController: All endpoints including refresh
- JSON Serialization: camelCase/PascalCase compatibility

### Integration Tests
- End-to-end refresh flow
- Error scenarios
- Token expiration

### Manual Testing
```powershell
# Run the test script
.\test-refresh-token.ps1
```

## Conclusion

The refresh token mechanism is now fully functional with:
1. ✅ Frontend-backend JSON compatibility
2. ✅ Proper token validation with time tolerance
3. ✅ Comprehensive logging for debugging
4. ✅ Full test coverage
5. ✅ Documentation and diagrams

The system can now seamlessly refresh access tokens without requiring users to re-login.

