# Test script for Docker containerization
param(
    [switch]$Build = $false,
    [switch]$HealthCheck = $false
)

Write-Host "🐳 Testing Docker containerization..." -ForegroundColor Cyan

# Check if Docker is running
try {
    docker info | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not running. Please start Docker and try again." -ForegroundColor Red
    exit 1
}

# Check if docker-compose is available
try {
    docker-compose --version | Out-Null
    Write-Host "✅ docker-compose is available" -ForegroundColor Green
} catch {
    Write-Host "❌ docker-compose is not installed. Please install docker-compose and try again." -ForegroundColor Red
    exit 1
}

# Validate docker-compose configuration
Write-Host "🔍 Validating docker-compose configuration..." -ForegroundColor Yellow
try {
    docker-compose config | Out-Null
    Write-Host "✅ docker-compose.yml is valid" -ForegroundColor Green
} catch {
    Write-Host "❌ docker-compose.yml has configuration errors" -ForegroundColor Red
    exit 1
}

# Build images if requested
if ($Build) {
    Write-Host "🏗️  Building Docker images..." -ForegroundColor Yellow
    try {
        docker-compose build --no-cache
        Write-Host "✅ All Docker images built successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to build Docker images" -ForegroundColor Red
        exit 1
    }
}

# Test health check endpoints if requested
if ($HealthCheck) {
    Write-Host "🏥 Testing health check endpoints..." -ForegroundColor Yellow
    
    # Check if services are running
    $runningServices = docker-compose ps --services --filter status=running
    
    if ($runningServices) {
        Write-Host "Services are running, testing health checks..." -ForegroundColor Blue
        
        # Test backend health
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:5000/api/health" -TimeoutSec 5 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ Backend health check passed" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠️  Backend health check failed (service may not be ready)" -ForegroundColor Yellow
        }
        
        # Test predictor health
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:8000/health" -TimeoutSec 5 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ Predictor health check passed" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠️  Predictor health check failed (service may not be ready)" -ForegroundColor Yellow
        }
        
        # Test frontend health
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:3000/health" -TimeoutSec 5 -UseBasicParsing
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ Frontend health check passed" -ForegroundColor Green
            }
        } catch {
            Write-Host "⚠️  Frontend health check failed (service may not be ready)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "ℹ️  Services are not running. Use 'docker-compose up' to start them." -ForegroundColor Blue
    }
}

Write-Host "🎉 Docker containerization test completed!" -ForegroundColor Green
Write-Host ""
Write-Host "To start all services:" -ForegroundColor Cyan
Write-Host "  docker-compose up -d" -ForegroundColor White
Write-Host ""
Write-Host "To start in development mode:" -ForegroundColor Cyan
Write-Host "  docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d" -ForegroundColor White
Write-Host ""
Write-Host "To stop all services:" -ForegroundColor Cyan
Write-Host "  docker-compose down" -ForegroundColor White