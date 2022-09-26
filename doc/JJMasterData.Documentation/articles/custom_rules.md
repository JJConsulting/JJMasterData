# Customize Rules

The main way to customize is using the FormEvents class to define by the IFormEvent interface. These can be Python scripts or .NET assemblies.

## Overview
The purpose of a class implementing IFormEvent, is to padronize the customizations and programmaticaly customize your dictionary rules without a custom View.
<br>

Program.cs file
```cs
builder.Services.AddJJMasterDataWeb().WithFormEvents();
```

If the class it is not in the current assembly you must pass the assembly path on startup.
```cs
Assembly assemblyWithEvents = typeof(ProductsFormEvent).Assembly;
builder.Services.AddJJMasterDataWeb().WithFormEvents(assemblyWithEvents);
```

or

If you are using .NET Framework and want to display your assemblies in a external Data Dictionary website, add to the external site Web.Config, the following key:
If for some specific reason are with this scenario in .NET 6, set this key using JJMasterDataSettings.

```xml
	<add key="JJMasterData.ExternalAssemblies" value="C:\\FullPathToTheMainApplicationAssembly" />
```

> [!TIP] 
> To use them, just add WithFormEvents() method on AddJJMasterDataWeb(). It will automatically add any class implementing IFormEvent to the services container.

To customize bussiness rules on form follow theses steps:

## Customizing with C# 
A .NET Form Event is a that inherits from BaseFormEvent which consequently implements the IFormEvent interface. 
By creating such a class it is possible to execute code in all events defined by the interface.

Create a class with starts with the Data Dictionary name and inherits it from [BaseFormEvent.](../lib/JJMasterData.Core.FormEvents.Abstractions.BaseFormEvent.html) <br>

Press Alt+. on Visual Studio and click in Generate overrrides...
<img alt="CustomRules1" src="../media/CustomRules1.png"/>

Then choose the events to customize
<img alt="CustomRules2" src="../media/CustomRules2.png"/>

It's look like with this

```cs
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Web.DataDictionary
{
    public class ProductsFormEvent : BaseFormEvent
    {
        public override void OnInstanceCreated(JJFormView sender)
        {
            sender.FormElement.Title = "My Custom title";
        }
    }
}
```

## Customizing with Python

```py
data_access.set_command("EXEC YOUR_SQL_STUFF")
```
### How to use .NET Code ?

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

### How to iterate through a DataTable ?
With a code like the example below:

```py
data_table = data_access.get_data_table("SELECT * FROM MY_TABLE")
    
for row in data_table.Rows:
    # do stuff with your str(row["COLUMN"])

```

## Customizing with Database procedure 

In the MSSQL procedure you can throw a custom exception with error id > 50000. 

```sql
IF (1=1)
  THROW 50001, 'My Custom error message', 1
```

> [!WARNING] 
> It is not a best pratice, use to litle validations.



