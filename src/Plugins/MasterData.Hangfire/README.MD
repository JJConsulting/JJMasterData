# JJMasterData Hangfire

This optional package replaces JJMasterData's default in-memory job queue with Hangfire.

Configure a Hangfire storage and server in the host application, then register the backend:

```csharp
services.AddHangfire(configuration =>
    configuration.UseSqlServerStorage(connectionString));
services.AddHangfireServer();

services.AddJJMasterDataCore()
    .WithHangfire();
```

Import and export requests remain serializable values. Scoped handlers and registered export/import formats are resolved by Hangfire when each job executes.
