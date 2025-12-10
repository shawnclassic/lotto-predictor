-- Database initialization script for PredictLottoNZ
-- This script ensures the database is properly set up for the application

-- Create the database if it doesn't exist (this is handled by POSTGRES_DB env var)
-- The database will be created automatically by the PostgreSQL Docker container

-- Set up any initial database configuration
-- Enable UUID extension if needed for future use
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create a schema for the application (optional, using public schema by default)
-- CREATE SCHEMA IF NOT EXISTS predict_lotto;

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON DATABASE predict_lotto_nz TO postgres;

-- Log successful initialization
DO $$
BEGIN
    RAISE NOTICE 'PredictLottoNZ database initialized successfully';
END $$;