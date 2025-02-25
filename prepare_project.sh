#!/bin/bash

# Exit on error
set -e

# Define paths
CERTS_DIR="infra/devcerts"
CERT_FILE="$CERTS_DIR/aspnetapp.pfx"
CERT_PASSWORD="aspnetapp"

# Create certificate directory if not exists
mkdir -p "$CERTS_DIR"

echo "🔹 Generating HTTPS certificate..."
dotnet dev-certs https -ep "$CERT_FILE" -p "$CERT_PASSWORD"

echo "🔹 Trusting HTTPS certificate (optional, required for local development)..."
dotnet dev-certs https --trust || echo "⚠️ Skipping trust step. You may need to manually trust the cert."

echo "🔹 Restoring dependencies..."
dotnet restore

echo "🔹 Building the project..."
dotnet build --configuration Release

echo "🔹 Running unit tests..."
dotnet test tests/ValidatorService.UnitTests --logger "trx;LogFileName=unit-test-results.trx"

echo "🔹 Running integration tests..."
dotnet test tests/ValidatorService.IntegrationTests --logger "trx;LogFileName=integration-test-results.trx"

echo "🔹 Starting the project..."
dotnet run --project src/ValidatorService --configuration Release

echo "✅ Project setup complete!"