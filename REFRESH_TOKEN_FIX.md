# Исправление проблемы Refresh Token

## Обнаруженные проблемы

### 1. Отсутствие PropertyNameCaseInsensitive в Gateway
**Проблема**: Frontend отправляет JSON с camelCase (`refreshToken`), а backend ожидает PascalCase (`RefreshToken`).
**Решение**: Добавлен `PropertyNameCaseInsensitive = true` в конфигурацию контроллеров gateway.

### 2. Слишком строгий ClockSkew
**Проблема**: `ClockSkew = TimeSpan.Zero` в валидации refresh token может отклонять валидные токены из-за небольших расхождений времени между серверами.
**Решение**: Изменен на `ClockSkew = TimeSpan.FromMinutes(5)`.

### 3. Недостаточное логирование
**Проблема**: Сложно диагностировать проблемы с refresh token.
**Решение**: Добавлено подробное логирование в `AuthController.Refresh()` и `JwtTokenService.ValidateRefreshToken()`.

## Внесенные изменения

### Program.cs (Gateway)
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
```

### JwtTokenService.cs
1. Добавлен `ILogger<JwtTokenService>` в конструктор
2. Изменен `ClockSkew` с `TimeSpan.Zero` на `TimeSpan.FromMinutes(5)`
3. Добавлено логирование всех этапов валидации токена
4. Добавлена детальная обработка исключений (особенно `SecurityTokenExpiredException`)

### AuthController.cs
1. Добавлено логирование начала обработки refresh запроса
2. Добавлено логирование успешного обновления токена
3. Улучшена обработка ошибок

## Созданные тесты

### JwtTokenServiceTests.cs
- 11 юнит-тестов для проверки генерации и валидации токенов
- Тесты на граничные случаи (null, пустые токены, неверные токены)
- Тесты на разные issuer/audience

### AuthControllerTests.cs
- 8 юнит-тестов для проверки всех endpoint'ов контроллера
- Специальные тесты для endpoint `/api/auth/refresh`:
  - С валидным refresh token
  - С невалидным refresh token
  - С пустым refresh token
  - Когда пользователь не найден
  - Когда отсутствует claim с userId

### RefreshTokenIntegrationTests.cs
- Тесты сериализации/десериализации JSON
- Проверка совместимости с camelCase и PascalCase

## Как протестировать

### 1. Запуск юнит-тестов
```powershell
cd services\gateway\FeaneGateway.Tests
dotnet test --verbosity normal
```

### 2. Тестирование через PowerShell скрипт
```powershell
.\test-refresh-token.ps1
```

Этот скрипт:
1. Выполняет login
2. Получает access и refresh токены
3. Вызывает `/api/auth/refresh` с refresh токеном
4. Получает новые токены
5. Проверяет profile endpoint с новым токеном

### 3. Тестирование вручную через HTTP

#### Шаг 1: Login
```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
    "credential": "admin",
    "password": "Admin123!"
}
```

#### Шаг 2: Refresh Token
```http
POST http://localhost:5000/api/auth/refresh
Content-Type: application/json

{
    "refreshToken": "<REFRESH_TOKEN_FROM_STEP_1>"
}
```

## Технические детали

### JWT Claims в Refresh Token
- `nameid` или `sub`: ID пользователя
- `name`: Имя пользователя
- `token_type`: "refresh" (для различия с access token)

### Время жизни токенов
- **Access Token**: 60 минут (настраивается в `JwtSettings:AccessTokenExpirationMinutes`)
- **Refresh Token**: 7 дней (настраивается в `JwtSettings:RefreshTokenExpirationDays`)

### Формат запроса от Frontend
JavaScript (camelCase):
```javascript
{
    refreshToken: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

Теперь backend принимает и обрабатывает этот формат корректно благодаря `PropertyNameCaseInsensitive = true`.

## Проверка логов

При отладке проблем с refresh token, проверяйте следующие логи:

```
[Information] Refresh token request received
[Debug] Attempting to validate refresh token
[Debug] Validating refresh token...
[Warning] Token is not a refresh token. Token type: {TokenType}
[Warning] Refresh token has expired
[Information] Refresh token validated successfully
[Information] Token refreshed successfully for user {UserId}
[Error] Error validating refresh token
```

## Дальнейшие улучшения

1. **Rotation Strategy**: Реализовать ротацию refresh токенов (каждый refresh token используется только один раз)
2. **Token Revocation**: Хранить refresh токены в БД для возможности их отзыва
3. **Rate Limiting**: Ограничить количество refresh запросов от одного пользователя
4. **IP Binding**: Привязать refresh токен к IP адресу для дополнительной безопасности

## Заключение

Проблема с refresh token была вызвана несовместимостью форматов JSON между frontend (camelCase) и backend (PascalCase). Добавление `PropertyNameCaseInsensitive = true` решило основную проблему. Дополнительно улучшено логирование и увеличен ClockSkew для большей надежности.

