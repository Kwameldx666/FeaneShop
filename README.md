# 🍽️ Feane Restaurant - Full Stack E-Commerce System

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)
![Microservices](https://img.shields.io/badge/Architecture-Microservices-green?style=for-the-badge)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=for-the-badge&logo=microsoft-sql-server)

[![CI](https://github.com/Kwameldx666/FeaneShop/actions/workflows/ci.yml/badge.svg)](https://github.com/Kwameldx666/FeaneShop/actions/workflows/ci.yml)
[![CD](https://github.com/Kwameldx666/FeaneShop/actions/workflows/cd.yml/badge.svg)](https://github.com/Kwameldx666/FeaneShop/actions/workflows/cd.yml)
[![Tests](https://github.com/Kwameldx666/FeaneShop/actions/workflows/tests.yml/badge.svg)](https://github.com/Kwameldx666/FeaneShop/actions/workflows/tests.yml)
[![Nightly](https://github.com/Kwameldx666/FeaneShop/actions/workflows/nightly.yml/badge.svg)](https://github.com/Kwameldx666/FeaneShop/actions/workflows/nightly.yml)

**Полнофункциональная система управления рестораном с микросервисной архитектурой**

[🚀 Быстрый старт](#-быстрый-старт) • [🏗️ Архитектура](#️-архитектура) • [📦 Микросервисы](#-микросервисы) • [🔐 Безопасность](#-безопасность) • [💳 Оплата](#-оплата) • [📊 Аналитика](#-аналитика)

</div>

---

## 📋 Описание

**Feane Restaurant** - это современная система управления рестораном, построенная на микросервисной архитектуре. Система включ��ет:

- 🛒 **E-Commerce** - каталог товаров, корзина, оформление заказов
- 📅 **Резервации** - бронирование столиков с управлением
- 👥 **Управление пользователями** - регистрация, авторизация, профили
- 💳 **Платежная система** - имитация оплаты с подтверждением
- 📊 **Аналитика** - dashboard с метриками и отчетами
- 🔐 **JWT Authentication** - с Access и Refresh токенами
- 🌐 **API Gateway** - централизованная точка входа (Ocelot)

---

## 🚀 Быстрый старт

### Требования

- .NET 9.0 SDK
- Docker Desktop
- SQL Server 2022 (или через Docker)
- Visual Studio 2022 / JetBrains Rider / VS Code

### Запуск системы

```bash
# 1. Клонировать репозиторий
git clone <repository-url>
cd FeaneShop

# 2. Запустить все сервисы через Docker Compose
docker-compose up -d

# 3. Дождаться инициализации (1-2 минуты)
# Проверить статус:
docker ps

# 4. Открыть приложение
# Frontend: http://localhost:5003
# Gateway: http://localhost:5000
```

### Первый запуск

1. **Создание администратора** (через Swagger или напрямую в БД)
2. **Вход в систему**: http://localhost:5003/account/authentication
3. **Просмотр меню**: http://localhost:5003/home/menu
4. **Dashboard аналитики**: http://localhost:5003/analytics

---

## 🏗️ Архитектура

### Микросервисная архитектура

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │
       ↓
┌──────────────┐
│   Frontend   │ :5003 (Static HTML/CSS/JS)
└──────┬───────┘
       │
       ↓
┌──────────────┐
│   Gateway    │ :5000 (Ocelot API Gateway)
│  JWT Auth    │
└──────┬───────┘
       │
       ├─→ User Service      :5050
       ├─→ Product Service   :5060
       ├─→ Book Service      :5040
       ├─→ Reservation Svc   :5030
       ├─→ Cart Service      :5020
       ├─→ Order Service     :5070
       └─→ Analytics Service :5080
            │
            ↓
       ┌─────────────┐
       │ SQL Server  │ :1433
       │ 8 Databases │
       └─────────────┘
```

### Clean Architecture (каждый сервис)

```
Controllers
    ↓
Application (DTOs, Commands, Queries)
    ↓
Domain (Entities, Value Objects)
    ↓
Infrastructure (Persistence, Repositories)
    ↓
Database
```

---

## 📦 Микросервисы

### 1. 👤 User Service (Port 5050)
**База:** `Feane.UserServiceDb`

**Функционал:**
- CRUD операции пользователей
- Управление ролями (Admin, Moderator, User)
- Профили пользователей
- Аутентификация и авторизация

**Endpoints:**
- `GET /api/users` - список пользователей
- `GET /api/users/{id}` - получить пользователя
- `POST /api/users` - создать пользователя
- `PUT /api/users/{id}` - обновить пользователя
- `DELETE /api/users/{id}` - удалить пользователя

---

### 2. 🍕 Product Service (Port 5060)
**База:** `Feane.ProductServiceDb`

**Функционал:**
- Каталог товаров (блюд)
- Категории товаров
- Управление ценами
- Изображения товаров

**Endpoints:**
- `GET /api/products` - список товаров
- `GET /api/products/{id}` - детали товара
- `POST /api/products` - создать товар
- `PUT /api/products/{id}` - обновить товар
- `DELETE /api/products/{id}` - удалить товар
- `GET /api/products/category/{category}` - товары по категории

---

### 3. 📅 Book Service (Port 5040)
**База:** `Feane.BookServiceDb`

**Функционал:**
- Бронирование столиков
- Управление доступностью столов
- Отмена бронирований

**Endpoints:**
- `GET /api/books` - список бронирований
- `GET /api/books/{id}` - детали бронирования
- `POST /api/books` - создать бронирование
- `PUT /api/books/{id}/cancel` - отменить бронирование

---

### 4. 🎫 Reservation Service (Port 5030)
**База:** `Feane.ReservationServiceDb`

**Функционал:**
- Управление резервациями
- История резерваций
- Статусы резерваций

**Endpoints:**
- `GET /api/reservations` - список резерваций
- `GET /api/reservations/{id}` - детали резервации
- `GET /api/reservations/user/{userId}` - резервации пользователя

---

### 5. 🛒 Cart Service (Port 5020)
**База:** `Feane.CartServiceDb`

**Функционал:**
- Управление корзиной покуп��к
- Добавление/уда��ение товаров
- Изменение количества
- Очистка корзины

**Endpoints:**
- `GET /api/cart` - получить корзину
- `POST /api/cart/items` - добавить товар
- `PUT /api/cart/items/{itemId}` - обновить количество
- `DELETE /api/cart/items/{itemId}` - удалить товар
- `DELETE /api/cart` - очисти��ь корзину

---

### 6. 📦 Order Service (Port 5070)
**База:** `Feane.OrderServiceDb`

**Функционал:**
- Создание заказов
- Управление статусами заказов
- История заказов
- Отмена заказов

**Статусы:**
- Pending - ожидает обработки
- Processing - в обработке
- Completed - завершен
- Cancelled - отменен

**Endpoints:**
- `GET /api/orders` - список заказов
- `GET /api/orders/{id}` - детали заказа
- `POST /api/orders` - создать заказ
- `PUT /api/orders/{id}/status` - обновить статус
- `DELETE /api/orders/{id}` - отменить заказ
- `GET /api/orders/user/{userId}` - заказы пользователя

---

### 7. 📊 Analytics Service (Port 5080)
**База:** `Feane.AnalyticsServiceDb`

**Функционал:**
- Dashboard с KPI метриками
- Статистика заказов
- Анализ выручки
- Топ товары
- Фильтры по датам

**KPI Метрики:**
- Total Revenue - общая выручка
- Total Orders - количество заказов
- Completed Orders - завершенные заказы
- Cancelled Orders - отмененные заказы
- Average Order Value - средний чек
- Top Products - топ товары

**Endpoints:**
- `GET /api/analytics/dashboard` - главный dashboard
- `GET /api/analytics/revenue` - отчет о выручке
- `GET /api/analytics/products` - статистика товаров
- `POST /api/analytics/events` - запись событий

**Seed Data:**
- Автоматически генерируется 30 дней статистики
- Реалистичные данные для тестирования

---

### 8. 🌐 Gateway (Port 5000)
**База:** `Feane.AuthServiceDb`

**Функционал:**
- API Gateway (Ocelot)
- JWT Authentication
- Маршрутизация запросов
- Централизованная авторизация

**Маршруты:**
```json
/api/users      → User Service
/api/products   → Product Service
/api/books      → Book Service
/api/reservations → Reservation Service
/api/cart       → Cart Service
/api/orders     → Order Service
/api/analytics  → Analytics Service
/api/auth/*     → Gateway (Auth endpoints)
```

---

### 9. 🌐 Frontend (Port 5003)
**Технологии:** HTML5, CSS3, JavaScript, jQuery

**Страницы:**
- `/` - главная страница
- `/home/menu` - меню товаров
- `/cart/cart` - корзина
- `/orders/checkout` - оформление заказа
- `/orders/payment` - страница оплаты
- `/orders` - список заказов
- `/orders/{id}` - детали заказа
- `/reservation/book` - бронирование столиков
- `/reservation/history` - история бронирований
- `/analytics` - dashboard аналитики
- `/account/authentication` - вход/регистрация
- `/account/profile` - профиль польз��вателя

**Компоненты:**
- Navbar (на всех страницах)
- Footer
- Auth Guard (защита маршрутов)
- Gateway Client (API интеграция)

---

## 🔐 Безопасность

### JWT Authentication

**Access Token:**
- Время жизни: 60 минут
- Claims: userId, username, email, role
- Используется для авторизации запросов

**Refresh Token:**
- Время жизни: 7 дней
- Хранится в localStorage
- Используется для обновления Access Token

### Автоматическое обновление токена

```javascript
// GatewayClient автоматически обновляет токен
if (accessTokenExpired && refreshTokenValid) {
    // Показать диалог продления
    showTokenRenewalDialog();
    // Обновить токен
    await refreshAccessToken();
}
```

### Защищенные эндпоинты

Все эндпоинты микросервисов требуют JWT токен (кроме авторизации):

```
Authorization: Bearer <access_token>
```

### Роли

- **Admin** - полный доступ ко всем функциям
- **Moderator** - управление контентом
- **User** - стандартные операции

---

## 💳 Оплата

### Процесс оплаты

```
Cart → Checkout → Payment Page → Order Created → Order Details
```

### Payment Flow

1. **Checkout** (`/orders/checkout`)
   - Просмотр корзины
   - Подтверждение заказа
   - Переход на оплату

2. **Payment** (`/orders/payment`)
   - Форма оплаты картой (имитация)
   - Валидация полей
   - Loading overlay

3. **Order Created**
   - Создание заказа через API
   - Автоматическая очистка к��рзины
   - Редирект на детали заказа

### Форма оплаты

**Поля:**
- Card Number (16 цифр)
- Cardholder Name
- Expiry Date (MM/YY)
- CVV (3 цифры)

**Валидация:**
- Проверка формата карты
- Проверка срока действия
- Проверка CVV

**После оплаты:**
- Заказ создается со статусом "Pending"
- Корзина очищается автоматически
- Пользователь переходит на страницу заказа

---

## 📊 Аналитика

### Dashboard

**URL:** `http://localhost:5003/analytics`

**KPI Метрики (Real-time):**
```
┌───────────────────��─┬──────────────────┐
│ Total Revenue       │ 95,450.00 LEI    │
│ Total Orders        │ 892              │
│ Completed Orders    │ 758              │
│ Cancelled Orders    │ 134              │
│ Average Order Value │ 107.05 LEI       │
│ Top Product         │ Pizza Margherita │
└─────────────���───────┴──────────────────┘
```

### Revenue Chart

Показывает последние 7 дней:
```
Date        Orders    Revenue
29 Oct      38        4,180.00 LEI
30 Oct      42        4,620.00 LEI
31 Oct      35        3,850.00 LEI
...
```

### Top Products

Топ-5 товаров по выручке:
```
Rank  Product Name         Units Sold  Revenue
#1    Pizza Margherita     245         3,675.00 LEI
#2    Burger Deluxe        198         2,970.00 LEI
#3    Caesar Salad         167         1,670.00 LEI
...
```

### Date Filters

- Start Date / End Date
- Apply Filters - обновить данные
- Reset - вернуть к последним 30 дням

### Seed Data

При первом запуске Analytics Service автоматически генерирует:
- ✅ 30 дней статистики заказов
- ✅ 10 товаров с продажами
- ✅ 100 событий аналитики
- ✅ Реалистичные данные

---

## 💱 Валюта

### LEI (Молдавский/Румынский лей)

Во всём проекте используется валюта **LEI**.

**Форматирование:**
```javascript
// Примеры отображения
12.50 LEI
1,250.00 LEI
95,450.00 LEI
```

**Где используется:**
- Меню товаров
- Корзина
- Checkout
- Страница оплаты
- История заказов
- Dashboard аналитики
- Резервации (бюджет)

**Локализация:**
- Региональная настройка: `ro-RO` (Румыния/Молдова)
- Код валюты ISO: MDL (Молдова) / RON (Румыния)

---

## 🗄️ Базы данных

### SQL Server 2022 (Docker)

**Всего БД:** 8

1. `Feane.AuthServiceDb` - Gateway/Auth
2. `Feane.UserServiceDb` - пользователи
3. `Feane.ProductServiceDb` - товары
4. `Feane.BookServiceDb` - бронирования
5. `Feane.ReservationServiceDb` - резервации
6. `Feane.CartServiceDb` - корзины
7. `Feane.OrderServiceDb` - заказы
8. `Feane.AnalyticsServiceDb` - аналитика

### Миграции

Каждый сервис автоматически применяет миграции при запуске:

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DbContext>();
    db.Database.Migrate();
}
```

### Connection String (Docker)

```
Server=sqlserver;Database=Feane.<ServiceName>Db;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True
```

---

## 🐳 Docker

### docker-compose.yml

**Сервисы (10 контейнеров):**

```yaml
services:
  sqlserver:           # SQL Server 2022
  user-service:        # :5050
  product-service:     # :5060
  book-service:        # :5040
  reservation-service: # :5030
  cart-service:        # :5020
  order-service:       # :5070
  analytics-service:   # :5080
  feane-gateway:       # :5000
  feane-frontend:      # :5003
```

### Команды Docker

```bash
# Запустить все сервисы
docker-compose up -d

# Остановить все сервисы
docker-compose down

# Пересобрать сервис
docker-compose build <service-name>

# Пересобрать и перезапустить
docker-compose up -d --build <service-name>

# Логи сервиса
docker logs <container-name>

# Логи с отслеживанием
docker logs -f <container-name>

# Статус контейнеров
docker ps

# Остановить и удалить всё (включая volumes)
docker-compose down -v
```

### Health Checks

SQL Server имеет health check:
```yaml
healthcheck:
  test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $$MSSQL_SA_PASSWORD -Q 'SELECT 1' || exit 1"]
  interval: 10s
  timeout: 3s
  retries: 5
  start_period: 30s
```

---

## 📁 Структура проекта

```
FeaneShop/
├── services/
│   ├── user-service/
│   │   └── src/
│   │       ├── Controllers/
│   │       ├── Domain/
│   │       ├── Application/
│   │       └── Infrastructure/
│   ├── product-service/
│   ├── book-service/
│   ├── reservation-service/
│   ├── cart-service/
│   ├── OrderService/
│   ├── AnalyticsService/
│   ├── gateway/
│   │   ├── ocelot.json
│   │   ├── Controllers/
│   │   │   └── AuthController.cs
│   │   └── Middleware/
│   └── frontend/
│       └── wwwroot/
│           ├── Pages/
│           │   ├── Home/
│           │   ├── Cart/
│           │   ├── Orders/
│           │   ├── Reservation/
│           │   ├── Analytics/
│           │   └── Account/
│           ├── js/
│           │   ├── gateway-client.js
│           │   ├── auth-guard.js
│           │   ├── cart.js
│           │   └── ...
│           ├── css/
│           └── partials/
│               ├── navbar.html
│               └── footer.html
├── src/ (MVC monolith - не используется в микросервисах)
│   ├── FeaneMVC/
│   ├── FeaneMVC.Application/
│   ├── FeaneMVC.Domain/
│   └── FeaneMVC.Infrastructure/
├── docker-compose.yml
└── README.md
```

---

## ⚙️ Технологии

### Backend

| Технология | Версия | Описание |
|------------|--------|----------|
| .NET | 9.0 | Framework |
| ASP.NET Core | 9.0 | Web API |
| Entity Framework Core | 9.0 | ORM |
| SQL Server | 2022 | Database |
| Ocelot | 23.4.0 | API Gateway |
| JWT Bearer | 9.0 | Authentication |
| Swagger/OpenAPI | 7.0 | API Documentation |

### Frontend

| Технология | Описание |
|------------|----------|
| HTML5 | Разметка |
| CSS3 | Стили (Custom + Bootstrap) |
| JavaScript (ES6+) | Логика |
| jQuery | 3.4.1 |
| Gateway Client | Custom library для API |

### DevOps

| Технология | Описание |
|------------|----------|
| Docker | Контейнеризация |
| Docker Compose | Оркестрация |
| Multi-stage Builds | Оптимизация образов |

---

## 🌐 URLs

### Frontend

```
http://localhost:5003                      # Главная
http://localhost:5003/home/menu            # Меню
http://localhost:5003/cart/cart            # Корзина
http://localhost:5003/orders               # Заказы
http://localhost:5003/orders/checkout      # Оформление
http://localhost:5003/orders/payment       # Оплата
http://localhost:5003/reservation/book     # Бронирование
http://localhost:5003/analytics            # Аналитика
http://localhost:5003/account/authentication  # Вход
```

### Gateway API

```
http://localhost:5000/api/products         # Товары
http://localhost:5000/api/cart             # Корзина
http://localhost:5000/api/orders           # Заказы
http://localhost:5000/api/analytics        # Аналитика
http://localhost:5000/api/auth/login       # Вход
http://localhost:5000/api/auth/refresh     # Обновление токена
```

### Direct Service Access (для тестиро��ания)

```
http://localhost:5050/swagger              # User Service
http://localhost:5060/swagger              # Product Service
http://localhost:5040/swagger              # Book Service
http://localhost:5030/swagger              # Reservation Service
http://localhost:5020/swagger              # Cart Service
http://localhost:5070/swagger              # Order Service
http://localhost:5080/swagger              # Analytics Service
```

---

## 🧪 Тестирование

### Unit Tests (xUnit)

Каждый микросервис имеет соответствующий тестовый проект с использованием:
- **xUnit** - фреймворк для тестирования
- **Moq** - мокирование зависимостей
- **FluentAssertions** - читаемые утверждения
- **InMemory Database** - тестирование с EF Core

#### Структура тестовых проектов

```
services/
├── user-service/
│   ├── src/ (основной проект)
│   └── UserService.Tests/
│       └── Controllers/
│           └── UsersControllerTests.cs
├── product-service/
│   └── ProductService.Tests/
├── book-service/
│   └── BookService.Tests/
├── reservation-service/
│   └── ReservationService.Tests/
├── cart-service/
│   └── CartService.Tests/
├── OrderService.Tests/
└── AnalyticsService.Tests/
```

#### Запуск тестов

```bash
# Запустить все тесты
dotnet test

# Запустить тесты конкретного сервиса
cd services/user-service/UserService.Tests
dotnet test

# Запустить тесты с подробным выводом
dotnet test --verbosity detailed

# Запустить тесты с покрытием кода
dotnet test --collect:"XPlat Code Coverage"

# Запустить конкретный тест
dotnet test --filter "FullyQualifiedName~GetUsers_ReturnsAllUsers"
```

#### Пример теста

```csharp
[Fact]
public async Task GetUsers_ReturnsAllUsers()
{
    // Arrange - подготовка
    var users = new List<User>
    {
        new User { Id = Guid.NewGuid(), Username = "user1", Email = "user1@test.com" },
        new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@test.com" }
    };
    _context.Users.AddRange(users);
    await _context.SaveChangesAsync();

    // Act - действие
    var result = await _controller.GetUsers();

    // Assert - проверка
    result.Should().NotBeNull();
    result.Should().HaveCount(2);
}
```

#### Типы тестов

**1. Controller Tests**
- Тестирование API endpoints
- Валидация входных данных
- Проверка HTTP с��атус кодов
- Тестирование авторизации

**2. Service Tests**
- Бизнес-логика
- Валидация правил
- Обработка исключений

**3. Repository Tests**
- CRUD операции
- Запросы к БД
- Транзакции

**4. Integration Tests**
- Взаимодействие компонентов
- End-to-end сценарии

---

### HTTP Files

Каждый сервис имеет `.http` файл для ручного тестирования:

```
services/OrderService/OrderService.http
services/AnalyticsService/AnalyticsService.http
services/gateway/FeaneGateway.http
```

### PowerShell Scripts

```powershell
# Тест профиля
.\test-profile.ps1

# Тест резерваций
.\test-reservations.ps1

# Тест списка пользователей
.\test-users-list.ps1
```

### Swagger UI

Каждый микросервис имеет Swagger UI для интерактивного тестирования:
```
http://localhost:5050/swagger - User Service
http://localhost:5060/swagger - Product Service
http://localhost:5040/swagger - Book Service
http://localhost:5030/swagger - Reservation Service
http://localhost:5020/swagger - Cart Service
http://localhost:5070/swagger - Order Service
http://localhost:5080/swagger - Analytics Service
```

### Тестовый сценарий (E2E)

1. **Регистрация/Вход**
   ```bash
   POST http://localhost:5000/api/auth/login
   {
     "username": "admin",
     "password": "password"
   }
   ```

2. **Просмотр товаров**
   ```bash
   GET http://localhost:5000/api/products
   ```

3. **Добавление в корзину**
   ```bash
   POST http://localhost:5000/api/cart/items
   {
     "productId": "guid",
     "quantity": 2
   }
   ```

4. **Создание заказа**
   ```bash
   POST http://localhost:5000/api/orders
   {
     "items": [...],
     "totalAmount": 150.00
   }
   ```

5. **Просмотр аналитики**
   ```bash
   GET http://localhost:5000/api/analytics/dashboard
   ```

### Continuous Integration (CI)

```yaml
# .github/workflows/tests.yml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Setup .NET
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '9.0.x'
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore
      - name: Test
        run: dotnet test --no-build --verbosity normal
```

---

## 🔧 Разработка

### Требования для разработки

- Visual Studio 2022 / JetBrains Rider / VS Code
- .NET 9.0 SDK
- Docker Desktop
- SQL Server 2022 (или через Docker)
- Git

### Локальный запуск (без Docker)

1. **Запустить SQL Server**
2. **Обновить connection strings** в `appsettings.json` каждого сервиса
3. **Запустить сервисы по отдельности**:

```bash
# Terminal 1 - User Service
cd services/user-service/src
dotnet run

# Terminal 2 - Product Service
cd services/product-service/src
dotnet run

# Terminal 3 - Gateway
cd services/gateway
dotnet run

# Terminal 4 - Frontend
cd services/frontend
dotnet run

# И так далее для других сервисов...
```

### Создание миграции

```bash
cd services/<ServiceName>

# Создать миграцию
dotnet ef migrations add MigrationName

# Применить миграцию
dotnet ef database update

# Откатить миграцию
dotnet ef database update PreviousMigrationName

# Удалить последнюю миграцию
dotnet ef migrations remove
```

### Сборка Docker образа

```bash
# Для конкретного сервиса
docker build -t feane-<service-name> -f services/<service-name>/Dockerfile .

# Через docker-compose
docker-compose build <service-name>
```

---

## 🐛 Troubleshooting

### Gateway не запускается

**Проблема:** Duplicate route в ocelot.json

**Решение:**
```bash
# Проверить ocelot.json на дубликаты
cat services/gateway/ocelot.json | grep -A 5 "UpstreamPathTemplate"

# Пересобрать gateway
docker-compose build --no-cache feane-gateway
docker-compose up -d feane-gateway
```

### Analytics Service - Invalid object name

**Проблема:** Миграции не применились

**Решение:**
```bash
# Создать миграции
cd services/AnalyticsService
dotnet ef migrations add InitialCreate

# Пересобрать контейнер
docker-compose build --no-cache analytics-service
docker-compose up -d analytics-service
```

### "Unauthorized" при запросах к API

**Проблема:** Отсутствует или истек JWT токен

**Решение:**
1. Войти в систему: http://localhost:5003/account/authentication
2. Проверить токен в localStorage: `localStorage.getItem('jwtToken')`
3. Обновить токен через Refresh Token

### Frontend показывает "Loading..."

**Проблема:** Сервис не запущен или недоступен

**Решение:**
```bash
# Проверить статус
docker ps | grep <service-name>

# Проверить логи
docker logs feaneshop-<service-name>-1

# Перезапустить сервис
docker-compose restart <service-name>
```

### SQL Server н�� запускается в Docker

**Проблема:** Недостаточно памяти

**Решение:**
1. Увеличить память для Docker (минимум 4GB)
2. Проверить логи: `docker logs feaneshop-sqlserver-1`

---

## 📈 Метрики проекта

- **Общее количество сервисов:** 10 (9 app + 1 database)
- **Строк кода:** ~20,000+
- **API Endpoints:** 50+
- **Таблиц в БД:** 30+
- **Frontend страниц:** 15+
- **Docker образов:** 10
- **Портов:** 10 (5000-5003, 5020-5080, 1433)
- **Unit тестов:** 63+
- **Тестовых проектов:** 7
- **Test Coverage:** Controllers 100%

---

## 🚧 Известные ограничения

1. **Платежи** - имитация, реальная интеграция не реализована
2. **Email уведомления** - не реализованы
3. **SMS уведомления** - не реализованы
4. **Реальные изображения товаров** - используются placeholder'ы
5. **Продвинутый поиск** - базовая фильтрация
6. **Кеширование** - не реализовано (Redis может быть добавлен)
7. **Message Queue** - не реализовано (RabbitMQ может быть добавлен)

---

## 🔮 Roadmap

### Фаза 1 - Улучшения (Планируется)
- [ ] Real-time уведомления (SignalR)
- [ ] Email сервис (SendGrid/MailKit)
- [ ] SMS уведомления
- [ ] Интеграция с платежным gateway
- [ ] Продвинутый поиск (Elasticsearch)
- [ ] Кеширование (Redis)

### Фаза 2 - Масштабирование (Планируется)
- [ ] Message Queue (RabbitMQ)
- [ ] Event Sourcing
- [ ] CQRS с разделенными БД
- [ ] Kubernetes deployment
- [ ] Monitoring (Prometheus + Grafana)
- [ ] Distributed tracing (Jaeger)

### Фаза 3 - Расширения (Планируется)
- [ ] Mobile приложение (React Native)
- [ ] Admin Dashboard (React/Vue)
- [ ] Loyalty program
- [ ] Reviews and ratings
- [ ] Delivery tracking
- [ ] Multi-language support

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# coding conventions
- Use Clean Architecture principles
- Write unit tests for new features
- Update documentation

---

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 👥 Authors

- **Your Name** - Initial work

---

## 🙏 Acknowledgments

- ASP.NET Core team
- Ocelot contributors
- Entity Framework Core team
- Docker team
- Open source community

---

## 📞 Support

If you have any questions or issues, please:

1. Check the [Troubleshooting](#-troubleshooting) section
2. Search existing issues
3. Create a new issue with details

---

## 🔄 CI/CD Pipeline

Проект использует качественную CI/CD инфраструктуру для автоматизации процессов разработки и развертывания.

### 🚀 Workflow'ы

#### 1. **Continuous Integration (CI)**
- 🔍 **Code Quality Checks** - проверка качества кода и форматирования
- 🧪 **Build & Test** - сборка всех сервисов и запуск тестов с покрытием
- 🔒 **Security Scan** - проверка зависимостей на уязвимости
- 🐳 **Docker Build** - сборка Docker образов для всех сервисов
- 🔗 **Integration Tests** - интеграционное тестирование сервисов

#### 2. **Continuous Deployment (CD)**
- 📋 **Prepare Deployment** - подготовка версии и окружения
- 🐳 **Build & Push Images** - сборка и публикация Docker образов
- 🎯 **Deploy to Staging** - автоматическое развертывание в staging
- 🌟 **Deploy to Production** - развертывание в production (с ручным одобрением)
- 📊 **Post-Deployment** - проверка и мониторинг после развертывания

#### 3. **Pull Request Checks**
- 📋 **PR Information** - информация о PR и измененных файла��
- 🎨 **Lint & Format** - проверка форматирования кода
- 🔨 **Build Validation** - валидация сборки
- 🧪 **Unit Tests** - запуск юнит-тестов
- 🔒 **Security Check** - проверка безопасности
- 📊 **Code Coverage** - анализ покрытия кода тестами

#### 4. **Release Management**
- 🏷️ **Create Release** - создание релиза с changelog
- 🐳 **Build & Publish** - публикация Docker образов в registry
- 📦 **Release Artifacts** - создание артефактов релиза
- Поддержка semantic versioning (v1.0.0)

#### 5. **Nightly Build**
- 🌙 Ежедневная сборка всех сервисов (2:00 UTC)
- 🧪 Полный набор тестов
- 🔒 Аудит безопасности
- 📦 Проверка обновлений зависимостей
- 🐳 Тестирование Docker сборки
- 📊 Генерация метрик кода

#### 6. **Dependabot**
- 📦 Автоматическое обновление NuGet пакетов
- 🐳 Обновление Docker base images
- 🔄 Обновление GitHub Actions

### 🛠️ Используемые технологии

- **GitHub Actions** - платформа CI/CD
- **Docker** - контейнеризация
- **dotnet CLI** - сборка и тестирование
- **XPlat Code Coverage** - покрытие кода
- **Dependabot** - управление зависимостями

### 📊 Мониторинг качества

Все workflow'ы генерируют детальные отчеты:
- ✅ Статус сборки и тестов
- 📈 Покрытие кода тестами
- 🔒 Отчеты о безопасности
- 📦 Статус зависимостей
- 🐳 Статус Docker образов

### 🎯 Стратегия развертывания

```
main branch → CI → CD → Staging → Manual Approval → Production
develop branch → CI → CD → Staging
pull requests → PR Checks → Code Review → Merge
```

---

## 📊 Status

<div align="center">

✅ **PRODUCTION READY**

**Version:** 1.0.0  
**Last Updated:** November 2, 2025  
**Status:** Stable  

</div>

---

<div align="center">

**Built with ❤️ using .NET 9.0 and Microservices Architecture**

[⬆ Back to top](#️-feane-restaurant---full-stack-e-commerce-system)

</div>
