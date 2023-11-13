# Using ConsoleApp to version control Form Elements


## Configuration
At your ConsoleApp `appsettings.json` at `JJMasterData` section you must set:
- DataDictionaryFolderPath: The folder where your version controled dictionaries are stored. 
- ConnectionString: The connection string where your data dictionaries will be imported

```json
{
    "JJMasterData":
    {
        "ConnectionString":"",
        "DataDictionaryFolderPath":"/home/gumbarros/Elements/"
    }
}
```

At your CI/CD pipeline simply run

`JJMasterData.ConsoleApp.exe import`
