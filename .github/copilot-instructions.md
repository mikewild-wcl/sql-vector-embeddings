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

## Testing Guidelines

### Required Testing Stack
- **Test Framework**: xUnit v3.2.1 or above
- **Assertions**: **Shouldly v4.3.0** or above - Use ONLY Shouldly for assertions, NOT xUnit Assert
- **Mocking**: **NSubstitute v5.3.0** or above - Use ONLY NSubstitute for test doubles, NOT Moq

### Test Project Structure
- Unit test projects: `Sql.Vector.Embeddings.{Component}.UnitTests`
- Integration test projects: `Sql.Vector.Embeddings.{Component}.IntegrationTests`

### Testing Best Practices
- **Always use Shouldly syntax**: `result.ShouldBe(expected);` instead of `Assert.Equal(expected, result);`
- **Always use NSubstitute**: `var mock = Substitute.For<IService>();` instead of Moq
- **Test naming**: `MethodName_Scenario_ExpectedBehavior` (e.g., `SaveDocument_WithNullInput_ShouldThrowArgumentNullException`)
- Write tests that are isolated, repeatable, and fast
- Mock external dependencies (databases, HTTP clients, file system)
- Use Aspire.Hosting.Testing for container-based integration tests
- Include both positive and negative test cases
- Test edge cases and boundary conditions

### Example Test Structure
```csharp
public class MyServiceTests
{
    [Fact]
    public void MethodName_Scenario_ExpectedBehavior()
    {
        // Arrange
        var dependency = Substitute.For<IDependency>();
        dependency.GetData().Returns("test");
        var sut = new MyService(dependency);

        // Act
        var result = sut.DoSomething();

        // Assert
        result.ShouldBe("expected");
        dependency.Received(1).GetData();
    }
}
```

## Documentation
- Add XML comments to public APIs.
- Update README.md and CONTRIBUTING.md when introducing new patterns or requirements.

## Example Prompts
- "Suggest a C# class for a background ingestion worker using BackgroundService."
- "Generate a SQL script for creating a vector index in SQL Server 2025."
- "Show how to call an Azure Function with an HTTP trigger and JSON body."
- "Write unit tests using Shouldly and NSubstitute for this service."