# 🍽️ FeaneMVC

Современное веб-приложение для онлайн-магазина еды с доставкой и бронированием столиков, построенное на ASP.NET Core с разделением на слои **Domain → Application → Infrastructure → Web**. Проект демонстрирует полноценный пользовательский и административный сценарии, интеграцию с платежами, уведомлениями и внешними сервисами.

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0_%28preview%29-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-9.0-512BD4?style=for-the-badge&logo=nuget)
![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC2927?style=for-the-badge&logo=microsoft-sql-server)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?style=for-the-badge&logo=docker)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

[🚀 Быстрый старт](#-быстрый-старт) • [🧱 Архитектура](#-архитектура) • [⚙️ Технологии](#-технологии) • [📂 Структура решения](#-структура-решения) • [🤝 Контрибьюция](#-контрибьюция)

</div>

---

## 📋 Описание

**FeaneMVC** реализует типичные сценарии e-commerce ресторана:

- показ меню, фильтрация и подбор блюд;
- управление корзиной, оформление и отслеживание заказов;
- бронирование столиков с историей и отменой;
- личный кабинет с управлением контактами и адресами доставки;
- прием и обработка платежей, фиксация транзакций;
- панель администратора с аналитикой, управлением меню и пользователями;
- уведомления клиентов, интеграция с OpenWeatherMap для отображения погоды.

Проект построен с использованием CQRS через MediatR, FluentValidation и Identity с поддержкой JWT-аутентификации и пользовательских сессий.

---

## 🧱 Архитектура

- **FeaneMVC.Domain** — сущности, value-object'ы, доменные сервисы и перечисления.
- **FeaneMVC.Application** — обработчики команд и запросов (MediatR), DTO, валидаторы и бизнес-правила.
- **FeaneMVC.Infrastructure** — EF Core контекст, миграции, репозитории, реализация Identity и внешние сервисы.
- **FeaneMVC (Web)** — MVC-контроллеры, Razor-представления, middleware и конфигурация веб-приложения.

Подход обеспечивает слабую связанность, модульность и удобство для unit/integration тестирования.

---

## ⚙️ Технологии

| Категория | Стек |
|-----------|------|
| Язык / Runtime | C# 13, .NET 10 (preview) |
| Веб | ASP.NET Core MVC, Razor Views |
| Доступ к данным | Entity Framework Core 9, Dapper |
| Аутентификация | ASP.NET Core Identity, JWT Bearer, Cookie auth |
| Валидация | FluentValidation |
| Паттерны | CQRS, Mediator, Repository, Unit of Work |
| Инфраструктура | SQL Server, Docker |
| UI | Bootstrap 5, jQuery |
| Интеграции | OpenWeatherMap API, email/notifications |

---

## ⚡ Быстрый старт

### 📦 Предварительные требования

- [.NET SDK 10.0 preview](https://dotnet.microsoft.com/) (проект таргетирует `net10.0`)
- SQL Server 2019+ (Express, LocalDB или полноценный сервер)
- Docker (опционально, для контейнеризации)
- Git

> 💡 Если вы используете стабильный .NET 8/9, переключите `TargetFramework` в `FeaneMVC.csproj` на соответствующую версию.

### 🔐 Настройка конфигурации

Скопируйте `FeaneMVC/appsettings.json` или `appsettings.Development.json` и задайте значения:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FeabeDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "OpenWeatherMap": {
    "ApiKey": "ВАШ_API_КЛЮЧ"
  },
  "JwtSettings": {
    "Issuer": "FeaneMVC",
    "Audience": "FeaneMVCUsers",
    "SecretKey": "ПРОИЗВОЛЬНАЯ_СТРОКА_32+_СИМВОЛА",
    "AccessTokenExpirationMinutes": 60,
    "CookieName": "AuthToken"
  }
}
```

Для безопасного хранения секретов используйте [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
cd FeaneMVC
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=FeabeDb;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "OpenWeatherMap:ApiKey" "<ваш ключ>"
dotnet user-secrets set "JwtSettings:SecretKey" "<секрет>"
```

### 🛠️ Локальный запуск

```bash
git clone https://github.com/Kwameldx666/FeaneMVC.git
cd FeaneMVC

dotnet restore
# Применяем миграции из проекта инфраструктуры
dotnet ef database update \
  --project FeaneMVC.Infrastructure \
  --startup-project FeaneMVC

dotnet run --project FeaneMVC
```

По умолчанию приложение доступно по адресам `https://localhost:5001` и `http://localhost:5000`.

### 🐳 Запуск в Docker

```bash
# Сборка образа
docker build -t feane-mvc .

# Запуск контейнера
docker run -it --rm -p 8080:80 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=FeabeDb;User Id=sa;Password=<пароль>;TrustServerCertificate=True" \
  -e JwtSettings__SecretKey="<секрет>" \
  feane-mvc
```

Приложение будет доступно по адресу `http://localhost:8080`.

---

## 📂 Структура решения

```
FeaneMVC.sln
├── FeaneMVC/                  # Веб-слой (Controllers, Views, Middleware, wwwroot)
│   ├── Configuration/         # Расширения для ServiceCollection и Middleware
│   ├── Controllers/           # MVC-контроллеры (меню, корзина, аккаунты, платежи, бронирования и др.)
│   ├── Middleware/            # Конвейер обработки запросов, обработка ошибок
│   ├── Views/                 # Razor-представления пользовательской части
│   └── wwwroot/               # Статические ресурсы (CSS, JS, изображения)
├── FeaneMVC.Application/      # CQRS-слой: команды, запросы, обработчики, валидация
├── FeaneMVC.Domain/           # Доменные сущности, enum'ы, value object'ы, сервисы
├── FeaneMVC.Infrastructure/   # EF Core контекст, миграции, репозитории, интеграции, Identity
└── Dockerfile                 # Описание контейнера для деплоя
```

---

## 🧪 Тестирование и проверки

В репозитории пока нет автоматических тестов. Планируется добавление unit- и integration-тестов для ключевых сценариев. После их появления запуск будет осуществляться командой:

```bash
dotnet test
```

Для проверки код-стайла можно использовать:

```bash
dotnet format --verify-no-changes
```

---

## 🚀 Дальнейшее развитие

- Покрытие бизнес-логики тестами.
- Настройка CI/CD (GitHub Actions) с прогоном `build`, `test`, `format`.
- Реализация реальных платёжных шлюзов и уведомлений.
- Локализация интерфейса и контента.

---

## 🤝 Контрибьюция

1. Сделайте форк репозитория.
2. Создайте ветку фичи: `git checkout -b feature/my-awesome-feature`.
3. Внесите изменения и зафиксируйте коммиты.
4. Прогоните тесты/проверки.
5. Откройте Pull Request, подробно описав изменения и шаги по проверке.

Пожалуйста, придерживайтесь принятого стиля кодирования и обновляйте документацию при необходимости.

---

## 📄 Лицензия

Проект распространяется по лицензии [MIT](LICENSE.txt).

---

## 📬 Контакты

- Email: [wonderful_by@bk.ru](mailto:wonderful_by@bk.ru)
- Баги и запросы: [GitHub Issues](https://github.com/Kwameldx666/FeaneMVC/issues)
- Если проект полезен — поставьте ⭐️!

<div align="center">

**Сделано с ❤️ для любителей вкусной еды**

</div>
