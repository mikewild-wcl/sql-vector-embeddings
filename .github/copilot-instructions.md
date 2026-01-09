# GitHub Copilot Custom Instructions for sql-vector-embeddings

## Project Context
- .NET 10, C#
- Azure SQL 2025 vector capabilities
- AI document ingestion, embedding, and search
- Worker Service and Azure Function App patterns
- Strict adherence to .editorconfig and CONTRIBUTING.md

## Coding Guidelines
- Always follow .editorconfig and CONTRIBUTING.md for formatting, naming, and style.
- Use `BackgroundService` for long-running background tasks in Worker Service projects.
- Prefer async/await for I/O and database operations.
- Use dependency injection for services and configuration.
- Use clear, descriptive names for classes, methods, and variables.
- Write code that is testable and maintainable.
- Use C# 13 features where appropriate.
- Use Primary Constructors. **DO NOT** replace primary constructors when refactoring or adding code.

## Project-Specific Practices
- For database access, use SQL Server 2025 features and vector search patterns.
- For AI/embedding, integrate with external models as described in the README.
- For Azure Functions, follow the function signature and binding conventions.
- For configuration, use appsettings and secrets as described in the README.

## Documentation
- Add XML comments to public APIs.
- Update README.md and CONTRIBUTING.md when introducing new patterns or requirements.

## Example Prompts
- "Suggest a C# class for a background ingestion worker using BackgroundService."
- "Generate a SQL script for creating a vector index in SQL Server 2025."
- "Show how to call an Azure Function with an HTTP trigger and JSON body."