# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Timetable Planning App is a web application for model railway operation planning at module meetings. It generates print-optimized scheduling reports (driver duties, station duties, timetables, pocket cards) in multiple languages. The app replaces a legacy Microsoft Access database with a modern Blazor WebAssembly frontend.

## Build Commands

```bash
# Build entire solution
dotnet build "Timetable Planning App.sln"

# Run server (starts at https://localhost:5001)
dotnet run --project src/App.Server/App.Server.csproj

# Run all tests
dotnet test

# Run specific test project
dotnet test src/App.Server.Tests/App.Server.Tests.csproj

# Run single test by name filter
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Run with coverage
dotnet test /p:CollectCoverage=true

# Release build
dotnet build -c Release
```

## Architecture

```
Client (Blazor WebAssembly) <-> Server (ASP.NET Core) <-> Repositories.Access <-> Microsoft Access Database
```

**Projects:**
- `App.Client` - Blazor WASM frontend with print-optimized Razor components
- `App.Server` - ASP.NET Core API host
- `App.Contract` - Shared data models and service interfaces
- `Repositories.Access` - Data access via ODBC to Access database

**Data flow:** Pages call `PrintedReportsService` → HTTP to `ReportsController` → `AccessPrintedReportsStore` → SQL via Mappers → Domain objects returned as JSON

## Key Domain Concepts

- **Layout** - Railway configuration for a specific meeting/event
- **Train** - Has operator, category, operation days, and StationCalls
- **OperationDays** - Bit flags (0x00-0x7F) for multi-day scheduling patterns
- **Duty** - Abstract base for `DriverDuty` and `StationDuty`
- **DutyBooklet** - Print-ready booklet with pagination for A5 folding

## Important Patterns

**Pagination for print:** Uses height-based pagination in `PaginationExtensions` for breaking content across A5/A4 pages:
```csharp
pages.Pages(item => item.Height(), maxHeight, pagebreak)
```

**Booklet page ordering:** Special ordering for print-and-fold booklets (e.g., 8-page: [8, 1, 2, 7, 6, 3, 4, 5])

**Mappers:** Data transformation in `Repositories.Access/*Mapper.cs` converts database records to domain objects

**Extension methods:** Heavy use throughout (`StringExtensions`, `TimeExtensions`, `MarkdownExtensions`, etc.)

## API Endpoints

Base: `/api/layouts/{layoutId}/reports/`

Key endpoints: `driverduties`, `stationduties`, `locoschedules`, `trainsetschedules`, `timetablestretches`, `traincompositions`, `stationstrainorders`

## Localization

Supports: English, Swedish, Danish, German, Norwegian via RESX files in `App.Client/Resources/`. Uses browser language detection.

## Configuration

- **Database:** Configure ODBC connection in `src/App.Server/appsettings.json` under `RepositoryOptions.ConnectionString`
- **Client settings:** Layout ID and fonts in `src/App.Client/wwwroot/appsettings.json`

## Tech Stack

- .NET 10.0, C# latest
- Blazor WebAssembly (hosted)
- MSTest for testing
- Markdig for markdown processing
- System.Data.Odbc for Access database
