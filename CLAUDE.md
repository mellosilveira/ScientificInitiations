# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This repository contains two scientific initiation research projects in engineering:

1. **Mechanical Vibrations** (`Mechanical vibrations/`) — Finite Element Method (FEM) and rigid body dynamics analysis
2. **Suspension System** (`Formula and Baja SAE/Suspension/`) — Suspension analysis for Baja SAE racing vehicles, with both a C# REST API and a Python automation tool

## Commands

### C# Projects

Both C# projects use standard `dotnet` CLI. Run from within the project directory or solution root.

```bash
# Build
dotnet build

# Run tests
dotnet test

# Run a single test project
dotnet test "Mechanical vibrations/IcVibracoes.Test/IcVibracoes.Test.csproj"
dotnet test "Formula and Baja SAE/Suspension/test/Suspension.UnitTest/Suspension.UnitTest.csproj"

# Run the API (from solution directory)
dotnet run --project "Formula and Baja SAE/Suspension/src/Suspension.Application"
dotnet run --project "Mechanical vibrations/IcVibracoes"
```

Build outputs go to `build/bin/` and `build/tmp/` (configured via `Directory.Build.props`).

### Python Project

```bash
# Run the Python suspension tool
python "Formula and Baja SAE/Suspension/python/main.py"
```

No `requirements.txt` exists — dependencies must be inferred from imports in the source files.

## Architecture

### Mechanical Vibrations (`Mechanical vibrations/`)

ASP.NET Core 6.0 REST API for structural vibration analysis.

- **IcVibracoes** — API entry point; controllers for `FiniteElement` and `RigidBody` endpoints
- **IcVibracoes.Core** — Calculation engine: eigenvalue solvers, differential equations of motion, geometric property calculators (circular/rectangular), and numerical integrators (Newmark, Newmark-Beta, Runge-Kutta 4th order)
- **IcVibracoes.DataContracts** — Request/response DTOs
- **IcVibracoes.Common** — Shared utilities
- **IcVibracoes.Test** — Unit tests

### Suspension System (`Formula and Baja SAE/Suspension/`)

ASP.NET Core 9.0 REST API plus a Python automation layer.

**C# layers:**
- **Suspension.Application** — API entry point; controllers for `Reaction`, `StaticAnalysis`, `FatigueAnalysis`, and `DynamicAnalysis`
- **Suspension.Core** — Business logic; generic operations (`RunStaticAnalysis<T>`, `RunFatigueAnalysis<T>`) parameterized by profile types; depends on the `MelloSilveiraTools` NuGet package (custom mechanics-of-materials library, v1.2.0-rc03)
- **Suspension.DataContracts** — DTOs; includes domain models for `SuspensionSystem`, `Wishbone`, `TieRod`, `ShockAbsorber`, `SteeringKnuckle`

**Python layer (`python/`):**
- `main.py` — Primary entry point (~1,400 lines)
- `calculators/` — Mathematical/physics calculators
- `models/` — Data structures
- `orchestrators/` — Workflow orchestration
- `ui_ux/` — Terminal UI components

### Key Patterns

- Dependency Injection throughout both C# projects
- Generic operations for reusable analysis logic across different cross-section profile types
- Swagger/OpenAPI enabled on both APIs (accessible at `/swagger` when running)
- Sample requests/responses in `request/` and `response/` directories; generated solutions in `solutions/`
