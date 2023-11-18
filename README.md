# Nummy Http Request & Response Logging package for .NET Core

[![NuGet Version](https://img.shields.io/nuget/v/Nummy.HttpLogger.svg)](https://www.nuget.org/packages/Nummy.HttpLogger/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Overview

This is a .NET Core library for http request and response logging in your application.

## Installation

https://www.nuget.org/packages/Nummy.HttpLogger
Or install the package via NuGet Package Manager Console:

```bash
Install-Package Nummy.HttpLogger
```

## Getting Started

In your `Program.cs` file add the following line:

```csharp
using Nummy.HttpLogger.Extensions;
```

```csharp
// .. other configurations

services.AddNummyHttpLogger(options =>
{
    // Configure options here
    // Example: 
    options.EnableRequestLogging = true;
    options.EnableResponseLoggging = true;
    options.ExcludeContainingPaths = new []{"api/user/login", "statistics", "user/create" };
    options.DatabaseType = NummyHttpLoggerDatabaseType.PostgreSql;
    options.DatabaseConnectionString = "your-database-connection-string"
});

// .. other configurations
var app = builder.Build();
```

Use Middleware: In the Configure method of your Startup.cs file, add the following line:

```csharp
using Nummy.HttpLogger.Extensions;
```

```csharp
var app = builder.Build();

// .. other configurations

app.UseNummyHttpLogger();

// .. other middleware
```

Now, your application is set up to log http request and responses using the Nummy Http Logger.

## License

This library is licensed under the MIT License.