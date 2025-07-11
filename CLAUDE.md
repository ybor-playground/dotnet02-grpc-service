# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Essential Commands

### Build and Test
```bash
# Build the entire solution
dotnet build

# Clean build outputs
dotnet clean

# Restore dependencies
dotnet restore

# Run all tests (unit + integration)
dotnet test

# Run unit tests only
dotnet test Dotnet02GrpcService.UnitTests

# Run integration tests only (uses ephemeral PostgreSQL containers)
dotnet test Dotnet02GrpcService.IntegrationTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with detailed output
dotnet test --verbosity normal
```

### Running the Service

#### Ephemeral Mode (Recommended for Development)
```bash
# Start the server with automatic PostgreSQL container (recommended)
dotnet run --project Dotnet02GrpcService.Server -- --ephemeral

# The service will:
# - Automatically start a PostgreSQL 15 container on a random port
# - Apply database migrations
# - Start the gRPC service on ports 5030 (gRPC) and 5031 (HTTP)
# - Clean up the container when stopped
```

#### Troubleshooting Port Conflicts
If you get a "address already in use" error, check for existing server instances:
```bash
# Check what's using the ports
lsof -i :5030
lsof -i :5031

# Stop existing server process (replace PID with actual process ID)
kill <PID>

# Force kill if needed
kill -9 <PID>
```

#### Standard Mode
```bash
# Start the server locally (requires external database)
dotnet run --project Dotnet02GrpcService.Server

# Start local database dependencies
docker-compose up -d

# Stop local database
docker-compose down
```

#### Ephemeral Mode (Development)
```bash
# Run with ephemeral database and debug logging
dotnet run --project Dotnet02GrpcService.Server -- --ephemeral
```


#### Environment Variables
```bash
# Custom port configuration
GRPC_PORT=6000 HTTP_PORT=6001 dotnet run --project Dotnet02GrpcService.Server

# Debug logging
LOG_LEVEL=Debug CORE_LOG_LEVEL=Debug dotnet run --project Dotnet02GrpcService.Server

# Custom database connection
DATABASE_URL="Host=myhost;Port=5432;Database=mydb;Username=user;Password=pass" dotnet run --project Dotnet02GrpcService.Server
```

### Database Operations
```bash
# Create new migration
dotnet ef migrations add <MigrationName> --project Dotnet02GrpcService.Persistence -s Dotnet02GrpcService.Server

# Apply migrations
dotnet ef database update --project Dotnet02GrpcService.Persistence -s Dotnet02GrpcService.Server

# Remove last migration
dotnet ef migrations remove --project Dotnet02GrpcService.Persistence -s Dotnet02GrpcService.Server

# Disable migrations (useful for ephemeral environments)
DATABASE_MIGRATIONS_ENABLED=false dotnet run --project Dotnet02GrpcService.Server
```

### Testing the gRPC Service

#### Authentication
```bash
# Get authentication token
curl -X POST http://localhost:5031/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"clientId": "admin-client", "clientSecret": "admin-secret"}'

# Extract token for use in gRPC calls
TOKEN="your-jwt-token-here"
```

#### gRPC Endpoints (with authentication)
```bash
# Test CreateDotnet02Grpc endpoint (requires admin or write role)
grpcurl -plaintext \
  -H "authorization: Bearer $TOKEN" \
  -d '{"name": "test"}' \
  localhost:5030 ybor.playground.dotnet02_grpc.service.grpc.Dotnet02GrpcService/CreateDotnet02Grpc

# Test GetDotnet02Grpcs endpoint (requires any role)
grpcurl -plaintext \
  -H "authorization: Bearer $TOKEN" \
  -d '{"start_page": "1", "page_size": "5"}' \
  localhost:5030 ybor.playground.dotnet02_grpc.service.grpc.Dotnet02GrpcService/GetDotnet02Grpcs
```

#### Health and Monitoring
```bash
# Comprehensive health check (includes database and service dependencies)
curl localhost:5031/health

# Kubernetes liveness probe (basic application health)
curl localhost:5031/health/live

# Kubernetes readiness probe (dependencies health)
curl localhost:5031/health/ready

# Prometheus metrics endpoint (custom business metrics + OpenTelemetry)
curl localhost:5031/metrics

# Custom metrics available:
# - dotnet02_grpc_requests_total (gRPC request count)
# - dotnet02_grpc_request_duration_seconds (request latency)
# - dotnet02_grpc_errors_total (error count by type)
# - dotnet02_grpc_entities_created_total (business metric)
# - dotnet02_grpc_entities_updated_total (business metric)
# - dotnet02_grpc_entities_deleted_total (business metric)
# - dotnet02_grpc_active_connections (current connections)
# - dotnet02_grpc_database_operation_duration_seconds (DB latency)
# - dotnet02_grpc_validation_errors_total (validation failures)
# - dotnet02_grpc_authorization_failures_total (auth failures)
```

### Container Operations
```bash
# Build container image
docker build -t dotnet02-grpc-service .

# Run container (ephemeral mode)
docker run -p 5030:5030 -p 5031:5031 -e SPRING_PROFILES_ACTIVE=ephemeral dotnet02-grpc-service

# Run container with custom configuration
docker run -p 5030:5030 -p 5031:5031 \
  -e DATABASE_URL="Host=host.docker.internal;Port=26257;Database=mydb;Username=root;Password=" \
  -e LOG_LEVEL=Debug \
  dotnet02-grpc-service
```

## Architecture Overview

This is a .NET 8.0 gRPC service following clean architecture with 7 main projects:

- **Dotnet02GrpcService.API**: gRPC service contracts and Protocol Buffers definitions
- **Dotnet02GrpcService.Client**: gRPC client implementation 
- **Dotnet02GrpcService.Core**: Business logic layer with service implementations
- **Dotnet02GrpcService.Persistence**: Data access layer using Entity Framework Core
- **Dotnet02GrpcService.Server**: gRPC server host and application entry point
- **Dotnet02GrpcService.IntegrationTests**: End-to-end integration tests
- **Dotnet02GrpcService.UnitTests**: Unit tests for Core and Persistence layers

### Key Technologies
- .NET 8.0 with nullable reference types enabled
- gRPC with Protocol Buffers for service contracts
- Entity Framework Core with PostgreSQL
- xUnit, Moq, FluentAssertions for comprehensive testing
- Testcontainers for integration tests
- Serilog for structured logging with correlation IDs
- OpenTelemetry for observability and distributed tracing
- CorrelationId for request tracing
- JWT-based authentication and role-based authorization

### Service Endpoints
- gRPC service: port 5030 (HTTP/2)
- Health/metrics: port 5031 (HTTP/1.1)
- Local database: port 5432

### Health Check Endpoints
- `/health` - Comprehensive health status with detailed information
- `/health/live` - Kubernetes liveness probe (basic application health)
- `/health/ready` - Kubernetes readiness probe (dependencies health)
- `/metrics` - Prometheus metrics endpoint

### Project Dependencies
The projects follow a dependency flow: Server → Core → Persistence, with API containing shared contracts and Client for external consumption. Tests reference appropriate layers for unit/integration testing.

## Observability and Monitoring

### OpenTelemetry Configuration
The service includes comprehensive OpenTelemetry support for metrics, tracing, and logging:

#### Environment Variables for Telemetry
```bash
# OpenTelemetry OTLP endpoint (e.g., Jaeger, Grafana)
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317

# Trace sampling (0.0 = no traces, 1.0 = all traces)
OTEL_TRACES_SAMPLER_ARG=1.0

# Service identification
APPLICATION_NAME=dotnet02-grpc-service
APPLICATION_VERSION=1.0.0
```

#### Built-in Instrumentation
- **ASP.NET Core**: HTTP request/response tracing
- **Entity Framework Core**: Database query tracing with SQL statements
- **HTTP Client**: Outbound HTTP call tracing
- **gRPC Core**: gRPC call tracing
- **Custom Business Metrics**: Entity operations, validation errors, auth failures

#### Structured Logging
Enhanced Serilog configuration with:
- Correlation ID enrichment for request tracing
- Machine name, thread ID, and environment enrichment
- Compact JSON formatting for log aggregation
- Configurable log levels per namespace
- Integration with OpenTelemetry for unified observability

### Health Checks
Comprehensive health checking with dependency validation:

#### Database Health Check
- Tests database connectivity and responsiveness
- Validates Entity Framework context and connection pooling
- Reports query performance and record counts

#### Service Health Check  
- Validates core service functionality
- Tests validation service operation
- Ensures all dependencies are properly configured

### Custom Metrics
Business-specific metrics exposed via Prometheus:

#### Request Metrics
- Request count by method and status
- Request duration histograms
- Error counts by error type
- Active connection tracking

#### Business Metrics
- Entity lifecycle operations (create, update, delete)
- Database operation performance
- Validation error tracking
- Authorization failure monitoring


### Configuration Management

#### Environment Variables (12-Factor App Compliant)
- `DATABASE_URL` - Complete database connection string
- `DATABASE_HOST`, `DATABASE_PORT`, `DATABASE_NAME`, `DATABASE_USERNAME`, `DATABASE_PASSWORD` - Individual database components
- `GRPC_PORT`, `HTTP_PORT` - Service port configuration
- `LOG_LEVEL`, `CORE_LOG_LEVEL`, `PERSISTENCE_LOG_LEVEL`, `GRPC_LOG_LEVEL` - Namespace-specific logging levels
- `APPLICATION_NAME`, `APPLICATION_VERSION` - Application metadata
- `DATABASE_MIGRATIONS_ENABLED` - Control migration execution
- `SPRING_PROFILES_ACTIVE` - Profile-based configuration (set to "ephemeral" for development)

#### Profiles
- **Production** (default): Uses external database, minimal logging
- **Ephemeral**: Automatic TestContainers database, debug logging, auto-migrations

### Database Configuration
- Uses Entity Framework migrations for schema management
- Supports both persistent PostgreSQL (via docker-compose) and ephemeral TestContainers databases
- Configurable migration execution via environment variables
- Connection pooling and timeout configuration

### Logging Strategy
- Structured JSON logging using Serilog
- Correlation ID tracking across requests
- Environment-specific log levels
- Machine name, thread ID, and environment enrichment
- Namespace-level log level control for debugging

### Testing Strategy
- **Unit Tests**: Core business logic and repository layer testing
- **Integration Tests**: End-to-end testing with TestContainers
- **Test Builders**: AutoBogus-powered test data generation
- **Mocking**: Moq for dependency isolation
- **Assertions**: FluentAssertions for readable test assertions

### Security
- Non-root container execution
- Minimal Alpine-based runtime image
- Health check monitoring
- Structured logging without sensitive data exposure

### Observability
- OpenTelemetry distributed tracing
- Prometheus metrics export
- Health checks with dependency monitoring
- Correlation ID request tracking
- Performance and timing instrumentation