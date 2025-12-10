#!/bin/bash

# Test script for Docker containerization
set -e

echo "🐳 Testing Docker containerization..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

echo "✅ Docker is running"

# Check if docker-compose is available
if ! command -v docker-compose > /dev/null 2>&1; then
    echo "❌ docker-compose is not installed. Please install docker-compose and try again."
    exit 1
fi

echo "✅ docker-compose is available"

# Validate docker-compose configuration
echo "🔍 Validating docker-compose configuration..."
if docker-compose config > /dev/null 2>&1; then
    echo "✅ docker-compose.yml is valid"
else
    echo "❌ docker-compose.yml has configuration errors"
    exit 1
fi

# Build images (without starting services)
echo "🏗️  Building Docker images..."
docker-compose build --no-cache

if [ $? -eq 0 ]; then
    echo "✅ All Docker images built successfully"
else
    echo "❌ Failed to build Docker images"
    exit 1
fi

# Test health check endpoints (if services are running)
echo "🏥 Testing health check endpoints..."

# Check if services are running
if docker-compose ps | grep -q "Up"; then
    echo "Services are running, testing health checks..."
    
    # Test backend health
    if curl -f http://localhost:5000/api/health > /dev/null 2>&1; then
        echo "✅ Backend health check passed"
    else
        echo "⚠️  Backend health check failed (service may not be ready)"
    fi
    
    # Test predictor health
    if curl -f http://localhost:8000/health > /dev/null 2>&1; then
        echo "✅ Predictor health check passed"
    else
        echo "⚠️  Predictor health check failed (service may not be ready)"
    fi
    
    # Test frontend health
    if curl -f http://localhost:3000/health > /dev/null 2>&1; then
        echo "✅ Frontend health check passed"
    else
        echo "⚠️  Frontend health check failed (service may not be ready)"
    fi
else
    echo "ℹ️  Services are not running. Use 'docker-compose up' to start them."
fi

echo "🎉 Docker containerization test completed!"
echo ""
echo "To start all services:"
echo "  docker-compose up -d"
echo ""
echo "To start in development mode:"
echo "  docker-compose -f docker-compose.yml -f docker-compose.dev.yml up -d"
echo ""
echo "To stop all services:"
echo "  docker-compose down"