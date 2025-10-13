# 🍽️ Feane Shop (микросервисная версия)

Переосмысленная версия лабораторного проекта FeaneMVC. Вместо монолитного MVC-приложения теперь используется архитектура с
фронтовым шлюзом (BFF) и выделенными микросервисами меню и бронирований. Проект остаётся ориентированным на .NET 10 preview,
но код организован таким образом, чтобы каждый сервис можно было разворачивать и масштабировать независимо.

## 🧱 Архитектура

```
┌──────────────────────┐      HTTP       ┌──────────────────────┐
│  FeaneMVC (Gateway)  │  ───────────▶   │   Menu Service API   │
│  Razor + HttpClient  │  ◀───────────   │  (Feane.MenuService) │
└────────┬─────────────┘                 └──────────────────────┘
         │ HTTP                                       ▲
         │                                            │
         ▼                                            │
┌──────────────────────┐      HTTP       ┌────────────┴─────────┐
│ Reservation Service  │  ◀───────────   │  Shared Infrastructure│
│ (Feane.Reservation…) │  ───────────▶   │  & Contracts          │
└──────────────────────┘                 └──────────────────────┘
```

- **FeaneMVC** – фронтовой шлюз. Отвечает за пользовательский интерфейс, хранит анонимные сессии и общается с сервисами через
  `HttpClient`.
- **Feane.MenuService** – REST API для управления меню. Инкапсулирует CQRS-обработчики и инфраструктуру работы с БД.
- **Feane.ReservationService** – REST API бронирований. Хранит историю, создаёт записи и уведомляет пользователей.
- **Feane.Contracts** – общий пакет DTO, который переиспользуют все сервисы для обмена сообщениями.
- **FeaneMVC.Application / Domain / Infrastructure** – внутренние библиотеки, которые теперь подключаются только к микросервисам.

Такой подход исключает «распределённый монолит»: веб-приложение не имеет доступа ни к базе, ни к бизнес-логике – всё выполняют
микросервисы.

## 📂 Структура решения

```
FeaneMVC.sln
├── FeaneMVC/                  # Gateway-приложение (Razor, HttpClient, session-based user id)
├── Feane.Contracts/           # Общие DTO для HTTP-контрактов
├── Services/
│   ├── MenuService/           # ASP.NET Core Web API для операций с меню
│   └── ReservationService/    # ASP.NET Core Web API для бронирований
├── FeaneMVC.Application/      # CQRS-логика (используется в сервисах)
├── FeaneMVC.Domain/           # Доменные модели и enum'ы
└── FeaneMVC.Infrastructure/   # EF Core + репозитории, переиспользуются сервисами
```

## ⚙️ Ключевые изменения по сравнению с монолитом

- Веб-проект больше не содержит MediatR, EF Core и Identity – только Razor, HttpClient и конфигурация сессий.
- Контроллеры (`HomeController`, `DishController`, `ReservationController`) обращаются к микросервисам через typed-clients.
- Меню и бронирования опубликованы как отдельные API с собственными `Program.cs`, Swagger и CORS.
- DTO перемещены в библиотеку `Feane.Contracts`, чтобы избежать утечки кода между сервисами.
- README и appsettings содержат адреса сервисов (`ServiceEndpoints:MenuService`, `ServiceEndpoints:ReservationService`).

## 🚀 Запуск

1. Настройте строки подключения в `Services/MenuService/appsettings.json` и `Services/ReservationService/appsettings.json`.
2. Запустите каждый сервис (например, `dotnet run --project Services/MenuService/Feane.MenuService.csproj`).
3. Обновите адреса в `FeaneMVC/appsettings.json`, если сервисы работают на других портах.
4. Запустите фронтовой шлюз: `dotnet run --project FeaneMVC/FeaneMVC.csproj`.

> ⚠️ В учебной среде проекта нет docker-compose. Для полноценного запуска рекомендуется добавить оркестрацию (Docker Compose,
> Kubernetes) и сервис-реестр/обнаружение.

## 🔄 План дальнейшего развития

- Вынести пользователей/аутентификацию в отдельный сервис (Auth Service) и заменить локальные сессии JWT-токенами.
- Добавить gateway middleware для агрегации ответов и обработки ошибок сервисов.
- Настроить политику повторных попыток (`Polly`) для resilient-запросов из фронтового приложения.
- Подготовить docker-compose для поднятия всех сервисов одной командой.

---

Проект демонстрирует первый шаг перехода от монолита к микросервисам: BFF + два независимых API. Дальнейшее развитие предполагает
добавление новых сервисов и инфраструктурных компонентов (observability, service discovery, message bus).
