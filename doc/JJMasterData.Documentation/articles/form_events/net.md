# .NET Form Events

## What is a .NET Form Event?

A .NET Form Event is a that inherits from BaseFormEvent which consequently implements the IFormEvent interface. 
By creating such a class it is possible to execute code in all events defined by the interface.

```csharp
    public class MyDictionaryName : BaseFormEvent
    {

        public override void OnBeforeInsert(object sender, FormBeforeActionEventArgs e)
        {
            //TODO: Your code here.
        }
    }
```

If you are using .NET Framework and want to display your assemblies in a external Data Dictionary website, add to the external site Web.Config, the following key:
If for some specific reason are with this scenario in .NET 6, set this key using JJMasterDataSettings.

```xml
	<add key="JJMasterData.ExternalAssemblies" value="C:\\FullPathToTheMainApplicationAssembly" />
```