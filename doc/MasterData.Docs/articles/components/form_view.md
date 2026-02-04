# FormView

JJFormView is the heart of the JJMasterData CRUDs. 

## Usage

At your Controller, create a JJFormView instance and use the result as your Model or add it as a property to your ViewModel.
### At your Controller

```csharp
    
    [ViewData]
    public string FormViewHtml { get; set; }

    private readonly IFormElementComponentFactory<JJFormView> _formViewFactory;

    public MyController(IFormElementComponentFactory<JJFormView> formViewFactory)
    {
        _formViewFactory = formViewFactory;
    }

    ///You can also use IComponentFactory
    public MyController(IComponentFactory componentFactory)
    {
        _formViewFactory = componentFactory.FormView;
    }

    public async Task<IActionResult> YourAction()
    {
        var formView = await _formViewFactory.CreateAsync("Your element name");

        var result = await formView.GetResultAsync();
        
        /// Here we intercept any async POST request, like pagination and search boxes.
        if (result is IActionResult actionResult)
            return actionResult;
        
        FormViewHtml = result.HtmlContent;
        return View("YourView");
    }
```

### At your View

```html
    <h1>Hey, here is my dynamic form:</h1>
    @ViewData["FormViewHtml"]
```

## Customization

With this instance, you can customize nearly everything from your DataDictionary. For example,
if you want to programmatically change your CRUD title, your can simply call:

```csharp
    formView.FormElement.Title = "Your Title"
    formView.FormElement.Fields["YOUR_FIELD"].VisibleExpression = "YourLogicHere"
```

In a nutshell, use FormElement property to customize anything from the Data Dictionary, like your fields. 
You can check all properties from FormView at <xhref:JJMasterData.Core.UI.Components.JJFormView>
