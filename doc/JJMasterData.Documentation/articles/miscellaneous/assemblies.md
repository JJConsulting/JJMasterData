# Dependencies

JJMasterData dependencies can be represented by the following diagram:

![Dependencies](../../media/JJMasterDataDeps.png)

## Assemblies

### JJMasterData.Web.Bootstraper 
Target: .NET 6
</br>
Website to use the Razor Class Library during development.
### JJMasterData.Web 
Target: .NET 6 
</br>
DataDictionary Razor Class Library.
### JJMasterData.Api
Target: .NET 6
</br>
RESTful API to access the DataDictionary.

### JJMasterData.Core
Targets: .NET 6, .NET Standard 2.0 and .NET Framework 4.8
</br>
Class library with the components to render the data in HTML.

### JJMasterData.Commons
Target: .NET Standard 2.0
</br>
Class library with utilities to all assemblies, like database access, l10n, logging and utils.