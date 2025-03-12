
<h1 align="center">
  <br>
<img width=25% src="doc/Documentation/media/JJMasterDataLogoVertical.png"/>
</h1>
<p align="center">
  <a href="https://img.shields.io/badge/.NET-5C2D91">
    <img src="https://img.shields.io/badge/.NET-512BD4?logo=dotnet" alt="..NET 8">
  </a>
  <a href="https://img.shields.io/badge/TypeScript-007ACC">
    <img src="https://img.shields.io/badge/TypeScript-007ACC?logo=typescript&logoColor=white" alt="TS">
  </a>
  <a href="https://img.shields.io/badge/Microsoft_SQL_Server-CC2927">
    <img src="https://img.shields.io/badge/SQL_Server-CC2927?logo=microsoft-sql-server&logoColor=white" alt="TS">
  </a>
  <a href="https://www.nuget.org/profiles/jjconsulting">
    <img src="https://img.shields.io/nuget/v/JJMasterData.Web.svg?color=004880" alt="NuGet">
  </a>
  <a href="https://discord.gg/s9F2ntBXnn">
    <img src="https://img.shields.io/discord/984473468114456667?color=5b62ef&label=discord" alt="Discord">
  </a>
</p>

JJMasterData is an open-source .NET library to help you create dynamic CRUDs quickly from data dictionaries (database metadata), along with other boilerplate-intensive things like exporting and importing data.

## Useful Links
* [Library Documentation](https://md.jjconsulting.tech/articles/intro.html)
* [NuGet.Org](https://www.nuget.org/profiles/jjconsulting)
* [Community Discord Server](https://discord.gg/s9F2ntBXnn)

<br>

## Features
- Components generated at runtime 🔥
- Data exportation & importation ↔️
- Database script generation ✍️
- Plugins support by interfaces 🪄
- Multiple forms using relationships⛓️
  
<br>

## Getting Started

https://github.com/JJConsulting/JJMasterData/assets/28662273/9b874c9d-2a2f-4d3b-9e78-846db446def2

1. Create an ASP.NET Core project (it can be Blazor, MVC, Razor Pages, whetever you like...)
2. Install `JJMasterData.Web`
```shell
dotnet add package JJMasterData.Web
```
3. Configure your `IConfiguration` source with a connection string at `JJMasterData:ConnectionString` and a secret key at `JJMasterData:SecretKey`
```json
{
  "JJMasterData": {
    "DataDictionaryTableName": "MasterData",
    "ConnectionString": "Server=localhost;Database=JJMasterData;Integrated Security=True;Trust Server Certificate=true",
    "ReadProcedurePattern": "{tablename}Get",
    "WriteProcedurePattern": "{tablename}Set",
    "SecretKey": "ExampleSecretKey"
  }
}
```
3. At `Program.cs` add the following lines:
```
using JJMasterData.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddJJMasterDataWeb();

///...

var app = builder.Build();

///...

//Required middlewares
app.UseStaticFiles();
app.UseSession();

app.MapJJMasterData();

app.Run();
```
4. Create a `wwwroot` folder if your project is empty
5. Run the project and visit `/en-US/DataDictionary/Element/Index` <br>
<img width="960" alt="image" src="https://github.com/JJConsulting/JJMasterData/assets/52143624/c4bf083d-38e1-486d-aaf2-2177d7ad77ef">


You can get the appsettings.json schema URL from [here](https://raw.githubusercontent.com/JJConsulting/JJMasterData/main/jjmasterdata.json)

See all steps in [documentation](https://md.jjconsulting.tech/articles/getting_started.html).

<br>

### Building from source 🧰
1. Install [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

2. Install [NodeJS](https://nodejs.org/en/download/)

3. Clone this git repository

4. Open `JJMasterData.sln `file at your IDE

5. Set the `WebEntryPoint` as startup project

6. At `src/Web` run at your terminal
```bash
npm i
```
7. Run the project
## Special Thanks

#### Code contributors
<a href="https://github.com/jjconsulting/JJMasterData/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=jjconsulting/jjmasterdata" />
</a>

<br>

![Alt](https://repobeats.axiom.co/api/embed/d509fb71a5aae2a10fe80b8d163936470ef90925.svg "Repobeats analytics image")




## Bugs and feature requests 🐛
Have a bug or a feature request? 
Please first search for existing and closed issues.</br>
If your problem or idea is not addressed yet, [please open a new issue](https://github.com/jjconsulting/JJMasterData/issues/new).
