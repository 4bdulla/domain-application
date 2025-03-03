# Domain Application

A lightweight framework for rapidly bootstrapping ASP.NET Core applications with standardized configurations and features.

## Features

- Simplified application setup with sensible defaults
- Built-in database integration with Entity Framework Core
- Integrated authorization and authentication support
- Swagger/OpenAPI configuration
- Transaction behavior support
- JSON serialization handling
- API endpoint configuration

## Installation

```shell
dotnet add package Domain.App.Core
```

## Usage

### Basic Application Setup

```csharp
await DomainApplication.RunAsync(args);
```

### With Database Integration

```csharp
await DomainApplication.RunWithDbAsync<YourDbContext>(args);
```

### Custom Configuration

```csharp
await DomainApplication.RunAsync(
    args,
    builder => {
        // Configure services
        builder.Services.AddScoped<IYourService, YourService>();
    },
    app => {
        // Configure middleware
        app.UseCustomMiddleware();
    });
```

## Configuration Options

- `configureServices`: Custom service configuration
- `configureApplication`: Custom middleware configuration
- `serializerOptions`: Custom JSON serialization options
- `addTransactionBehavior`: Enable/disable transaction behavior (database only)

## License

This project is licensed under the [Apache 2.0 License](LICENSE).

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.~~~~