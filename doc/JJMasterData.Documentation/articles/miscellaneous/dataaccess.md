By default DataAccess recover connection string from appsettings.json with name ```ConnectionString```,
but you can pass a custom string connection in the constructor.

## How use?
Example to return a DataTable from a DataBase:
```csharp
var dataAccess = new DataAccess();
var cmd = new DataAccessCommand();
cmd.Sql = "select id, name from table1 where group = @group";
cmd.Paramneters.Add(new DataAccessCommand("@group", "G1"));
 
DataTable dt = dataAccess.GetDataTable(cmd);
```

## How maintain opened connection? 
Keeps the database connection open,
Allowing to execute a sequence of commands;

This example shows how the KeepConnAlive method should be used:<br>
```csharp
   class TestClass  
   { 
       private void Test()
       {
           var dao = new DataAccess())
           try
           {
               dao.KeepConnAlive = true;
               dao.SetCommand("update table1 set ...");
               dao.SetCommand("update table2 set ...");
           }
           catch (Exception ex)
           {
               //Do Log
           }
           finally
           {
              dao.KeepConnAlive = false;
           }
       }
   }
```
