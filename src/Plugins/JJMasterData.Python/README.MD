## JJMasterData.Python

A plugin to enable Python support at certain features of JJMasterData.

## Data Dictionary Actions

At your startup, add the following line:
```python
builder.Services.AddJJMasterDataWeb().WithPythonEngine()
```
Now you can execute Python actions at your form.

## FormEvents

To use FormEvents with Python, dda python file with your element name and implement a class with the same name. Add the JJMasterData folder (coming soon to pip)
to enable autocomplete features.
Specify the path on the services setup.

```csharp
builder.Services.AddJJMasterDataWeb()
        .WithPythonFormEventResolver(options => options.ScriptsPath = "../../example/JJMasterData.Web.Example/FormEvents/Python");
```

```py
from JJMasterData.Core.FormEvents.Abstractions import BaseFormEvent
from JJMasterData.Core.FormEvents.Args import *

class Example(BaseFormEvent):

    def OnBeforeInsert(self, sender, args: FormBeforeActionEventArgs):
        if int(args.Values["NumericField"]) < 0:
            args.Errors["NumericField"] = "Value needs to be greater than 0"

    def OnMetadataLoad(self, sender, args: MetadataLoadEventArgs):
        args.Metadata.Form.SubTitle = "Hello from IronPython runtime."
```

> [!TIP]
> Since IronPython runs on the CLR and does not use Reflection as our default implementation, higher performance is expected.

## How to use .NET Code ?

You can write .NET Code using CLR (Common Language Runtime).
You can access any library available on runtime, including JJMasterData. You can find more about it [here](https://ironpython.net/documentation/dotnet/dotnet.html).

Below you can see a implementation of DataAccess in Python that uses the .NET runtime.

Types are defined with the same nomenclature of .NET.

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