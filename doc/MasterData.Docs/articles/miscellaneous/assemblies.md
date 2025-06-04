# Assemblies

JJMasterData dependency tree can be represented by the following diagram:

![Dependencies](../../media/JJMasterDataDependencies.png)

## Assemblies

### JJMasterData.Web 
Target Frameworks: ..NET 8
</br>
Razor Class Library with web graphical interfaces to manipulate and render your metadata.

### JJMasterData.WebApi
Target Framework: ..NET 8
</br>
Restful API to consume your metadata at any front-end.

### JJMasterData.Core
Target Frameworks: ..NET 8, .NET Standard 2.0 and .NET Framework 4.8
</br>
Library to access your metadata from .NET and/or render it at HTML components.

### JJMasterData.Commons
Target: .NET Standard 2.0
</br>
Utilities to all assemblies, like database access, l10n, logging and utils.