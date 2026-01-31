# Truebooks.Platform.Core

Core infrastructure for TruebooksERP - Identity, Tenancy, DbContext, and common utilities.

## Overview

This package provides the foundational services and infrastructure needed to build TruebooksERP modules with multi-tenant support.

## Key Features

- **Multi-Tenancy** - ITenantContext, TenantMiddleware, automatic TenantId stamping
- **Module System** - IModuleRegistry, ModuleMiddleware for per-tenant module access control
- **Database** - PlatformDbContext with tenant query filters
- **Extensions** - AddPlatformCore(), UsePlatformMiddleware() for easy integration

## Installation

```bash
dotnet add package Truebooks.Platform.Core
```

## Usage

```csharp
// In Program.cs
builder.Services.AddPlatformCore(builder.Configuration);
builder.Services.AddModuleManifest<MyModuleManifest>();

var app = builder.Build();
app.UsePlatformMiddleware();
```

## Dependencies

- Truebooks.Platform.Contracts
- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.AspNetCore.Identity.EntityFrameworkCore

## Version History

- 1.0.0 - Initial release with core infrastructure
