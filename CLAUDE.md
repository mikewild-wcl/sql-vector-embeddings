# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **SQL Server 2025 vector embeddings system** built with .NET Aspire that ingests PDF documents, generates embeddings using Ollama, and stores them in SQL Server for vector-based semantic search. The architecture uses distributed microservices orchestrated by .NET Aspire.

## Build and Run Commands

### Running the Application

```bash
# Run the entire Aspire application (starts all services)
dotnet run --project src/Sql.Vector.Embeddings.AppHost

# This will start:
# - Ollama container with phi4 (chat) and all-minilm (embeddings) models
# - SQL Server 2025 container with "Documents" database
# - DatabaseDeploymentService (runs migrations, then exits)
# - Ingestion.Functions (Azure Function HTTP endpoint)
# - ServiceDefaults (shared infrastructure for all services)
# - Query Console (explicit start - use Aspire dashboard to start manually)
```

### Development Commands

```bash
# Build all projects
dotnet build

# Build specific project
dotnet build src/Sql.Vector.Embeddings.Ingestion.Functions

# Restore dependencies
dotnet restore

# Clean build artifacts
dotnet clean
```

### Calling the Ingestion Function

The function expects a POST with JSON payload containing URIs to PDF files:

```bash
curl -X POST http://localhost:7034/api/ingest-uris/ \
  -H "Content-Type: application/json" \
  -d '{"uris": ["https://arxiv.org/pdf/1409.0473"]}'
```

Alternatively, use the test HTTP script in `src/Sql.Vector.Embeddings.Ingestion.Functions/`.

### Database Connection

To connect to the SQL Server database with SSMS or Azure Data Studio:
1. Open the Aspire dashboard (shown when you run the AppHost)
2. Navigate to the SQL Server resource
3. Copy the connection string
4. Paste into SSMS/Azure Data Studio connection dialog

## Architecture Overview

### Service Orchestration Pattern

The AppHost (`src/Sql.Vector.Embeddings.AppHost/AppHost.cs`) orchestrates the startup sequence:

1. **Infrastructure Layer**: Ollama + SQL Server containers start
2. **Database Migration**: `DatabaseDeploymentService` runs DbUp migrations and waits for completion
3. **Ingestion Service**: `Ingestion.Functions` starts after migrations complete
4. **Query Interface**: `QueryConsole` requires explicit manual start from dashboard

### Key Components

**AppHost** (`Sql.Vector.Embeddings.AppHost`)
- Central orchestrator defining service topology
- Manages container lifetimes (Persistent for Ollama/SQL)
- Configures Dev Tunnels for remote Ollama access
- Reads configuration from `appsettings.json` (model names, embedding dimensions, GPU usage)

**ServiceDefaults** (`Sql.Vector.Embeddings.ServiceDefaults`)
- Shared infrastructure referenced by all services
- Configures OpenTelemetry (tracing/metrics)
- Sets up health check endpoints (`/health`, `/alive`)
- Enables HTTP client resilience patterns
- Provides service discovery configuration

**Ingestion.Functions** (`Sql.Vector.Embeddings.Ingestion.Functions`)
- Azure Functions v4 HTTP-triggered function
- Entry point: `IngestFromUriFunction.cs` - POST `/api/ingest-uris`
- Request model: `Models/UriListRequest.cs` (array of URI strings)
- **Status**: Framework in place, embedding generation logic incomplete

**DatabaseDeploymentService** (`Sql.Vector.Embeddings.DatabaseDeploymentService`)
- Runs at startup using DbUp v5.0.41 for schema migrations
- Waits for SQL Server container readiness
- Reads embedding dimensions from configuration
- Exits after migrations complete (WaitForCompletion pattern)

**QueryConsole** (`Sql.Vector.Embeddings.QueryConsole`)
- Interactive console using Microsoft.Agents.AI (preview)
- Streaming chat interface with multi-turn conversations
- References both chat model (phi4) and embedding model (all-minilm)
- Custom command: "Run query console" with interactive argument input
- Explicit start only (not auto-started with AppHost)

**BlobUploadConsole** (`Sql.Vector.Embeddings.BlobUploadConsole`)
- Handles Azure Blob Storage uploads for document staging
- Currently disabled in AppHost orchestration
- Scans for ZIP files and uploads with metadata

**MigrationService** (`Sql.Vector.Embeddings.MigrationService`)
- Alternative BackgroundService-based migration approach
- Currently disabled (DatabaseDeploymentService is active instead)

### Data Flow

```
PDF URIs → Ingestion Function → Ollama (embeddings) → SQL Server 2025 → Query Console
```

1. Client POSTs URIs to `/api/ingest-uris`
2. Function validates URIs via `UriListRequest`
3. Ollama generates 384-dimensional embeddings (all-minilm model)
4. Embeddings stored in SQL Server vector columns
5. QueryConsole enables semantic search + chat interaction

### Configuration

**AppHost Parameters** (`src/Sql.Vector.Embeddings.AppHost/appsettings.json`):

```json
{
  "Parameters": {
    "model": "phi4",                 // Chat model name
    "embeddingModel": "all-minilm",  // Embedding model name
    "embeddingDimensions": 384,      // Vector size
    "useGPU": true                   // Enable GPU acceleration
  }
}
```

**Service Discovery**: Services reference resources by name (e.g., `"sql"`, `"ollama"`) and Aspire automatically injects connection strings via environment variables following pattern `services__{resource}__{protocol}__{index}`.

### Key Technologies

- **.NET 10** (SDK 10.0.101+)
- **.NET Aspire v13.0.2+** - Orchestration framework
- **Ollama** - Local LLM hosting (via CommunityToolkit.Aspire.Hosting.Ollama)
- **SQL Server 2025** - Vector database capabilities
- **Azure Functions v4** - Serverless ingestion endpoint
- **DbUp v5.0.41** - Database migration framework
- **Microsoft.Agents.AI v1.0.0-preview** - Agent framework for QueryConsole
- **OpenTelemetry** - Distributed tracing and observability

### Dev Tunnels

The AppHost configures Dev Tunnels to expose the local Ollama instance:
- Resource name: `"ollama-api"`
- Anonymous access enabled
- Lifecycle hooks: `OnResourceEndpointsAllocated`, `OnResourceReady`
- Used for remote embedding generation (work in progress)

## Code Quality Settings

Configured via `Directory.Build.props`:
- **Analysis Level**: Latest (all rules enabled)
- **Analysis Mode**: All
- **Code Analysis Warnings**: Treated as errors
- **Build Warnings**: Not treated as errors (development-friendly)
- **EnforceCodeStyleInBuild**: Enabled
- **SonarAnalyzer.CSharp**: v10.17.0+ enforced on all .csproj files

## Project State

Based on recent git history:
- Initial migration and deployment projects added (b22f168)
- Function app and SQL database partially configured (abd05d2)
- Ollama client integration working (9c476ea)
- **Current blockers**:
  - Embedding generation logic in IngestFromUriFunction not implemented
  - Database schema not fully deployed
  - QueryConsole interactive commands incomplete

## Important Patterns

### Service References
All services use Aspire's service discovery pattern:
```csharp
builder.AddReference(sql)  // Injects connection string automatically
```

### Wait Patterns
Critical for startup ordering:
```csharp
.WaitFor(sql)              // Wait for resource to be ready
.WaitForCompletion(deploy) // Wait for project to exit successfully
```

### Container Persistence
```csharp
.WithLifetime(ContainerLifetime.Persistent)  // Survives restarts
.WithDataVolume()                            // Persistent storage
```

### Health Checks
- Development mode: Health endpoints enabled
- Production: Secure endpoints (configured in ServiceDefaults)
- Liveness: Self-check only (`/alive`)
- Readiness: All dependency checks (`/health`)

## Common Gotchas

1. **No Solution File**: This repo uses `.slnx` format (XML-based solution). Standard `.sln` not present.
2. **Explicit Start Services**: QueryConsole won't auto-start - must use dashboard command
3. **Migration Ordering**: Ingestion Functions depends on DatabaseDeploymentService completion
4. **GPU Configuration**: Set `useGPU: false` in appsettings.json if no CUDA available
5. **Dev Tunnels**: Requires authentication on first run for remote access
6. **Container First Run**: Ollama will pull models on first start (multi-GB download)

## Testing

### Test Projects

The solution includes two unit test projects:
- **`Sql.Vector.Embeddings.Data.UnitTests`** - Tests for data access layer
- **`Sql.Vector.Embeddings.Ingestion.Core.UnitTests`** - Tests for ingestion business logic

### Testing Stack

Nuget packages are minimum versions. The latest version can be used if available.

**Test Framework**: xUnit v3.2.1

**Assertions**: **Shouldly v4.3.0** (REQUIRED)
- Provides fluent, readable assertions
- Example: `result.ShouldBe(expected);` instead of `Assert.Equal(expected, result);`

**Mocking/Test Doubles**: **NSubstitute v5.3.0** (REQUIRED)
- Simple, concise mocking syntax
- Example: `var mock = Substitute.For<IService>();`

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/Sql.Vector.Embeddings.Data.UnitTests

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run single test by name filter
dotnet test --filter "FullyQualifiedName~MyTestName"
```

### Test Conventions

- Follow naming: `Sql.Vector.Embeddings.{Component}.UnitTests`
- Use **Shouldly** for all assertions (not xUnit Assert)
- Use **NSubstitute** for all mocking (not Moq or other frameworks)
- Reference ServiceDefaults for integration tests
- Use Aspire.Hosting.Testing for container-based tests
- Test method naming: `MethodName_Scenario_ExpectedBehavior`
