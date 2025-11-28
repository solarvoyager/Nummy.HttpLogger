# Nummy Http Request & Response Logging package for .NET Core

[![NuGet Version](https://img.shields.io/nuget/v/Nummy.HttpLogger.svg)](https://www.nuget.org/packages/Nummy.HttpLogger/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Nummy.HttpLogger.svg)](https://www.nuget.org/packages/Nummy.HttpLogger/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Overview

This is a .NET Core library for http request and response logging in your application.

---

## Installation

[Nuget - Nummy.HttpLogger](https://www.nuget.org/packages/Nummy.HttpLogger)

or install the package via NuGet Package Manager Console:

```bash
Install-Package Nummy.HttpLogger
```

## Getting Started

#### 1. Run Nummy on your Docker

[Here is tutorial](https://github.com/solarvoyager/Nummy/blob/master/README.md)

#### 2. Add your application in Nummy

#### 3. Configure in your project

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
    // exclude urls containing strings
    options.ExcludeContainingPaths = new []{ "swagger", "api/user/login", "user/create" };
    // mask sensetive headers (labeled as [MASKED] in Nummy)
    options.MaskHeaders = new []{ "Authorization" }
    // max read response body length in bytes
    options.MaxBodyLength = 32768 // 32 KB
    // from your application's configuration section in Nummy
    options.NummyServiceUrl = "your-nummy-service-url";
    options.ApplicationId = "your-nummy-application-id";
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

> **Attention:** if you are using [Nummy.ExceptionHandler](https://www.nuget.org/packages/Nummy.ExceptionHandler),
> make sure to first register NummyHttpLogger and then NummyExceptionHandler.

#### 4. Now, your application is set up to log http request and responses using the Nummy Http Logger.

> **Attention 2:** This library logs all request to your api during lifetime of project.
> Please make sure to exclude unused requests for example starting with "swagger"

## License

This library is licensed under the MIT License.