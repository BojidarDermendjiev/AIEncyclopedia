#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Enable extensions
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    CREATE EXTENSION IF NOT EXISTS "pg_trgm";
    CREATE EXTENSION IF NOT EXISTS "unaccent";
    -- pgvector will be enabled in Phase 3 for semantic search
    -- CREATE EXTENSION IF NOT EXISTS "vector";

    -- Create n8n database
    CREATE DATABASE n8n OWNER $POSTGRES_USER;
EOSQL

echo "Database initialization complete."
