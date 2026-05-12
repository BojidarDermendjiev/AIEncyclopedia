#!/bin/bash
set -euo pipefail

echo "=== AI Encyclopedia Deploy ==="
echo "Branch: $(git rev-parse --abbrev-ref HEAD)"
echo "Commit: $(git rev-parse --short HEAD)"
echo ""

# Pull latest code
git pull origin main

# Build images
docker compose build --no-cache --parallel

# Start services (rolling restart)
docker compose up -d --remove-orphans

# Run database migrations
echo "Running database migrations..."
docker compose exec -T api dotnet ef database update --project src/AiEncyclopedia.Infrastructure --startup-project src/AiEncyclopedia.API

# Health check
echo "Waiting for API health check..."
for i in {1..10}; do
  if docker compose exec -T api curl -sf http://localhost:8080/health > /dev/null; then
    echo "API is healthy."
    break
  fi
  echo "Attempt $i/10 — waiting 5s..."
  sleep 5
done

echo ""
echo "=== Deploy complete ==="
docker compose ps
