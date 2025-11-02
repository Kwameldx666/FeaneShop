# 🔄 CI/CD Pipeline Guide

## Обзор

Проект использует единый унифицированный CI/CD pipeline на базе GitHub Actions для автоматизации всех процессов сборки, тестирования, проверки качества, сборки Docker образов, развертывания и релизов.

## 📋 Содержание

- [Workflow](#-workflow)
- [Настройка](#-настройка)
- [Использование](#-использование)
- [Jobs](#-jobs)
- [Лучшие практики](#-лучшие-практики)
- [Troubleshooting](#-troubleshooting)

---

## 🔄 Workflow

### Unified CI/CD Pipeline (`.github/workflows/main.yml`)

Единый workflow, который объединяет все необходимые процессы CI/CD в одном месте.

**Триггеры:**
- Push в ветки `main`, `develop`, `copilot/**`
- Push тегов `v*.*.*` (для релизов)
- Pull requests в ветки `main`, `develop`
- Ручной запуск с опцией выбора окружения для развертывания

**Переменные окружения:**
- `DOTNET_VERSION: '9.0.x'`
- `DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true`
- `DOTNET_CLI_TELEMETRY_OPTOUT: true`
- `REGISTRY: ghcr.io`

---

## 📦 Jobs

### 1. 🔨 Build Solution

**Назначение:** Сборка всего решения

**Шаги:**
- Checkout кода с полной историей
- Настройка .NET SDK
- Кэширование NuGet пакетов
- Восстановление зависимостей
- Сборка в Release конфигурации
- Генерация отчета о сборке

**Timeout:** 15 минут

**Кэширование:**
- Путь: `~/.nuget/packages`
- Ключ: `${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}`

---

### 2. 🧪 Run Tests

**Назначение:** Параллельное выполнение всех тестов

**Matrix Strategy:**
- `user-service`
- `product-service`
- `book-service`
- `reservation-service`
- `cart-service`
- `order-service`
- `analytics-service`

**Шаги:**
- Checkout кода
- Настройка .NET SDK
- Кэширование NuGet пакетов
- Восстановление зависимостей
- Сборка проекта
- Запуск тестов с генерацией .trx отчетов
- Загрузка результатов тестов как артефакты

**Timeout:** 20 минут

**Артефакты:**
- `test-results-*` - TRX файлы с результатами тестов
- Retention: 30 дней

**Настройки:**
- `fail-fast: false` - все тесты выполняются независимо
- `continue-on-error: true` - сбой тестов не прерывает pipeline

---

### 3. 🔍 Code Quality & Security

**Назначение:** Проверка качества кода и безопасности

**Шаги:**

#### a) Проверка форматирования кода
```bash
dotnet format --verify-no-changes --no-restore --verbosity diagnostic
```

#### b) Сканирование безопасности
```bash
dotnet list package --vulnerable --include-transitive
```

#### c) Статистика проекта
- Количество проектов
- Количество C# файлов
- Общее количество строк кода
- Количество сервисов

**Timeout:** 10 минут

**Отчеты:**
- Форматирование кода (предупреждения)
- Уязвимости в зависимостях
- Метрики проекта

---

### 4. 🐳 Docker Build

**Назначение:** Сборка Docker образов для всех сервисов

**Условия запуска:**
- Push в `main` или `develop`
- Push тега `v*.*.*`

**Matrix Strategy:**
Все сервисы:
- user-service
- product-service
- book-service
- reservation-service
- cart-service
- order-service
- analytics-service
- gateway
- frontend

**Шаги:**
- Checkout кода
- Настройка Docker Buildx
- Авторизация в Container Registry (только для тегов)
- Генерация тегов Docker
- Сборка образа
- Push образа (только для тегов)

**Docker Tags:**
- `SHA` - всегда
- `version` - для тегов
- `latest` - для тегов
- `main` - для main ветки
- `develop` - для develop ветки

**Timeout:** 45 минут

**Кэширование:**
- Тип: GitHub Actions cache
- Mode: max

---

### 5. 🚀 Deploy

**Назначение:** Развертывание приложения

**Условия запуска:**
- Ручной запуск с выбором окружения (staging/production)
- Push в `main` (автоматически в staging)

**GitHub Environments:**
- `staging` - https://staging.feane.app
- `production` - https://feane.app

**Шаги:**
- Checkout кода
- Генерация версии (YYYY.MM.DD-SHA)
- Определение целевого окружения
- Развертывание (симуляция)
- Health checks
- Генерация отчета о развертывании

**Timeout:** 20 минут

**Версионирование:**
```bash
VERSION=$(date +'%Y.%m.%d')-${GITHUB_SHA::7}
```

---

### 6. 📦 Create Release

**Назначение:** Создание GitHub Release

**Условия запуска:**
- Push тега `v*.*.*`

**Permissions:**
- `contents: write`

**Шаги:**
- Checkout кода с полной историей
- Извлечение версии из тега
- Генерация changelog
- Создание GitHub Release

**Changelog:**
- Список изменений с предыдущего тега
- Информация о Docker образах
- Ссылки на коммиты

---

### 7. 📋 Pipeline Summary

**Назначение:** Итоговый отчет о выполнении pipeline

**Условия:** Всегда выполняется (`if: always()`)

**Отчет включает:**
- Статус всех jobs
- Детали pipeline (branch, event, actor)
- Ссылки на артефакты и логи

---

## ⚙️ Настройка

### 1. Repository Settings

#### Secrets
Для полной функциональности требуется:

| Secret | Описание | Использование |
|--------|----------|---------------|
| `GITHUB_TOKEN` | Автоматически предоставляется | Docker registry, Releases |

#### Environments

**Staging Environment:**
```yaml
name: staging
url: https://staging.feane.app
protection_rules: none
```

**Production Environment:**
```yaml
name: production
url: https://feane.app
protection_rules:
  - required_reviewers: 1
  - wait_timer: 5
```

#### Actions Permissions
- Workflows: Read and write permissions
- Allow GitHub Actions to create and approve pull requests: ✓

---

### 2. Branch Protection Rules

**Main Branch:**
- Require pull request reviews before merging
- Require status checks to pass before merging:
  - `Build Solution`
  - `Run Tests`
  - `Code Quality & Security`
- Require conversation resolution before merging
- Do not allow bypassing the above settings

**Develop Branch:**
- Require status checks to pass before merging:
  - `Build Solution`
  - `Run Tests`

---

## 🚀 Использование

### Создание Pull Request

1. Создайте feature branch:
```bash
git checkout -b feature/your-feature
```

2. Внесите изменения и закоммитьте:
```bash
git add .
git commit -m "feat: описание изменений"
```

3. Отправьте изменения:
```bash
git push origin feature/your-feature
```

4. Создайте Pull Request на GitHub

**Автоматически запустится:**
- ✅ Build Solution
- ✅ Run Tests
- ✅ Code Quality & Security

---

### Merge в Main

При merge PR в `main`:

**Автоматически:**
1. ✅ Build
2. ✅ Tests
3. ✅ Quality checks
4. 🐳 Docker build
5. 🚀 Deploy to Staging

---

### Ручное развертывание

1. Перейдите в Actions → "CI/CD Pipeline"
2. Нажмите "Run workflow"
3. Выберите:
   - Branch (main/develop)
   - Deploy environment (none/staging/production)
4. Нажмите "Run workflow"

**Примеры:**
- Deploy to staging: выберите `main` + `staging`
- Deploy to production: выберите `main` + `production`
- Only build & test: выберите любую ветку + `none`

---

### Создание Release

1. Определите версию (semver):
```bash
VERSION=v1.2.3
```

2. Создайте и отправьте тег:
```bash
git tag -a $VERSION -m "Release $VERSION"
git push origin $VERSION
```

3. Pipeline автоматически:
   - Соберет проект
   - Прогонит тесты
   - Создаст Docker образы
   - Создаст GitHub Release
   - Опубликует образы в registry

---

## 💡 Лучшие практики

### Commit Messages

Используйте conventional commits:
```
feat: новая функциональность
fix: исправление бага
docs: изменения в документации
style: форматирование кода
refactor: рефакторинг
test: добавление тестов
chore: обновление зависимостей, конфигурации
```

### Branching Strategy

```
main
  ├── develop
  │   ├── feature/user-auth
  │   ├── feature/order-system
  │   └── fix/payment-bug
  └── hotfix/critical-fix
```

**Правила:**
- `main` - production-ready код
- `develop` - интеграционная ветка
- `feature/*` - новые функции
- `fix/*` - исправления багов
- `hotfix/*` - критические исправления для main
- `copilot/*` - автоматические изменения от GitHub Copilot

### Версионирование

Семантическое версионирование (SemVer):
```
v{MAJOR}.{MINOR}.{PATCH}

MAJOR - breaking changes
MINOR - новая функциональность (обратно совместимая)
PATCH - исправления (обратно совместимые)

Примеры:
v1.0.0 - первый релиз
v1.1.0 - новая функция
v1.1.1 - исправление бага
v2.0.0 - breaking change
```

### Testing

- Покрытие кода минимум 70%
- Все тесты должны проходить перед merge
- Добавляйте тесты для новой функциональности
- Обновляйте существующие тесты при изменении логики

### Docker Images

**Naming:**
```
feane/{service-name}:{tag}

Примеры:
feane/user-service:latest
feane/user-service:v1.2.3
feane/user-service:main
feane/user-service:develop
feane/user-service:abc123
```

**Best Practices:**
- Используйте multi-stage builds
- Минимизируйте размер образов
- Не включайте sensitive data
- Используйте .dockerignore

---

## 🔧 Troubleshooting

### Build Failed

**Проблема:** Сборка падает с ошибкой

**Решения:**
1. Проверьте логи сборки:
   ```bash
   dotnet build --verbosity detailed
   ```

2. Убедитесь, что все зависимости восстановлены:
   ```bash
   dotnet restore
   ```

3. Проверьте версию .NET SDK:
   ```bash
   dotnet --version
   ```

4. Очистите кэш:
   ```bash
   dotnet clean
   rm -rf ~/.nuget/packages
   ```

### Tests Failed

**Проблема:** Тесты не проходят

**Решения:**
1. Запустите тесты локально:
   ```bash
   dotnet test --verbosity normal
   ```

2. Проверьте конкретный проект:
   ```bash
   dotnet test services/user-service/UserService.Tests
   ```

3. Посмотрите детали ошибок в TRX файлах (артефакты)

4. Убедитесь, что тестовые данные актуальны

### Docker Build Failed

**Проблема:** Не удается собрать Docker образ

**Решения:**
1. Проверьте Dockerfile синтаксис
2. Убедитесь, что все файлы существуют
3. Проверьте .dockerignore
4. Соберите локально:
   ```bash
   docker build -t test-image .
   ```

### Deployment Failed

**Проблема:** Развертывание не удается

**Решения:**
1. Проверьте логи развертывания
2. Убедитесь, что environment настроен
3. Проверьте permissions для GITHUB_TOKEN
4. Проверьте доступность целевого окружения

### Артефакты не загружаются

**Проблема:** Test results не появляются

**Решения:**
1. Проверьте путь к артефактам
2. Убедитесь, что тесты генерируют TRX файлы
3. Проверьте права на artifacts (Actions permissions)

---

## 📊 Метрики

### Время выполнения

| Job | Среднее время | Timeout |
|-----|---------------|---------|
| Build | 3-5 мин | 15 мин |
| Tests | 5-10 мин | 20 мин |
| Quality | 2-3 мин | 10 мин |
| Docker | 15-30 мин | 45 мин |
| Deploy | 5-10 мин | 20 мин |
| Release | 2-5 мин | - |

**Общее время pipeline:** 20-40 минут

### Оптимизация

**Кэширование:**
- NuGet packages: ~2-3 минуты экономии
- Docker layers: ~5-10 минут экономии

**Matrix Strategy:**
- Параллельное выполнение тестов: 8x быстрее

**Best Practices:**
- Используйте `continue-on-error` для не критичных jobs
- Настройте правильные timeouts
- Оптимизируйте Docker образы
- Кэшируйте зависимости

---

## 🔗 Полезные ссылки

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Build Push Action](https://github.com/docker/build-push-action)
- [.NET CLI Reference](https://docs.microsoft.com/en-us/dotnet/core/tools/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)

---

## 📝 Changelog

### 2024.11.02
- ✨ Создан единый унифицированный CI/CD pipeline
- 🔄 Объединены все workflows в один (main.yml)
- 🗑️ Удалены отдельные workflows: ci.yml, cd.yml, pr-checks.yml, release.yml, nightly.yml, tests.yml
- 📝 Обновлена документация

---

## 🤝 Contributing

Если у вас есть предложения по улучшению CI/CD pipeline:

1. Создайте Issue с описанием предложения
2. Обсудите изменения с командой
3. Создайте PR с изменениями
4. Обновите документацию

---

## 📧 Контакты

Вопросы по CI/CD? Создайте Issue или обратитесь к maintainer'ам проекта.
