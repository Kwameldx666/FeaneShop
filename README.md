# 🍽️ Feane Shop (микросервисная версия)

Современное приложение для онлайн-магазина еды с доставкой и бронированием столиков, преобразованное в микросервисную архитектуру. Каждый доменный модуль развёрнут как независимый сервис, а единая точка входа реализована через API Gateway.

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Minimal_API-512BD4?style=for-the-badge&logo=dotnet)
![Microservices](https://img.shields.io/badge/Architecture-Microservices-orange?style=for-the-badge)
![HTTP](https://img.shields.io/badge/Protocol-REST-0d8abc?style=for-the-badge)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED?style=for-the-badge&logo=docker)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

[🚀 Быстрый старт](#-быстрый-старт) • [🧱 Архитектура](#-архитектура) • [⚙️ Технологии](#-технологии) • [📂 Структура решения](#-структура-решения) • [🤝 Контрибьюция](#-контрибьюция)

</div>

---

## 📋 Описание

Микросервисы покрывают основные сценарии e-commerce ресторана:

- показ меню, фильтрация и подбор блюд;
- управление корзиной, оформление и отслеживание заказов;
- бронирование столиков с историей и отменой;
- личный кабинет с управлением контактами и адресами доставки;
- прием и обработка платежей, фиксация транзакций;
- панель администратора и расширенные интеграции планируются как отдельные сервисы (вне текущего объёма).

Сервисы реализованы на ASP.NET Core Minimal API, снабжены Swagger для документации и запускаются независимо друг от друга (через `docker compose` или `dotnet run`).

---

## 🧱 Архитектура

Проект разделён на независимые микросервисы, каждый со своим REST API и жизненным циклом:

- **CatalogService** — отвечает за меню и категории блюд.
- **OrderingService** — принимает заказы и управляет статусами.
- **ReservationService** — ведёт учёт бронирований столиков.
- **Feane.Gateway.Api** — API Gateway/BFF, предоставляющий фронтенду единый контракт.

У каждого сервиса собственная кодовая база, Dockerfile и параметры запуска. Совместное использование кода исключено — вместо этого обмен идёт по HTTP, что предотвращает появление "распределённого монолита".

---

## ⚙️ Технологии

| Категория | Стек |
|-----------|------|
| Язык / Runtime | C# 12, .NET 8 |
| Веб | ASP.NET Core Minimal APIs |
| Доступ к данным | In-memory (для демо), планируется отдельная БД на сервис |
| Аутентификация | Подключается через Gateway (будущая интеграция) |
| Паттерны | BFF/API Gateway, Service per Bounded Context |
| Инфраструктура | Docker, docker-compose |
| Мониторинг | Swagger/OpenAPI для каждого сервиса |

---

## ⚡ Быстрый старт

### 📦 Предварительные требования

- [.NET SDK 8.0](https://dotnet.microsoft.com/)
- Docker / Docker Compose (для локального оркестрирования)
- Git

### 🔐 Настройка конфигурации

Текущая демо-реализация использует in-memory хранилища и не требует внешних зависимостей. Адреса downstream-сервисов для шлюза задаются переменными окружения `Downstream__Catalog`, `Downstream__Ordering`, `Downstream__Reservation` (см. `docker-compose.yml`).

### 🛠️ Локальный запуск

```bash
git clone https://github.com/Kwameldx666/FeaneMVC.git
cd FeaneMVC

# Запуск всех сервисов
docker compose up --build

# Локальный запуск одного сервиса (пример)
dotnet run --project services/CatalogService/CatalogService.Api
```

После запуска docker-compose сервисы доступны по адресам:

- Gateway: http://localhost:8080
- CatalogService: http://localhost:5001
- OrderingService: http://localhost:5002
- ReservationService: http://localhost:5003

### 🐳 Запуск в Docker

```bash
# Сборка и запуск всех сервисов
docker compose up --build

# Перезапуск одного сервиса после изменений
docker compose up --build catalog-api
```

Gateway доступен по адресу `http://localhost:8080`, остальные сервисы — на портах `5001-5003`.

---

## 📂 Структура репозитория

```
FeaneMicroservices.sln         # решение с микросервисами
docker-compose.yml             # оркестрация сервисов
gateway/
  ├── Dockerfile
  └── Feane.Gateway.Api/       # BFF/API Gateway
services/
  ├── CatalogService/
  │   ├── Dockerfile
  │   └── CatalogService.Api/  # сервис каталога меню
  ├── OrderingService/
  │   ├── Dockerfile
  │   └── OrderingService.Api/ # сервис заказов
  └── ReservationService/
      ├── Dockerfile
      └── ReservationService.Api/ # сервис резерваций
docs/
  └── microservices-architecture.md # подробности декомпозиции
FeaneMVC*/                     # исходный монолит (для справки, постепенно выносится)
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

- Покрытие каждого сервиса модульными и контрактными тестами.
- Настройка CI/CD (GitHub Actions) с прогоном `dotnet build`, `dotnet test`, `docker build` для всех сервисов.
- Добавление брокера сообщений и интеграция реальных платёжных шлюзов/уведомлений.
- Локализация интерфейса и контента поверх Gateway.

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
