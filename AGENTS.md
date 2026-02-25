## Overview
JJMasterData is a .NET library and web UI for generating dynamic CRUDs from data dictionaries. The repo includes core libraries, ASP.NET UI, Web API, plugins, console apps, docs, and an example host app.

## Repo Map
- `src/MasterData.Commons` Core utilities, security, configuration, resources.
- `src/MasterData.Core` Core runtime, data dictionary, UI helpers, tasks.
- `src/MasterData.Web` ASP.NET Core UI layer (Razor views, tag helpers, assets). Uses Node packages in `src/MasterData.Web/package.json`.
- `src/MasterData.WebApi` Web API layer and OpenAPI helpers.
- `src/Plugins/*` Optional integrations (Brasil, Hangfire, MongoDB, Pdf, Python).
- `src/ConsoleApps/*` CLI tools for migrations/imports.
- `example/MasterData.WebEntryPoint` Example host app.
- `doc/MasterData.Docs` DocFX documentation site.
- `test/*` xUnit test projects (Commons, Core, Web, WebApi, Plugins).

## Build
- Install .NET SDK
- Install Node.js for `MasterData.Web` assets.
- Restore web assets:
  - `npm install --prefix src/MasterData.Web`
- Build from repo root:
  - `dotnet build`

## Test
- Run all tests:
  - `dotnet test`
- Or run a specific project, for example:
  - `dotnet test test/MasterData.Core.Test/MasterData.Core.Test.csproj`

## Docs
- DocFX site lives in `doc/MasterData.Docs`.
- Build docs:
  - `dotnet tool update -g docfx`
  - `docfx doc/MasterData.Docs/docfx.json`

## Notes
- `Directory.Build.props` defines versioning and analyzer settings for packages.
- The solution file is `JJMasterData.slnx` (used by `dotnet build` in CI).
