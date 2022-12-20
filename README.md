
<h1 align="center">
  <br>
<img width=25% src="doc/JJMasterData.Documentation/media/JJMasterDataLogo.png"/>
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
  <a href="https://www.nuget.org/profiles/jjconsulting">
    <img src="https://img.shields.io/nuget/v/JJMasterData.Web.svg?color=004880" alt="NuGet">
  </a>
  <a href="https://discord.gg/s9F2ntBXnn">
    <img src="https://img.shields.io/discord/984473468114456667?color=5b62ef&label=discord" alt="Discord">
  </a>
</p>

JJMasterData is an open-source .NET library to help you create dynamic CRUDs quickly from data dictionaries (database metadata), along with other boilerplate-intensive things like exporting and importing data.

## Features
- Pages generated at runtime 🔥
- Data exportation & importation ↔️
- Database script generation ✍️
- Plugins support by [interfaces](https://portal.jjconsulting.com.br/jjdoc/articles/plugins/intro.html) 🪄
<br>

# Demo
![Demo](doc/JJMasterData.Documentation/media/Demo.gif)

<br>

## Useful Links
* [Library Documentation](https://portal.jjconsulting.com.br/jjdoc/articles/intro.html)
* [NuGet.Org](https://www.nuget.org/profiles/jjconsulting)
* [Community Discord Server](https://discord.gg/s9F2ntBXnn)

<br>

## Getting Started

https://user-images.githubusercontent.com/28662273/190284230-9173fcf9-b0c1-493b-bd4b-30ca08b432fa.mp4

See all steps in [documentation](https://portal.jjconsulting.tech/jjdoc/articles/getting_started.html).

<br>

## Building from source 🧰
1. Install [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

2. Install [NodeJS](https://nodejs.org/en/download/)

3. Clone this git repository

4. Open `JJMasterData.sln `file at your IDE

5. Set the `JJMasterData.WebExample` as startup project

6. At `src/JJMasterData.Web` run at your terminal
```bash
npm i
```
7. It will be necessary to add a database of your choice. You will need add a ConnectionString at your `IConfiguration` source.<br>
![image](https://user-images.githubusercontent.com/100393691/203789109-ef71f492-3f90-4739-8c41-8a92890c72dc.png)

8. Run the project

9. This is the expected output <br>
![image](https://user-images.githubusercontent.com/52143624/205990349-fc9c24d1-c9e6-4729-a334-4d0487222d29.png)


## Special Thanks

#### Code contributors
![Alt](https://repobeats.axiom.co/api/embed/d509fb71a5aae2a10fe80b8d163936470ef90925.svg "Repobeats analytics image")
<a href="https://github.com/jjconsulting/JJMasterData/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=jjconsulting/jjmasterdata" />
</a>

<br>

## Bugs and feature requests 🐛
Have a bug or a feature request? 
Please first search for existing and closed issues.</br>
If your problem or idea is not addressed yet, [please open a new issue](https://github.com/jjconsulting/JJMasterData/issues/new).
