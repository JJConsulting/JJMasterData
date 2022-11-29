# JJSearchBox

JJSearchBox is a searchable combobox.

## Usage

### SQL Query

```csharp
@{
  var searchBox = new JJSearchBox
   {
     Name = "mySearchBox",
     DataItem = new FormElementDataItem
     {
         Command = new("SELECT ID, DESCRIPTION FROM TABLE")
     }
   };
}

/// ...
@Html.Raw(searchBox.GetHtml())
/// ...
```
### Values

```csharp
@{
    var search = new JJSearchBox
    {
        Name = "mySearchBox",
        DataItem = new FormElementDataItem
        {
            Itens = new List<DataItemValue>
            {
                new("ID","DESCRIPTION")
            }
        }
    };
}
/// ...
@Html.Raw(search.GetHtml())
/// ...
```

## Customization

You can check all properties from JJSearchBox at our [API Reference](https://portal.jjconsulting.com.br/jjdoc/lib/JJMasterData.Core.WebComponents.JJSearchBox.html)
