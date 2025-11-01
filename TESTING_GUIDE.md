**Что тестируем:**
- HTTP endpoints
- Валидацию входных данных
- HTTP статус коды
- Авторизацию и аутентификацию
- Обработку ошибок

**Пример:**
```csharp
public class OrdersControllerTests
{
    [Fact]
    public async Task CreateOrder_WithValidData_CreatesOrder()
    [Fact]
    public async Task UpdateOrderStatus_WithValidData_UpdatesStatus()
    [Fact]
    public async Task DeleteOrder_WithValidId_DeletesOrder()
}
```

### 2. Service Tests (Тесты сервисов)

**Что тестируем:**
- Бизнес-логику
- Валидацию правил
- Обработку исключений
- Взаимодействие с другими сервисами

### 3. Repository Tests (Тесты репозиториев)

**Что тестируем:**
- CRUD операции
- Запросы к БД
- Транзакции
- Фильтрацию и сортировку

### 4. Integration Tests (Интеграционные тесты)

**Что тестируем:**
- Взаимодействие компонентов
- End-to-end сценарии
- API Gateway интеграцию

---

## 📊 Покрытие кода

### Генерация отчета о покрытии

```bash
# Запустить тесты с покрытием
dotnet test --collect:"XPlat Code Coverage"

# Установить ReportGenerator (один раз)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Сгенерировать HTML отчет
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

### Просмотр отчета

```bash
# Открыть отчет в браузере
start coveragereport/index.html
```

---

## 🔧 Настройка InMemory Database

Каждый тест использует изолированную InMemory базу данных:

```csharp
public UsersControllerTests()
{
    var options = new DbContextOptionsBuilder<UserDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _context = new UserDbContext(options);
    _controller = new UsersController(_context);
}
```

**Преимущества:**
- ✓ Быстрая работа (в памяти)
- ✓ Изоляция тестов (уникальная БД для каждого теста)
- ✓ Нет необходимости в реальной БД
- ✓ Детерминированность результатов

---

## 🎨 FluentAssertions

### Примеры использования

```csharp
// Базовые проверки
result.Should().NotBeNull();
result.Should().BeOfType<User>();

// Коллекции
result.Should().HaveCount(5);
result.Should().Contain(u => u.Username == "admin");
result.Should().OnlyContain(u => u.IsActive);

// Числа
order.TotalAmount.Should().Be(150.00m);
order.TotalAmount.Should().BeGreaterThan(0);

// Строки
user.Email.Should().Contain("@");
user.Email.Should().EndWith(".com");

// Даты
order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
```

---

## 🔄 CI/CD Integration

### GitHub Actions

Workflow автоматически запускается при:
- Push в main/develop
- Pull Request в main/develop

```yaml
# .github/workflows/tests.yml
name: .NET Microservices Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
      - run: dotnet test
```

---

## 📝 Best Practices

### 1. Naming Convention

```csharp
// Формат: MethodName_Scenario_ExpectedResult
GetUsers_ReturnsAllUsers
GetOrder_WithInvalidId_ReturnsNotFound
CreateOrder_WithValidData_CreatesOrder
```

### 2. Arrange-Act-Assert (AAA) Pattern

```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange - Подготовка данных и зависимостей
    var data = CreateTestData();
    
    // Act - Выполнение тестируемого метода
    var result = await _controller.MethodUnderTest(data);
    
    // Assert - Проверка результата
    result.Should().NotBeNull();
}
```

### 3. Test Isolation

- ✓ Каждый тест должен быть независимым
- ✓ Используйте уникальные идентификаторы
- ✓ Очищайте ресурсы после теста (Dispose)
- ✓ Не полагайтесь на порядок выполнения тестов

### 4. Mock Objects

```csharp
// Пример использования Moq
var mockRepository = new Mock<IUserRepository>();
mockRepository
    .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
    .ReturnsAsync(new User { Username = "test" });
```

### 5. Test Data Builders

```csharp
public class UserBuilder
{
    private string _username = "testuser";
    private string _email = "test@test.com";
    
    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }
    
    public User Build()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Username = _username,
            Email = _email
        };
    }
}

// Использование
var user = new UserBuilder()
    .WithUsername("admin")
    .Build();
```

---

## 📈 Метрики

### Статистика тестов по сервисам

| Сервис | Тестов | Статус |
|--------|--------|--------|
| User Service | 6+ | ✓ |
| Product Service | 5+ | ✓ |
| Book Service | 4+ | ✓ |
| Reservation Service | 5+ | ✓ |
| Cart Service | 6+ | ✓ |
| Order Service | 10+ | ✓ |
| Analytics Service | 8+ | ✓ |
| **TOTAL** | **44+** | ✓ |

### Целевое покрытие кода

- **Минимум:** 70%
- **Цель:** 80%+
- **Controllers:** 90%+
- **Business Logic:** 85%+
- **Repositories:** 75%+

---

## 🐛 Troubleshooting

### Проблема: Тесты падают из-за конфликта портов

```bash
# Убедитесь что Docker контейнеры остановлены
docker-compose down

# Или используйте другие порты для тестирования
```

### Проблема: InMemory Database не очищается

```csharp
// Используйте уникальное имя для каждого теста
.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())

// Не забывайте Dispose
public void Dispose()
{
    _context?.Dispose();
}
```

### Проблема: Тесты проходят локально, но падают в CI

```bash
# Проверьте версию .NET SDK
dotnet --version

# Убедитесь что зависимости одинаковые
dotnet restore

# Проверьте переменные окружения
```

---

## 📚 Дополнительные ресурсы

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [EF Core InMemory Database](https://docs.microsoft.com/en-us/ef/core/testing/)

---

## 🎯 Следующие шаги

1. ✅ Создать тесты для всех контроллеров
2. ✅ Настроить CI/CD pipeline
3. ⬜ Добавить Integration tests
4. ⬜ Настроить автоматическую генерацию отчетов покрытия
5. ⬜ Добавить Performance tests
6. ⬜ Добавить E2E tests с реальными HTTP запросами

---

**Автор:** Feane Restaurant Development Team  
**Последнее обновление:** 2 ноября 2025  
**Версия:** 1.0.0
# 🧪 Unit Testing Guide

## Обзор

Все микросервисы проекта Feane Restaurant имеют полный набор Unit тестов, написанных с использованием:

- **xUnit** - популярный фреймворк для тестирования .NET
- **Moq** - библиотека для создания mock-объектов
- **FluentAssertions** - для читаемых утверждений
- **InMemory Database** - для тестирования с Entity Framework Core

---

## 📂 Структура тестовых проектов

```
FeaneShop/
├── services/
│   ├── user-service/
│   │   ├── src/                         # Основной проект
│   │   └── UserService.Tests/           # Тесты
│   │       └── Controllers/
│   │           └── UsersControllerTests.cs
│   │
│   ├── product-service/
│   │   └── ProductService.Tests/
│   │
│   ├── book-service/
│   │   └── BookService.Tests/
│   │
│   ├── reservation-service/
│   │   └── ReservationService.Tests/
│   │
│   ├── cart-service/
│   │   └── CartService.Tests/
│   │
│   ├── OrderService/
│   │   └── ../OrderService.Tests/
│   │       └── Controllers/
│   │           └── OrdersControllerTests.cs
│   │
│   └── AnalyticsService/
│       └── ../AnalyticsService.Tests/
│           └── Controllers/
│               └── AnalyticsControllerTests.cs
│
├── run-all-tests.ps1                    # Скрипт для запуска всех тестов
└── .github/workflows/tests.yml          # CI/CD конфигурация
```

---

## 🚀 Быстрый старт

### Запуск всех тестов

```powershell
# Из корня проекта
.\run-all-tests.ps1
```

### Запуск тестов конкретного сервиса

```bash
# User Service
cd services/user-service/UserService.Tests
dotnet test

# Order Service
cd services/OrderService.Tests
dotnet test

# Analytics Service
cd services/AnalyticsService.Tests
dotnet test
```

### Запуск с подробным выводом

```bash
dotnet test --verbosity detailed
```

### Запуск конкретного теста

```bash
dotnet test --filter "FullyQualifiedName~GetUsers_ReturnsAllUsers"
```

### Запуск с покрытием кода

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 📝 Примеры тестов

### 1. Простой тест контроллера (User Service)

```csharp
[Fact]
public async Task GetUsers_ReturnsAllUsers()
{
    // Arrange - Подготовка данных
    var users = new List<User>
    {
        new User { Id = Guid.NewGuid(), Username = "user1" },
        new User { Id = Guid.NewGuid(), Username = "user2" }
    };
    _context.Users.AddRange(users);
    await _context.SaveChangesAsync();

    // Act - Выполнение действия
    var result = await _controller.GetUsers();

    // Assert - Проверка результата
    result.Should().NotBeNull();
    result.Should().HaveCount(2);
}
```

### 2. Тест с проверкой NotFound (Order Service)

```csharp
[Fact]
public async Task GetOrder_WithInvalidId_ReturnsNotFound()
{
    // Arrange
    var invalidId = Guid.NewGuid();

    // Act
    var result = await _controller.GetOrder(invalidId);

    // Assert
    result.Value.Should().BeNull();
}
```

### 3. Параметризованный тест (Analytics Service)

```csharp
[Theory]
[InlineData(1)]
[InlineData(7)]
[InlineData(30)]
public async Task GetDashboard_WithDifferentDateRanges_ReturnsCorrectData(int days)
{
    // Arrange
    var events = CreateTestEvents(days);
    _context.AnalyticsEvents.AddRange(events);
    await _context.SaveChangesAsync();

    // Act
    var result = await _controller.GetDashboard(
        DateTime.UtcNow.AddDays(-days),
        DateTime.UtcNow);

    // Assert
    result.TotalOrders.Should().Be(days);
}
```

### 4. Тест с исключением

```csharp
[Fact]
public async Task CreateOrder_WithNegativeAmount_ThrowsException()
{
    // Arrange
    var newOrder = new Order { TotalAmount = -100.00m };

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(async () =>
    {
        await _controller.CreateOrder(newOrder);
    });
}
```

---

## 🎯 Типы тестов

### 1. Controller Tests (Тесты контроллеров)


