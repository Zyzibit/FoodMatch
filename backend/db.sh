#!/bin/bash

# FoodMatch Database Management Script

case "$1" in
    "start"|"up")
        echo "?? Starting FoodMatch database..."
        docker-compose up -d
        echo "? Database started!"
        echo "?? pgAdmin: http://localhost:8080"
        echo "?? PostgreSQL: localhost:5432"
        ;;
    "stop"|"down")
        echo "?? Stopping FoodMatch database..."
        docker-compose down
        echo "? Database stopped!"
        ;;
    "restart")
        echo "?? Restarting FoodMatch database..."
        docker-compose down
        docker-compose up -d
        echo "? Database restarted!"
        ;;
    "logs")
        echo "?? Database logs:"
        docker-compose logs -f postgres
        ;;
    "status")
        echo "?? Database status:"
        docker-compose ps
        ;;
    "clean")
        echo "?? Cleaning up database (removes all data)..."
        read -p "Are you sure? This will delete all data! (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            docker-compose down -v
            docker volume rm foodmatch_postgres_data foodmatch_pgadmin_data 2>/dev/null || true
            echo "? Database cleaned!"
        else
            echo "? Operation cancelled"
        fi
        ;;
    *)
        echo "?? FoodMatch Database Management"
        echo ""
        echo "Usage: $0 {start|stop|restart|logs|status|clean}"
        echo ""
        echo "Commands:"
        echo "  start   - Start database containers"
        echo "  stop    - Stop database containers"
        echo "  restart - Restart database containers"
        echo "  logs    - Show database logs"
        echo "  status  - Show container status"
        echo "  clean   - Remove all data (use with caution!)"
        echo ""
        exit 1
        ;;
esac