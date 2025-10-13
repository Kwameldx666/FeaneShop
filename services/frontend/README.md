# Frontend микросервис

Отдельный фронтенд сервис на React + Vite, отвечающий только за пользовательский интерфейс сценариев авторизации. Взаимодействует с gateway-сервисом, проксирующим запросы к микросервису аутентификации.

## Возможности

- формы регистрации и входа, работающие через REST API gateway
- отображение и обновление профиля текущего пользователя по токену
- хранение access-токена в `localStorage` и управление сессией на клиенте
- адаптивный UI с современным неоморфным оформлением без серверной логики

## Запуск в разработке

```bash
cd services/frontend
npm install
npm run dev -- --host 0.0.0.0 --port 5173
```

Перед запуском создайте файл `.env.local` на основе [.env.example](./.env.example) и укажите `VITE_GATEWAY_BASE_URL` (например, `http://localhost:5200`).

## Сборка и предпросмотр

```bash
npm run build
npm run preview -- --host 0.0.0.0 --port 4173
```

## Docker

Сборка и запуск контейнера:

```bash
docker build -t feane-frontend services/frontend

docker run -it --rm -p 8081:80 \
  -e VITE_GATEWAY_BASE_URL=http://gateway:80 \
  feane-frontend
```

Фронтенд статически обслуживается Nginx и не содержит серверной логики, все запросы направляются в gateway.
