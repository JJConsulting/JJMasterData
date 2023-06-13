# JJFormView

JJFormView is the heart of the JJMasterData CRUDs. 

## Usage

At your Controller, create a JJFormView instance and use it as your Model or add it as a property to your ViewModel.

```csharp
    var formView = new JJFormView("YourDataDictionaryName")
```

At your view, simply call GetHtmlString().

```csharp
    @Model.GetHtmlString()
```

## Customization

With this instance, you can customize nearly everything from your DataDictionary. For example,
if you want to programmatically change your CRUD title, your can simply call:

```csharp
    formView.FormElement.Title = "Your Title"
    formView.FormElement.Fields["YOUR_FIELD"].VisibleExpression = "YourLogicHere"
```

In a nutshell, use FormElement property to customize DataDictionary configurations, like your fields. 
You can check all properties from FormView at our [API Reference](https://portal.jjconsulting.com.br/jjdoc/lib/JJMasterData.Core.WebComponents.JJFormView.html)
