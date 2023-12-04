By default DataAccess recover connection string from appsettings.json with name ```ConnectionString```,
but you can pass a custom string connection in the constructor.

## How use?
Example to return a DataTable from a DataBase:
```csharp
var dataAccess = new DataAccess();
var cmd = new DataAccessCommand();
cmd.Sql = "select id, name from table1 where group = @group";
cmd.Paramneters.Add(new DataAccessCommand("@group", "G1"));
 
DataTable dt = await dataAccess.GetDataTableAsync(cmd);
```

## How to execute a sequence of commands?
This example shows how to execute a sequence of commands:<br>
```csharp
   class TestClass  
   { 
       var dataAccess = new DataAccess())
        try
        {
            var commands = new List<DataAccessCommand>
            {
                new("update table1 set ..."),
                new("update table2 set ..."),
            };
            await dataAccess.SetCommandAsync(commands);
        }
        catch (Exception ex)
        {
            //Handle your exception
        }
   }
```
