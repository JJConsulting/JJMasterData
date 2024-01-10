# Using FormElementImporter to version control Form Elements


## Configuration
At your FormElementImporter.exe root, in your `appsettings.json` at `JJMasterData` section you must set:
- DataDictionaryFolderPath: The folder where your version controlled FormElements are stored. 
- ConnectionString: The connection string where your FormElements will be imported

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
`JJMasterData.FormElementImporter.exe`
