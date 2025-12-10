# PredictLottoNZ Development Makefile

.PHONY: help build up down logs clean test dev-setup

# Default target
help:
	@echo "Available commands:"
	@echo "  build      - Build all Docker images"
	@echo "  up         - Start all services"
	@echo "  down       - Stop all services"
	@echo "  logs       - View logs from all services"
	@echo "  clean      - Remove all containers and volumes"
	@echo "  test       - Run all tests"
	@echo "  dev-setup  - Set up development environment"
	@echo "  db-reset   - Reset database"
	@echo "  db-migrate - Run database migrations"

# Build all Docker images
build:
	docker-compose build

# Start all services
up:
	docker-compose up -d

# Stop all services
down:
	docker-compose down

# View logs from all services
logs:
	docker-compose logs -f

# Remove all containers and volumes
clean:
	docker-compose down -v --remove-orphans
	docker system prune -f

# Run all tests
test:
	@echo "Running backend tests..."
	cd backend && dotnet test
	@echo "Running frontend tests..."
	cd frontend && npm test
	@echo "Running predictor tests..."
	cd predictor && python -m pytest

# Set up development environment
dev-setup:
	@echo "Setting up development environment..."
	@if [ ! -f .env ]; then cp .env.example .env; echo "Created .env file from template"; fi
	@echo "Starting database..."
	docker-compose up -d postgres
	@echo "Waiting for database to be ready..."
	@sleep 10
	@echo "Development environment ready!"

# Reset database
db-reset:
	docker-compose down postgres
	docker volume rm predict-lotto-postgres-data || true
	docker-compose up -d postgres

# Run database migrations (when backend is implemented)
db-migrate:
	cd backend && dotnet ef database update

# Quick development start
dev: dev-setup
	@echo "Starting all services for development..."
	docker-compose up -d

# Production deployment
deploy:
	@echo "Deploying to production..."
	docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Health check all services
health:
	@echo "Checking service health..."
	@curl -f http://localhost:5000/health || echo "Backend: DOWN"
	@curl -f http://localhost:8000/health || echo "Predictor: DOWN"
	@curl -f http://localhost:3000 || echo "Frontend: DOWN"