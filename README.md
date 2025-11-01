# 🍽️ Feane Restaurant - Microservices Platform

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
![Microservices](https://img.shields.io/badge/Architecture-Microservices-green?style=flat-square)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=flat-square&logo=docker)

**Modern restaurant management system built with microservices**

[🚀 Quick Start](#-quick-start) • [🏗️ Architecture](#️-architecture) • [📚 Docs](#-documentation)

</div>

---

## 📋 Overview

Full-stack microservices e-commerce platform for restaurant management:

- 🛒 **E-Commerce** - products, cart, orders
- 📅 **Reservations** - table booking
- 👥 **Users** - authentication, profiles
- 💳 **Payments** - payment processing
- 📊 **Analytics** - metrics & reports
- 🔐 **JWT Auth** - access + refresh tokens
- 🌐 **API Gateway** - Ocelot

---

## 🚀 Quick Start

```bash
# Start all services
docker-compose up -d

# Access
# Frontend: http://localhost:5003
# Gateway: http://localhost:5000
```

**Requirements**: .NET 10.0, Docker Desktop

---

## 🏗️ Architecture

### Services

| Service | Port | Description |
|---------|------|-------------|
| Frontend | 5003 | Web UI |
| Gateway | 5000 | API Gateway + Auth |
| User | 5020 | User management |
| Product | 5030 | Catalog |
| Book | 5040 | Menu books |
| Reservation | 5050 | Bookings |
| Cart | 5060 | Shopping cart |
| Order | 5070 | Orders |
| Analytics | 5080 | Analytics |

### Structure

```
services/
├── gateway/
│   ├── src/
│   └── FeaneGateway.Tests/
├── user-service/
├── product-service/
├── cart-service/
├── OrderService/
├── reservation-service/
├── book-service/
├── AnalyticsService/
└── frontend/
```

---

## 🔐 Authentication

**JWT Tokens**:
- Access: 60 min
- Refresh: 7 days

```http
POST /api/auth/login
POST /api/auth/register
POST /api/auth/refresh
```

---

## 🧪 Testing

```bash
# All tests
.\scripts\run-all-unit-tests.ps1

# Refresh token test
.\scripts\test-refresh-token.ps1
```

**Stats**: 60+ tests, 98% pass

---

## 📚 Documentation

- [ARCHITECTURE_CLEAN.md](docs/ARCHITECTURE_CLEAN.md) - Clean architecture overview ⭐
- [PROJECT_OVERVIEW.md](docs/PROJECT_OVERVIEW.md) - Complete overview
- [PROJECT_STRUCTURE.md](docs/PROJECT_STRUCTURE.md) - Architecture details
- [REFRESH_TOKEN_GUIDE.md](docs/REFRESH_TOKEN_GUIDE.md) - JWT guide
- [CLEANUP_REPORT.md](docs/CLEANUP_REPORT.md) - Cleanup report

---

## 🛠️ Stack

.NET 10.0 • ASP.NET Core • EF Core • Ocelot • SQL Server • Docker • xUnit

---

## 📝 License

MIT

---

<div align="center">

**Made with ❤️ for Feane Restaurant**

</div>

