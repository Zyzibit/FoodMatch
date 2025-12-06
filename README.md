# FoodMatch

Aplikacja do zarządzania dietą i planowania posiłków.

## 🚀 Szybki start

```bash
# Uruchom aplikację (Development z HTTPS)
./run.sh

# Lub przez Rider/Visual Studio
# Otwórz FoodMatch.sln, ustaw FoodMatch.AppHost jako startup project i uruchom
```

Szczegółowe instrukcje: [QUICKSTART.md](./QUICKSTART.md)

## 📋 Technologie

### Backend
- .NET 8 / ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Redis
- JWT Authentication

### Frontend
- React + TypeScript
- Vite
- TailwindCSS

### Infrastruktura
- .NET Aspire (orkiestracja i observability)
- Docker (PostgreSQL, Redis)

## 🏗️ Architektura

Projekt używa **.NET Aspire** do zarządzania orkiestracją mikroserwisów:

```
FoodMatch.AppHost/          # Orkiestrator Aspire
├── Program.cs              # Konfiguracja środowisk
└── Properties/
    └── launchSettings.json # Profile uruchomieniowe

FoodMatch.ServiceDefaults/  # Współdzielone ustawienia
backend/                    # Backend API (.NET 8)
frontend/                   # Frontend (React + Vite)
```

## 🔧 Wymagania

- .NET 8 SDK
- Node.js 18+
- Docker Desktop
- (Opcjonalnie) Rider lub Visual Studio

## 📚 Dokumentacja

- [QUICKSTART.md](./QUICKSTART.md) - Szybki start
- [FoodMatch.AppHost/README_DEPLOYMENT.md](./FoodMatch.AppHost/README_DEPLOYMENT.md) - Deployment i środowiska
- [backend/README_ASPIRE.md](./backend/README_ASPIRE.md) - Integracja Aspire z backendem

## 🌐 Adresy (Development)

- Frontend: http://localhost:5173
- Backend (HTTPS): https://localhost:7257
- Backend (HTTP): http://localhost:5127
- Swagger: https://localhost:7257/swagger
- Aspire Dashboard: https://localhost:17888

## 🔐 HTTPS

Backend **automatycznie używa HTTPS** jako głównego endpointu (port 7257). Frontend automatycznie łączy się przez HTTPS.

Jeśli masz problemy z certyfikatami:
```bash
dotnet dev-certs https --trust
```

## 📦 Struktura bazy danych

- PostgreSQL: localhost:5433
- Username: postgres
- Password: postgres
- Database: foodmatch

Connection string jest automatycznie dostarczany przez Aspire Service Discovery.

## 🛠️ Development

```bash
# Uruchom w trybie development
./run.sh

# Uruchom w trybie production
./run.sh prod

# Wyczyść i przebuduj
./run.sh dev --clean
```

## 🐛 Troubleshooting

Zobacz sekcję Troubleshooting w [QUICKSTART.md](./QUICKSTART.md)

## 📄 Licencja

Projekt studencki

