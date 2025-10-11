@echo off
REM FoodMatch Database Management Script for Windows

if "%1"=="start" goto start
if "%1"=="up" goto start
if "%1"=="stop" goto stop
if "%1"=="down" goto stop
if "%1"=="restart" goto restart
if "%1"=="logs" goto logs
if "%1"=="status" goto status
if "%1"=="clean" goto clean
goto help

:start
echo ?? Starting FoodMatch database...
docker-compose up -d
echo ? Database started!
echo ?? pgAdmin: http://localhost:8080
echo ?? PostgreSQL: localhost:5432
goto end

:stop
echo ?? Stopping FoodMatch database...
docker-compose down
echo ? Database stopped!
goto end

:restart
echo ?? Restarting FoodMatch database...
docker-compose down
docker-compose up -d
echo ? Database restarted!
goto end

:logs
echo ?? Database logs:
docker-compose logs -f postgres
goto end

:status
echo ?? Database status:
docker-compose ps
goto end

:clean
echo ?? Cleaning up database (removes all data)...
set /p "confirm=Are you sure? This will delete all data! (y/N): "
if /i "%confirm%"=="y" (
    docker-compose down -v
    docker volume rm foodmatch_postgres_data foodmatch_pgladmin_data 2>nul
    echo ? Database cleaned!
) else (
    echo ? Operation cancelled
)
goto end

:help
echo ?? FoodMatch Database Management
echo.
echo Usage: %0 {start^|stop^|restart^|logs^|status^|clean}
echo.
echo Commands:
echo   start   - Start database containers
echo   stop    - Stop database containers  
echo   restart - Restart database containers
echo   logs    - Show database logs
echo   status  - Show container status
echo   clean   - Remove all data (use with caution!)
echo.

:end