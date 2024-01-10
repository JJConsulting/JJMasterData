<h1>Localization</h1>


First you will need to add the following line to your Program.cs, with the additional cultures you will be using:
```cs
app.UseJJMasterDataWeb(options =>
{
    options.AdditionalCultures.Add(new CultureInfo("es-AR"));
});

```
Now we have 3 scenarios to accomplish this:

## Resource File
Fork JJMasterData, add a resource file for your language like [this one](https://github.com/JJConsulting/JJMasterData/blob/main/src/Commons/Localization/MasterDataResources.pt-BR.resx), and send us a pull request. Other people from your country will have the values already out of the box.

## Database
Go to ```/en-us/DataDictionary/Localization``` or click at the localization modal located at Data Dictionary and populate the strings with your culture values, but only your system will reflect these changes

<img alt="Localization Modal" src="../media/Localization.png"/>
<br>

> [!TIP] 
> By default we write the resources in a table, allowed the user create dictionaries dynamically by adding translated words and texts, you can set the table name with the TableResources property in the JJMasterDataOptions class.

## Implement IStringLocalizer<MasterDataResources>

See [ASP.NET Core Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-8.0) for more information.