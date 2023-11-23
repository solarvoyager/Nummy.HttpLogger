# Nummy Http Request & Response Logging package for .NET Core

[![NuGet Version](https://img.shields.io/nuget/v/Nummy.HttpLogger.svg)](https://www.nuget.org/packages/Nummy.HttpLogger/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Overview

This is a .NET Core library for http request and response logging in your application.
Just set connection string of your database then package will create and manage required tables for itself.

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
using Nummy.HttpLogger.Models;
```

```csharp
// .. other configurations

builder.Services.AddNummyHttpLogger(options =>
{
    // Configure options here
    // Example: 
    options.EnableRequestLogging = true;
    options.EnableResponseLogging = true;
    options.ExcludeContainingPaths = new []{ "swagger", "api/user/login", "user/create" };
    options.DatabaseType = NummyHttpLoggerDatabaseType.PostgreSql;
    options.DatabaseConnectionString = "Host=localhost;Port=5432;Database=nummy_db;Username=postgres;Password=postgres;IncludeErrorDetail=true;";
});

// .. other configurations
var app = builder.Build();
```

```csharp
var app = builder.Build();

// .. other configurations

app.UseNummyHttpLogger();

// .. other middleware
```

Now, your application is set up to log http request and responses using the Nummy Http Logger.

Note: This library logs all request to your api during lifetime of project. 
Please make sure to exclude unused requests for example starting with "swagger"

## License

This library is licensed under the MIT License.