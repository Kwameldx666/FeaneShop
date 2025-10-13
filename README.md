# 🍽️ Feane Shop Platform

Feane Shop evolves the original monolithic MVC project into a microservice-first architecture tailored for food ordering, checkout, and delivery flows. Each bounded context is packaged as an independent service with its own deployable unit, tests, configuration, and container image.

## 🧭 Solution Overview

| Layer | Responsibility |
|-------|----------------|
| `/services/user-service` | Registration, authentication primitives, and profile storage |
| `/services/product-service` | Catalog, categories, and menu management |
| `/services/cart-service` | Cart sessions, line-item management |
| `/services/order-service` | Checkout, order lifecycle, status updates |
| `/services/payment-service` | Payment authorization abstraction |
| `/services/delivery-service` | Delivery tracking and status transitions |
| `/services/frontend` | Lightweight Node/Express UI for the storefront |
| `/services/gateway` | HTTP API gateway that fronts downstream services |
| `/shared` | Place for cross-cutting DTOs/utilities kept intentionally small |

The legacy MVC solution (`FeaneMVC`, `FeaneMVC.Domain`, etc.) remains available for reference but the recommended approach is to interact with the platform exclusively via the gateway and frontend services.

## 📂 Repository Structure

```
services/
├── cart-service/
│   ├── src/                # ASP.NET Core Web API
│   ├── tests/              # xUnit tests
│   ├── Dockerfile
│   └── appsettings.json
├── delivery-service/
│   ├── src/
│   ├── tests/
│   ├── Dockerfile
│   └── appsettings.json
├── frontend/
│   ├── public/
│   ├── src/
│   ├── Dockerfile
│   └── package.json
├── gateway/
│   ├── src/
│   ├── Dockerfile
│   └── appsettings.json
├── order-service/
│   ├── src/
│   ├── tests/
│   ├── Dockerfile
│   └── appsettings.json
├── payment-service/
│   ├── src/
│   ├── tests/
│   ├── Dockerfile
│   └── appsettings.json
├── product-service/
│   ├── src/
│   ├── tests/
│   ├── Dockerfile
│   └── appsettings.json
└── user-service/
    ├── src/
    ├── tests/
    ├── Dockerfile
    └── appsettings.json

shared/
├── models/
└── utils/

docker-compose.yml
```

Every service exposes `/health` endpoints for liveness checks and sample controllers with in-memory data stores to illustrate service boundaries. Each `.csproj` targets `net10.0` and is ready to be extended with storage, messaging, or observability integrations.

## 🚀 Getting Started

### Prerequisites

- .NET SDK 10 preview (or align the `TargetFramework` to your installed runtime)
- Node.js 20+ (for the frontend service)
- Docker (optional, for containerized orchestration)

### Restore & Test

```bash
# restore dependencies for every dotnet service
for project in services/*/src/*.csproj; do dotnet restore "$project"; done

# run unit tests for all .NET services
for testProject in services/*/tests/**/*.csproj; do dotnet test "$testProject"; done
```

### Run Locally with Docker Compose

```bash
docker compose up --build
```

This command launches all backend services, the API gateway on `http://localhost:8080`, and the frontend on `http://localhost:3000`.

### Manual Local Execution

Each service can be started individually from its `src` folder:

```bash
cd services/user-service
dotnet run --project src/UserService.csproj --urls http://localhost:5001
```

Repeat for the remaining services (ports 5001-5006 and gateway 8080). Start the frontend with `npm install && npm start` inside `services/frontend`.

## 🧪 Testing

xUnit projects under each service provide seed tests for the in-memory stores. Extend them as you add business logic or persistence layers.

## 🗂️ CI/CD

Add workflow definitions under `.github/workflows/` to automate restore, build, and test steps per service. Each service's Dockerfile supports multi-stage builds for efficient containerization.

## 📄 License

MIT — see `LICENSE.txt`.
