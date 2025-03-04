# Nummy Http Request & Response Logging package for .NET Core

[![NuGet Version](https://img.shields.io/nuget/v/Nummy.HttpLogger.svg)](https://www.nuget.org/packages/Nummy.HttpLogger/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Nummy.HttpLogger.svg)](https://www.nuget.org/packages/Nummy.HttpLogger/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Overview

This is a .NET Core library for http request and response logging in your application.
Just set connection string of your database then package will create and manage required tables for itself.

## Installation

[Nuget - Nummy.HttpLogger](https://www.nuget.org/packages/Nummy.HttpLogger)

or install the package via NuGet Package Manager Console:

```bash
Install-Package Nummy.HttpLogger
```

## Getting Started

#### 1. Run Nummy on your Docker and get DSN url of your local instance

[Here is tutorial](https://github.com/solarvoyager/Nummy/blob/master/README.md)

#### 2. Configure your application

In your `Program.cs` file add the following line:

```csharp
using Nummy.HttpLogger.Extensions;
using Nummy.HttpLogger.Models;
```

```csharp
// .. other configurations

builder.Services.AddNummyHttpLogger(options =>
{
    options.EnableRequestLogging = true;
    options.EnableResponseLogging = true;
    // exclude logging from requests which contains these patterns
    options.ExcludeContainingPaths = new []{ "swagger", "api/user/login", "user/create" };
    options.DsnUrl = "your-nummy-dsn-url";
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

> **Attetion:** if you are using [Nummy.ExceptionHandler](https://www.nuget.org/packages/Nummy.ExceptionHandler),
> make sure to first register NummyHttpLogger and then NummyExceptionHandler.

#### 3. Now, your application is set up to log http request and responses using the Nummy Http Logger.

Note: This library logs all request to your api during lifetime of project.
Please make sure to exclude unused requests for example starting with "swagger"

## License

This library is licensed under the MIT License.