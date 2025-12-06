# FoodMatch - Quick Start Guide

## Uruchamianie aplikacji

### Metoda 1: Przez skrypt (Najprostsza)

```bash
# Development (z narzędziami developerskimi)
./run.sh

# Production
./run.sh prod
```

### Metoda 2: Przez Rider/Visual Studio

1. Otwórz `FoodMatch.sln`
2. Ustaw `FoodMatch.AppHost` jako startup project
3. Wybierz profil `https`
4. Naciśnij F5 lub kliknij Run

### Metoda 3: Z terminala

```bash
# Development
cd FoodMatch.AppHost
dotnet run

# Production
cd FoodMatch.AppHost
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_ENVIRONMENT=Production
dotnet run
```

## Adresy aplikacji

Po uruchomieniu aplikacja będzie dostępna pod następującymi adresami:

### Development
- **Frontend**: http://localhost:5173
- **Backend HTTPS**: https://localhost:7257
- **Backend HTTP**: http://localhost:5127
- **Swagger**: https://localhost:7257/swagger
- **PostgreSQL**: localhost:5433
- **Redis**: localhost:6379
- **Aspire Dashboard**: https://localhost:17888 (token w konsoli)

### Narzędzia developerskie (tylko Development)
- **PgAdmin**: Dostępny przez Aspire Dashboard
- **Redis Commander**: Dostępny przez Aspire Dashboard

## Ważne informacje

### HTTPS
- **Backend automatycznie używa profilu "https" z launchSettings.json**
- Główny endpoint: https://localhost:7257
- Fallback HTTP endpoint: http://localhost:5127
- Frontend automatycznie łączy się z backendem przez HTTPS
- Aspire automatycznie zarządza endpointami na podstawie launchSettings.json

### CORS
Backend akceptuje wszystkie żądania z localhost (http i https), więc nie powinno być problemów z CORS.

### Baza danych
Connection string jest automatycznie dostarczany przez Aspire Service Discovery. Nie musisz go ręcznie konfigurować.

### Certyfikaty rozwojowe
Jeśli masz problemy z certyfikatami HTTPS w development:

```bash
dotnet dev-certs https --trust
```

## Troubleshooting

### "Port 5173 jest zajęty"
```bash
lsof -ti:5173 | xargs kill -9
```

### "Port 7257 jest zajęty"
```bash
lsof -ti:7257 | xargs kill -9
```

### Restart wszystkich kontenerów
```bash
cd FoodMatch.AppHost
dotnet run
# Naciśnij Ctrl+C
# Uruchom ponownie
```

### Problemy z bazą danych
Sprawdź czy kontener PostgreSQL działa:
```bash
docker ps | grep postgres
```

## Przydatne komendy

```bash
# Sprawdź status wszystkich kontenerów
docker ps

# Wyświetl logi PostgreSQL
docker logs <postgres-container-id>

# Wyświetl logi Redis
docker logs <redis-container-id>

# Zatrzymaj wszystkie kontenery
docker stop $(docker ps -q)

# Wyczyść projekt
dotnet clean
```

## Więcej informacji

Zobacz `FoodMatch.AppHost/README_DEPLOYMENT.md` dla szczegółowej dokumentacji o różnych środowiskach i deployment.

