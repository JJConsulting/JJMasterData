
# Python F.A.Q

## How to access the database ?

#### You can use the data_access object.

```py
data_access.set_command("EXEC YOUR_SQL_STUFF")
```
## How to use .NET Code ?

You can write .NET Code using CLR (Common Language Runtime). You can access any library available on runtime, including JJMasterData. You can find more about it [here](https://ironpython.net/documentation/dotnet/dotnet.html).

Below you can see the source code of the data_access module that uses the .NET runtime.

```py
import clr
clr.AddReference("JJMasterData.Commons")
import System
import JJMasterData.Commons.Dao
    
data_access_instance = System.Activator.CreateInstance(JJMasterData.Commons.Dao.DataAccess)
    
get_fields = lambda sql : dict(data_access_instance.GetFields(sql))
    
get_data_table = lambda sql : data_access_instance.GetDataTable(sql)
    
set_command = lambda sql : data_access_instance.SetCommand(sql)
```
## How to iterate through a DataTable ?

With a code like the example below:

```py
data_table = data_access.get_data_table("SELECT * FROM MY_TABLE")
    
for row in data_table.Rows:
    # do stuff with your str(row["COLUMN"])

```