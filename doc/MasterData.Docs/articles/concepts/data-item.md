# Data items

Use `FormElementDataItem` to provide the available values for a selection component: `ComboBox`, `Search`, `Lookup`, `RadioButtonGroup` or `Hierarchy`. Configure it on the form field's `DataItem` property. The field stores the selected ID; the data item supplies the description and, when configured, icon and grouping information.

## Choose a source

Set `DataItemType` to exactly one source:

| Type | Use when | Configure |
| --- | --- | --- |
| `Manual` | The list is small and fixed. | Add `DataItemValue` objects to `Items`. Each needs an `Id`; set `Description` for the label. |
| `SqlCommand` | Values come from a query or change with form data. | Set `Command` to a query returning `Id` and `Description`, in that order. |
| `ElementMap` | Values come from another JJMasterData element. | Set `ElementMap.ElementName`, `IdFieldName`, and `DescriptionFieldName`. |

For example, a manual list can be configured like this:

```csharp
field.DataItem = new FormElementDataItem
{
    DataItemType = DataItemType.Manual,
    Items =
    [
        new DataItemValue("draft", "Draft"),
        new DataItemValue("published", "Published")
    ]
};
```

A basic SQL source can be configured with a `DataAccessCommand`:

```csharp
field.DataItem = new FormElementDataItem
{
    DataItemType = DataItemType.SqlCommand,
    Command = new DataAccessCommand("SELECT Id, Name FROM Status ORDER BY Name")
};
```

Use query parameters for values supplied by the form. Do not concatenate user input into SQL. For an `ElementMap`, use `MapFilters` to limit the mapped records; filter expressions can reference other form fields. Set `AutoPostBack` on a source field when changing it must reload a dependent selection field.

## Configure selection behavior

These settings are shared by data-item components; the selected component determines which settings apply:

| Property | Effect |
| --- | --- |
| `FirstOption` | Adds a leading option such as “All” or an empty choice. |
| `EnableMultiSelect` | Allows more than one ID. Use only when the field's storage and save/read logic can represent multiple values; a write-only field or a separate relationship table is usually appropriate. |
| `ShowIcon` | Shows item icons where the component supports them. Supply icon values in manual items, query results, or the element map. |
| `GridBehavior` | Controls how the selected value is represented in the grid. |
| `EnableLocalization` | Localizes item descriptions. |
| `RadioLayout` | Selects the radio button group's layout. |

`DataItemValue` can carry `Id`, `Description`, `Icon`, `IconColor`, `ImageUrl`, and `Group`. For an SQL source, the standard query returns ID and description. When icons or groups are needed, return the corresponding values in the order expected by the data-item reader and configure the matching display option.

## Dependent lists

To make one list depend on another field:

1. Configure the dependent query or `ElementMap` filter to use the source field's current value.
2. Set `AutoPostBack = true` on the source field.
3. When the source changes, the form reloads and the dependent list is populated again.

Use this for, for example, Country → State → City. See [expressions](expressions.md) for calculated values and conditional visibility or enablement.

## Hierarchy

Choose `Hierarchy` when each option belongs under a parent and the user should select one node. The stored value is that node's ID. Roots have no parent; each other node has a `ParentId`.

### Manual source

Use the normal `Items` list and set `ParentId` on each child:

```csharp
Items =
[
    new DataItemValue("company", "Company"),
    new DataItemValue("engineering", "Engineering") { ParentId = "company" },
    new DataItemValue("sales", "Sales") { ParentId = "company" }
]
```

Manual hierarchies are checked for duplicate IDs, missing parents, self-parenting, cycles, and roots.

### SQL source

The SQL command must return these columns in order:

1. `Id`
2. `Description`
3. `ParentId`
4. `IconId`, when `ShowIcon` is enabled
5. `IconColor`, when `ShowIcon` is enabled

The same query handles both loading children and restoring an existing selection. `{SearchId}` is set when JJMasterData requests the selected item by ID. Otherwise, `{ParentId}` is set to the expanded node's ID; a null parent requests roots. Both placeholders are database parameters.

```sql
SELECT Id, Name, ParentId
FROM Organization
WHERE ({SearchId} IS NOT NULL AND Id = {SearchId})
   OR ({SearchId} IS NULL AND
       (({ParentId} IS NULL AND ParentId IS NULL) OR ParentId = {ParentId}))
ORDER BY Name
```

The UI loads a node's children when it is expanded and caches the result. Selecting a node closes the tree. If the field is optional, the user can clear the selection. Hierarchy does not support multiple selection; its filters support only `None` and `Equal`.

### Element map source

Configure the same mapping fields as for a regular `ElementMap`, then set `ParentIdFieldName` to the field containing the parent's ID. The mapped element must expose a root value with a null or empty parent ID.

## Related

- [Components and file fields](components.md) for choosing the editor and configuring files.
- [Data dictionary model](data-dictionary.md) for fields and element structure.
