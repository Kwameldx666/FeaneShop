# 🔄 CI/CD Pipeline Guide

## Обзор

Проект использует комплексную CI/CD инфраструктуру на базе GitHub Actions для автоматизации процессов сборки, тестирования и развертывания.

## 📋 Содержание

- [Workflows](#-workflows)
- [Настройка](#-настройка)
- [Использование](#-использование)
- [Лучшие практики](#-лучшие-практики)
- [Troubleshooting](#-troubleshooting)

---

## 🔄 Workflows

### 1. Continuous Integration (`.github/workflows/ci.yml`)

**Триггеры:**
- Push в ветки `main`, `develop`
- Pull requests в ветки `main`, `develop`
- Ручной запуск

**Jobs:**

#### 📍 Code Quality Checks
- Проверка форматирования кода (`dotnet format`)
- Генерация статистики проекта
- Анализ качества кода

**Артефакты:** нет

#### 📍 Build & Test
- Параллельное тестирование всех сервисов (matrix strategy)
- Сборка в Release конфигурации
- Запуск юнит-тестов с покрытием кода
- Кэширование NuGet пакетов

**Артефакты:**
- `test-results-*` - результаты тестов (.trx)
- `coverage-*` - отчеты покрытия кода

#### 📍 Test Summary
- Агрегация результатов тестов
- Публикация сводного отчета

#### 📍 Security Scan
- Сканирование зависимостей на уязвимости
- Генерация отчета безопасности

**Артефакты:**
- `security-report` - отчет безопасности

#### 📍 Docker Build
- Параллельная сборка Docker образов всех сервисов
- Использование GitHub Actions cache
- Проверка работоспособности сборки

#### 📍 Integration Test
- Запуск всех сервисов через Docker Compose
- Health check проверки
- Только для main ветки

**Время выполнения:** ~15-20 минут

---

### 2. Continuous Deployment (`.github/workflows/cd.yml`)

**Триггеры:**
- Успешное выполнение CI workflow (для main)
- Ручной запуск с выбором окружения

**Jobs:**

#### 📍 Prepare Deployment
- Определение целевого окружения (staging/production)
- Генерация версии (YYYY.MM.DD-SHA)
- Создание deployment summary

**Outputs:**
- `environment` - целевое окружение
- `version` - версия для развертывания

#### 📍 Build & Push Images
- Сборка Docker образов для всех сервисов
- Тегирование образов (версия, latest, branch-sha)
- Публикация в GitHub Container Registry (опционально)

**Требования:**
- `GITHUB_TOKEN` для авторизации в registry

#### 📍 Deploy Staging
- Автоматическое развертывание в staging
- Проверка развертывания
- Environment: `staging`
- URL: https://staging.feane.app

#### 📍 Deploy Production
- Развертывание в production с ручным одобрением
- Environment: `production`
- URL: https://feane.app

**Требования:**
- Ручное одобрение в GitHub UI

#### 📍 Post-Deployment
- Сводка развертывания
- Рекомендации по мониторингу

**Время выполнения:** ~10-15 минут

---

### 3. Pull Request Checks (`.github/workflows/pr-checks.yml`)

**Триггеры:**
- Открытие PR
- Обновление PR
- Повторное открытие PR

**Jobs:**

#### 📍 PR Info
- Информация о PR
- Список измененных файлов

#### 📍 Lint & Format
- Проверка форматирования кода
- Рекомендации по исправлению

#### 📍 Build Validation
- Полная сборка решения
- Проверка отсутствия ошибок компиляции

#### 📍 Unit Tests
- Запуск всех юнит-тестов
- Публикация результатов в PR

#### 📍 Security Check
- Проверка зависимостей на уязвимости
- Отчет в PR

#### 📍 Code Coverage
- Анализ покрытия кода
- Генерация отчетов

**Артефакты:**
- `code-coverage` - отчеты покрытия

**Время выполнения:** ~10-15 минут

---

### 4. Release Management (`.github/workflows/release.yml`)

**Триггеры:**
- Push тега `v*.*.*` (например, v1.0.0)
- Ручной запуск с указанием версии

**Jobs:**

#### 📍 Create Release
- Генерация changelog из коммитов
- Создание GitHub Release
- Прикрепление changelog

#### 📍 Build & Publish
- Сборка и публикация всех Docker образов
- Тегирование semantic versioning
- Публикация в registry

#### 📍 Release Artifacts
- Сборка release конфигурации
- Создание артефактов для скачивания

**Время выполнения:** ~15-20 минут

---

### 5. Nightly Build (`.github/workflows/nightly.yml`)

**Триггеры:**
- Расписание: ежедневно в 2:00 UTC
- Ручной запуск

**Jobs:**

#### 📍 Nightly Build
- Сборка в Debug и Release
- Проверка сборки

#### 📍 Nightly Tests
- Полный набор тестов с детальным выводом
- Длительное хранение результатов (90 дней)

#### 📍 Security Audit
- Глубокая проверка безопасности
- Отчет по уязвимостям

#### 📍 Dependency Check
- Проверка устаревших пакетов
- Рекомендации по обновлению

#### 📍 Docker Build Test
- Тестовая сборка через Docker Compose
- Проверка работоспособности

#### 📍 Code Metrics
- Статистика проекта
- Git метрики
- Отчет о размере кодовой базы

**Артефакты:**
- `nightly-test-results` (90 дней)
- `security-audit-report` (90 дней)
- `dependency-report` (90 дней)
- `code-metrics-report` (90 дней)

**Время выполнения:** ~25-30 минут

---

### 6. Dependabot (`.github/dependabot.yml`)

**Конфигурация:**

#### NuGet пакеты
- Проверка: еженедельно (понедельник, 9:00)
- Лимит PR: 10
- Стратегия версионирования: increase

#### Docker образы
- Проверка каждого сервиса: еженедельно
- Автоматические PR для обновления base images

#### GitHub Actions
- Проверка: еженедельно
- Обновление версий actions

---

## ⚙️ Настройка

### Требования

1. **GitHub Repository Settings:**
   - Actions включены
   - Permissions для GITHUB_TOKEN:
     - contents: write
     - packages: write

2. **Environments (опционально):**
   ```
   staging:
     - URL: https://staging.feane.app
     - Без ограничений
   
   production:
     - URL: https://feane.app
     - Required reviewers: [@your-team]
     - Deployment branch: main
   ```

3. **Secrets (опционально):**
   - `GITHUB_TOKEN` - создается автоматически
   - Дополнительные secrets для deployment

### Первый запуск

1. **Проверка workflows:**
   ```bash
   # Проверить синтаксис всех workflows
   git ls-files .github/workflows/*.yml | xargs -I {} sh -c 'echo "Checking {}" && yamllint {}'
   ```

2. **Тестовый запуск:**
   - Перейдите в Actions
   - Выберите "Continuous Integration"
   - Нажмите "Run workflow"
   - Выберите ветку
   - Нажмите "Run workflow"

3. **Настройка Dependabot:**
   - Dependabot настроен автоматически
   - PR будут создаваться автоматически

---

## 🚀 Использование

### Ежедневная разработка

1. **Создание feature branch:**
   ```bash
   git checkout -b feature/my-feature
   # Внесите изменения
   git add .
   git commit -m "feat: add new feature"
   git push origin feature/my-feature
   ```

2. **Создание Pull Request:**
   - Откройте PR в GitHub
   - Автоматически запустятся PR Checks
   - Дождитесь успешного выполнения
   - Запросите code review

3. **Merge в develop/main:**
   - После одобрения, merge PR
   - Автоматически запустится CI
   - Для main: автоматически запустится CD в staging

### Release процесс

1. **Подготовка релиза:**
   ```bash
   # Убедитесь что все тесты проходят
   git checkout main
   git pull origin main
   ```

2. **Создание тега:**
   ```bash
   git tag -a v1.0.0 -m "Release version 1.0.0"
   git push origin v1.0.0
   ```

3. **Автоматический процесс:**
   - Запустится Release workflow
   - Создастся GitHub Release
   - Соберутся и опубликуются Docker образы
   - Changelog сгенерируется автоматически

4. **Production deployment:**
   - Перейдите в Actions → CD workflow
   - Выберите "Run workflow"
   - Установите environment: production
   - Одобрите deployment в GitHub UI

### Мониторинг

1. **Проверка статуса:**
   - README.md отображает badges статуса
   - GitHub Actions → текущие runs

2. **Просмотр отчетов:**
   - Artifacts в completed runs
   - Job summaries в каждом run

3. **Анализ ошибок:**
   - Logs в failed jobs
   - Annotations в PR checks

---

## ✅ Лучшие практики

### Коммиты

```bash
# Используйте conventional commits
feat: add new feature
fix: resolve bug in user service
docs: update CI/CD documentation
ci: improve build performance
test: add unit tests for cart service
refactor: optimize product service
perf: improve query performance
style: fix code formatting
```

### Pull Requests

1. **Дождитесь завершения всех checks**
2. **Исправьте все найденные проблемы**
3. **Запросите code review от команды**
4. **Merge только после одобрения**

### Deployment

1. **Staging first:** всегда деплойте в staging перед production
2. **Smoke tests:** проверьте основную функциональность после deployment
3. **Мониторинг:** следите за метриками после deployment
4. **Rollback plan:** будьте готовы к откату

### Безопасность

1. **Регулярно проверяйте отчеты безопасности**
2. **Обновляйте зависимости** через Dependabot PR
3. **Не храните секреты в коде**
4. **Используйте GitHub Secrets** для sensitive data

---

## 🔧 Troubleshooting

### CI падает с ошибкой сборки

**Проблема:** `dotnet build` завершается с ошибками

**Решение:**
```bash
# Локально проверьте сборку
dotnet restore
dotnet build --configuration Release

# Проверьте версию .NET
dotnet --version  # Должна быть 9.0.x
```

### Тесты не проходят в CI

**Проблема:** Тесты проходят локально, но падают в CI

**Решение:**
1. Проверьте зависимости тестов от environment
2. Убедитесь что тесты не зависят друг от друга
3. Проверьте connection strings и пути
4. Запустите тесты локально в Docker

### Docker build timeout

**Проблема:** Docker сборка превышает timeout

**Решение:**
1. Увеличьте timeout в workflow
2. Оптимизируйте Dockerfile (multi-stage builds)
3. Используйте .dockerignore
4. Проверьте размер зависимостей

### Dependabot PR конфликты

**Проблема:** Dependabot PR имеют конфликты

**Решение:**
```bash
# Rebase Dependabot branch
gh pr checkout <pr-number>
git fetch origin main
git rebase origin/main
git push --force-with-lease
```

### Failed deployment

**Проблема:** Deployment завершился с ошибкой

**Решение:**
1. Проверьте logs в failed job
2. Проверьте health checks сервисов
3. Проверьте connection strings
4. Rollback к предыдущей версии:
   ```bash
   # Deployment через Docker
   docker-compose down
   git checkout <previous-tag>
   docker-compose up -d
   ```

---

## 📊 Метрики и KPI

### Рекомендуемые метрики

- **Build Success Rate:** >95%
- **Test Success Rate:** >98%
- **Average Build Time:** <20 минут
- **Code Coverage:** >80%
- **Deployment Frequency:** ежедневно (staging), еженедельно (production)
- **Mean Time to Recovery:** <1 час
- **Change Failure Rate:** <5%

### Мониторинг

```yaml
# Metrics отслеживаемые в workflows:
- Build time
- Test execution time
- Docker build time
- Number of tests
- Code coverage percentage
- Security vulnerabilities
- Outdated packages
```

---

## 🔗 Полезные ссылки

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [.NET Testing Guide](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)

---

## 🤝 Contributing

При внесении изменений в CI/CD:

1. Тестируйте изменения в feature branch
2. Документируйте изменения в этом файле
3. Обновите README.md при необходимости
4. Создайте PR с описанием изменений

---

**Last Updated:** November 1, 2025  
**Version:** 1.0.0  
**Maintainer:** @Kwameldx666
