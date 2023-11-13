# JJSearchBox

JJSearchBox is a searchable combobox.

## Usage

### SQL Query

### At your Controller

```csharp

    private readonly IControlFactory<JJSearchBox> _searchBoxFactory;

    public MyController(IControlFactory<JJSearchBox> searchBoxFactory)
    {
        _searchBoxFactory = searchBoxFactory;
    }

    ///You can also use IComponentFactory
    public MyController(IComponentFactory componentFactory)
    {
        _searchBoxFactory = componentFactory.Controls.SearchBox;
    }

    public async Task<IActionResult> YourAction()
    {
        var searchBox = await _searchBoxFactory.Create();
        
        //You can use SQL queries
        searchBox.DataItem = new FormElementDataItem
         {
             Command = new("SELECT ID, DESCRIPTION FROM TABLE")
          }
   
        
        //Or static values
        searchBox.DataItem = new FormElementDataItem
        {
            Itens = new List<DataItemValue>
            {
                new("ID","DESCRIPTION")
            }
        };
        var result = await searchBox.GetResultAsync();
        
        /// Here we intercept your POST request to search your data.
        if (result is IActionResult actionResult)
            return actionResult;
        
        var model = new Model(searchBox.Content);
        return View("YourView",model);
    }

```
### At your View

```html
    <h1>Hey, here is my dynamic searchBox:</h1>
    @Html.Raw(Model.SearchBoxHtml)
```

## Customization

You can check all properties from JJSearchBox at <xhref:JJMasterData.Core.UI.Components.JJSearchBox>