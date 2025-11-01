# CI/CD Pipeline для FeaneShop

Полная документация по настроенному CI/CD процессу для проекта FeaneShop.

## 📊 Обзор пайплайнов

### 1. **CI (Continuous Integration)** - `ci.yml`
Запускается при:
- ✅ Push в ветки `master` и `develop`
- ✅ Pull Request в эти ветки

**Что делает:**
- Сборка .NET проекта (C#)
- Прогон unit-тестов
- Генерация отчетов о тестировании
- Проверка качества кода
- Кэширование NuGet пакетов

---

### 2. **CD (Continuous Deployment)** - `cd.yml`
Запускается при:
- ✅ Push в ветку `master`
- ✅ Создание тегов версий (v*.*)
- ✅ Ручной запуск (workflow_dispatch)

**Что делает:**
- Сборка Docker-образа
- Публикация в Docker Hub и GitHub Container Registry
- Автоматический деплой на сервер
- Кэширование Docker слоев

---

### 3. **Security Checks** - `security.yml`
Запускается при:
- ✅ Push и PR в основные ветки
- ✅ По расписанию (еженедельно)

**Что делает:**
- Проверка на устаревшие NuGet пакеты
- Сканирование уязвимостей через Trivy
- CodeQL анализ безопасности

---

### 4. **Docker Build** - `docker-build.yml`
Запускается при:
- ✅ Изменение Dockerfile, docker-compose.yml или src/
- ✅ Pull Request с этими изменениями

**Что делает:**
- Сборка и тестирование Docker-образа
- Кэширование слоев для ускорения
- Публикация образа в реестры

---

## 🔐 Необходимые GitHub Secrets

Перейдите в **Settings → Secrets and variables → Actions** и создайте:

### Для Docker публикации:
```
DOCKER_USERNAME          # Имя пользователя Docker Hub
DOCKER_PASSWORD          # Пароль/токен Docker Hub
```

### Для деплоя на сервер:
```
DEPLOY_SERVER_IP         # IP адрес сервера (например: 192.168.1.100)
DEPLOY_SERVER_USER       # SSH пользователь (например: deploy)
DEPLOY_SERVER_KEY        # SSH приватный ключ
```

---

## 🚀 Как начать использовать

### Шаг 1: Добавьте Secrets
1. Перейдите в GitHub репозиторий
2. **Settings** → **Secrets and variables** → **Actions**
3. Добавьте необходимые secrets (см. выше)

### Шаг 2: Merge Pull Request
Все workflow файлы готовы в PR. Просто merge в master.

### Шаг 3: Протестируйте
```bash
# Создайте новый branch и сделайте простое изменение
git checkout -b feature/test-ci
echo "test" >> README.md
git add .
git commit -m "Test CI pipeline"
git push origin feature/test-ci

# Откройте PR и посмотрите выполнение workflow в Actions tab
```

---

## 📈 Отслеживание результатов

### GitHub Actions Dashboard
1. Перейдите в репозиторий → **Actions**
2. Выберите нужный workflow
3. Посмотрите:
   - ✅ Статус каждого шага
   - 📊 Логи выполнения
   - 📁 Артефакты (test results и т.д.)
   - 🔍 Ошибки и warnings

### Значки статуса в README
Добавьте эти строки в README.md:

```markdown
[![CI](https://github.com/Kwameldx666/FeaneShop/actions/workflows/ci.yml/badge.svg)](https://github.com/Kwameldx666/FeaneShop/actions/workflows/ci.yml)
[![CD](https://github.com/Kwameldx666/FeaneShop/actions/workflows/cd.yml/badge.svg)](https://github.com/Kwameldx666/FeaneShop/actions/workflows/cd.yml)
[![Security](https://github.com/Kwameldx666/FeaneShop/actions/workflows/security.yml/badge.svg)](https://github.com/Kwameldx666/FeaneShop/actions/workflows/security.yml)
```

---

## 🔧 Настройка под ваши нужды

### Изменение веток для деплоя
В `cd.yml` найдите и измените:
```yaml
on:
  push:
    branches:
      - master  # ← Измените на нужную ветку
```

### Изменение версии .NET
В `ci.yml` найдите:
```yaml
matrix:
  dotnet-version: ['8.0']  # ← Измените версию
```

### Отключение автоматического деплоя
В `cd.yml` удалите `deploy` job

---

## 🆘 Решение проблем

### Workflow не запускается
1. Проверьте имя ветки (должна быть `master` или `develop`)
2. Проверьте, что файлы в `.github/workflows/` имеют расширение `.yml`

### Ошибка при push Docker образа
1. Проверьте `DOCKER_USERNAME` и `DOCKER_PASSWORD` secrets
2. Убедитесь, что учетные данные правильные

### Ошибка при деплое
1. Проверьте `DEPLOY_SERVER_IP` и `DEPLOY_SERVER_USER`
2. Проверьте SSH ключ (должен быть приватным ключом)
3. Убедитесь, что сервер доступен с GitHub (firewall)

---

## ✨ Готово!

Ваш проект теперь имеет **полнофункциональный CI/CD** 🎉