## JJMasterData.Python

A plugin to enable Python support at certain features of JJMasterData.

## Setup

At your startup, add the following line:
```python
builder.Services.AddJJMasterDataWeb().WithPythonEvents()
```
The following IConfiguration keys are possible:

## FormEvents

To use FormEvents with Python, the Python file with your element name and implement a class with the same name.

```py
from JJMasterData.Core.Events.Abstractions import FormEventHandlerBase
from JJMasterData.Core.Events.Args import *

# Your class name AND file name, must be the same from your ElementName
class MyElementName(FormEventHandlerBase):
    def OnFormElementLoad(self, sender, args: FormElementLoadEventArgs):
        args.FormElement.Title = "My Title Changed in Python"

    def OnBeforeInsert(self, sender, args: FormBeforeActionEventArgs):
        if int(args.Values["NumericField"]) < 0:
            args.Errors["NumericField"] = "Value needs to be greater than 0"


```

> [!TIP]
> Since IronPython runs on the CLR, you can edit your Python files without re-compiling your app.

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
    
data_access_instance = System.Activator.CreateInstance(JJMasterData.Commons.Data.DataAccess)
    
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
> [!TIP]
> This is for demonstration purposes only, use async code to access your database.
