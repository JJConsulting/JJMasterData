# Custom Rules

There are three ways to customize MasterData

- Event Handlers
- Editing your JJFormView at runtime
- Stored procedures

The first way is the recommended, Event Handlers don't customize the database and they reflect on systems that use JJMasterData.WebApi. They also works on any JJFormView object.

# Event Handlers

## Overview
The purpose of a class implementing <xref:JJMasterData.Core.Events.Abstractions.IFormEventHandler>, is to patronize the customizations and programmatically customize your dictionary rules without a custom view.

To customize your business rules you can use one of these:

## C# interface implementations
A .NET Form Event is a class that implements the <xref:JJMasterData.Core.Events.Abstractions.IFormEventHandler> interface.

Implement <xref:JJMasterData.Core.Events.Abstractions.IFormEventHandler> in your class and then generate your overrides with CTRL+. in your class using VS or Rider:

Your implementation will look like this:
```cs
public class MyFormEventHandler(Service service) : IFormEventHandler
{  
    public IEnumerable<string> GetCustomizedFields()
    {
        yield return "my_field";
    }
    
    public async ValueTask OnAfterInsertAsync(object sender, FormAfterActionEventArgs args)
    {
        var myFieldValue = Convert.ToInt32(args.Values["my_field"]);
                
        await service.DoCustomRule(myFieldValue);
    }
}
```
Then wire up your event like this:

```csharp
builder.Services.AddFormEventHandler<MyFormEventHandler>("MyElementName");
```

> [!TIP] 
> If you add the <xref:JJMasterData.Core.Events.Abstractions.IFormEventHandler.GetCustomizedFields> method at your class, your customized fields will look like the image below.

<br>
<img alt="Customized Fields" src="../media/CustomizedFieldsAttribute.png"/>

> [!TIP] 
> If you want to also customize Grid events, you can also implement <xref:JJMasterData.Core.UI.Events.Abstractions.IGridEventHandler> at your class.

## IFormEventResolver

<xref:JJMasterData.Core.Events.Abstractions.IFormEventHandlerResolver> is the responsible for retrieving <xref:JJMasterData.Core.Events.Abstractions.IFormEventHandler> implementations from a element name.
You can also create your own implementation.
At your Program.cs use:
```cs
builder.Services.AddJJMasterDataWeb().WithFormEventResolver<TYourImplementation>();
```

# Customizing your FormView

```cs
var formView = await FormViewFactory.CreateAsync("ElementName");
formView.FormElement.Title = "Runtime customization".
```

> [!WARNING]
> These changes will only reflect at this specific instance.



# Customizing with stored procedures

In your stored procedure (defined at your Data Dictionary) you can throw a custom exception with a code `>` than 50000.

```sql
IF (MyCustomLogic, you can use your @Parameters)
  THROW 50001, 'My custom validation rule', 1
```

> [!WARNING] 
> It is not a best practice write business rules at database, recommended only for simple validations.

---

