
<h1 align="center">
  <br>
<img style="width:42%;height:42%" src="../media/JJMasterDataLogo.png"/>
    <br>
    JJMasterData
  <br>
</h1>
<p align="center">
  <a href="https://img.shields.io/badge/.NET-5C2D91">
    <img src="https://img.shields.io/badge/.NET-512BD4?logo=dotnet" alt=".NET 6">
  </a>
  <a href="https://img.shields.io/badge/TypeScript-007ACC">
    <img src="https://img.shields.io/badge/TypeScript-007ACC?logo=typescript&logoColor=white" alt="TS">
  </a>
  <a href="https://img.shields.io/badge/Microsoft_SQL_Server-CC2927">
    <img src="https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoft-sql-server&logoColor=white" alt="TS">
  </a>
  <a href="https://img.shields.io/badge/Python">
    <img src="https://img.shields.io/badge/Python-3776AB?logo=python&logoColor=white" alt="TS">
  </a>
  <a href="https://img.shields.io/nuget/v/JJMasterData.Web.svg">
    <img src="https://img.shields.io/nuget/v/JJMasterData.Web.svg?color=004880" alt="NuGet">
  </a>
  <a href="https://discord.gg/s9F2ntBXnn">
    <img src="https://img.shields.io/discord/984473468114456667?color=5b62ef&label=discord" alt="Discord">
  </a>
</p>

JJMasterData is an open-source .NET library to help you create CRUDs quickly from data dictionaries (database metadata), along with other boilerplate-intensive things like exporting and importing data.

## Features
- Complete CRUD operations ✅
- Pages generated at runtime ✅
- Data export & import ✅
- Hangfire support ✅
- Database script generation ✅
- IronPython support ✅
- Bootstrap 3,4 & 5 support ✅
- Log interface ✅
- Test library ✅

# Demo
![Demo](../media/Demo.gif)

## Useful Links
* [Library Documentation](https://portal.jjconsulting.com.br/jjdoc/)
* [NuGet.Org](https://www.nuget.org/profiles/jjconsulting)
* [Community Discord Server](https://discord.gg/s9F2ntBXnn)

## Quick Start

This tutorial assumes you will use .NET 6+, for .NET Framework 4.8 support,
check our [documentation](https://portal.jjconsulting.com.br/jjdoc/articles/introduction/netframework.html).

### 1. Install JJMasterData.Web from NuGet
![JJMasterData Nuget](../media/NuGet.png)

Installing JJMasterData.Web, will install all required dependencies.

### 2. Add a SQL Server ConnectionString to your configuration file
In your configuration file (normally appsettings.json), add a SQL Server connection string. Support for more DBMSs is planned.

### 3. Modify Program.cs
Add the following lines to your Program.cs
```csharp
//This line will add JJMasterData required services.
builder.Services.AddJJMasterDataWeb();

//Add this line before specifing your default route. It will use the required services and add the RCL routes.
app.UseJJMasterDataWeb();
```

Add the following lines to your _Layout.cshtml <head>
```html
<partial name="_MasterDataStylesheets"/>
<partial name="_MasterDataScripts"/>
<partial name="_MasterDataTheme"/>
```
**IMPORTANT**: If you have Bootstrap and jQuery installed, remove it from your _Layout.cshtml both CSS and JS,
JJMasterData already have Bootstrap and jQuery installed. You can choose Bootstrap 3, 4 and 5 using [JJMasterDataSettings](https://portal.jjconsulting.com.br/jjdoc/lib/JJMasterData.Commons.Settings.JJMasterDataSettings.html).
If you really want to use your own Bootstrap or jQuery files or CDNs, check _MasterDataStylesheets and _MasterDataScripts source code and specify all dependencies except Bootstrap.

### 4. Create your first Data Dictionary
In your preferred database-tool like Azure Data Studio or SSMS, create the following example table:
```sql
-- Create a new table called '[Person]' in schema '[dbo]'
-- Drop the table if it already exists
IF OBJECT_ID('[dbo].[Person]', 'U') IS NOT NULL
DROP TABLE [dbo].[Person]
    GO
-- Create the table in the specified schema
CREATE TABLE [dbo].[Person]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY,
    [FirstName] NVARCHAR(50) NOT NULL,
    [Age] INT NOT NULL
    -- Specify more columns here
    );
GO
```
Next, run your website and open in your browser the route /en-us/DataDictionary,
you will be presented with the following screen:

![DataDictionary Not Found](../media/DataDictionaryNotFound.png)

After clicking on "Next", click on "Add"

![DataDictionaries Home](../media/DataDictionariesHome.png)

Enter the table name (Person), checking the option "Import Fields", after that, you will have a representation of your metadata.

![Person Data Dictionary](../media/PersonDataDictionary.png)

Click on "Get Scripts" and then "Run Stored Procedures"

![Person Scripts.png](../media/PersonScripts.png)

After running the Stored Procedures Scripts, click on Preview, and you will have your CRUD with nearly zero code 🪄

![Person CRUD](../media/PersonCRUD.png)

## Ok, really cool! But how to use these CRUDs on my application?
JJFormView is the class responsible to render all JJMasterData CRUDs. It have many features out of the box like filters, data exportation and
a huge customization potential using .NET code, you can even inject Python code in your application at runtime.
You have 2 options to instantiate a JJFormView:

1. Using the /DataDictionary/Render/{dictionaryName} route
2. Creating a View or Page instantiating a JJFormView

For customization you have a lot of scenarios too:
1. Using the DataDictionary Web interface, we have lots of options, we don't have everything documented **yet**, but you can help submitting a PR
2. Using the IFormEvent interface (recommended), check the docs for more information
3. Customizing your own JJFormView object at your pages (the old school way of the JJ Consulting team in the WebForms era, not recommended)

## Who is using JJMasterData ?
JJMasterData is **production-ready** and is already being used by JJConsulting [customers](https://www.jjconsulting.com.br#clientes).

## Special Thanks

### Code contributors

<a href="https://github.com/jjconsulting/JJMasterData/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=jjconsulting/JJMasterData" />
</a>

## Bugs and feature requests
Have a bug or a feature request?
Please first search for existing and closed issues.</br>
If your problem or idea is not addressed yet, [please open a new issue](https://github.com/jjconsultingdev/JJMasterData/issues/new).