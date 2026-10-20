# Persist multiple selections

A project can have several labels. The form should show one multiple-choice field while the database stores one row per project/label relationship. Use a custom read and write procedure to translate between those representations.

## Define the contract

Assume tables `Project(Id, Name)`, `Label(Id, Name)` and `ProjectLabel(ProjectId, LabelId)`, with a unique or primary key on `(ProjectId, LabelId)` and foreign keys to both parent tables.

Add a string field called `LabelIds` to the project dictionary and allocate enough size for the serialized selections. Use `FormComponent.ComboBox`, set `DataItem.EnableMultiSelect = true`, and configure its SQL source to return label IDs and descriptions:

```csharp
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;

var field = form.FormElement.Fields["LabelIds"];
field.Component = FormComponent.ComboBox;
field.DataBehavior = FieldBehavior.Real;
field.DataItem = new FormElementDataItem
{
    DataItemType = DataItemType.SqlCommand,
    EnableMultiSelect = true,
    Command = new DataAccessCommand("SELECT Id, Name FROM dbo.Label ORDER BY Name")
};
```

Here `form` is the runtime `JJFormView` created by the form factory. The combo box uses a comma-separated `SelectedValue`. The query returns the stored ID first and the description second; see [list components](../concepts/components.md#list-and-relationship-components).

`LabelIds` is not a physical column. When both reading and writing it through custom procedures, use `DataBehavior = Real` so it participates in both contracts. This deliberately makes the generated table/read/write scripts unsuitable for direct deployment: maintain the actual schema and both procedures yourself. Do not add a fake `LabelIds` column merely to satisfy the generator.

A `Virtual` field never enters the write command. `ViewOnly` cannot submit the selection; `WriteOnly` cannot restore it when reopening a record. See [field data behavior](../concepts/data-dictionary.md#field-data-behavior).

## Read the selection

For SQL Server versions supporting `STRING_AGG`, include a correlated projection in the custom read procedure:

```sql
SELECT p.Id, p.Name,
       (SELECT STRING_AGG(CONVERT(VARCHAR(MAX), pl.LabelId), ',')
               WITHIN GROUP (ORDER BY pl.LabelId)
        FROM dbo.ProjectLabel AS pl
        WHERE pl.ProjectId = p.Id) AS LabelIds
FROM dbo.Project AS p;
```

This is the projection to integrate, not a complete JJMasterData read procedure. Keep its key filters, ordering, pagination and count contract. An aggregate per project avoids multiplying grid rows.

## Write the relationships atomically

Generate the write signature and preserve `@action`, the project fields, `@LabelIds` and `@RET`. After choosing insert/update/delete, execute the base-row and relationship changes within one transaction. For insert, capture the new `Id` in a variable or table variable instead of returning it before the transaction commits.

Inside the insert/update branch, after the project exists, this fragment replaces its labels:

```sql
-- @Id identifies the saved project; @LabelIds is a comma-separated string.
DECLARE @Selected TABLE (LabelId INT NOT NULL PRIMARY KEY);

IF EXISTS
(
    SELECT 1 FROM STRING_SPLIT(COALESCE(@LabelIds, ''), ',')
    WHERE NULLIF(LTRIM(RTRIM(value)), '') IS NOT NULL
      AND TRY_CONVERT(INT, value) IS NULL
)
    THROW 50010, 'Invalid label selection.', 1;

INSERT @Selected (LabelId)
SELECT DISTINCT CONVERT(INT, value)
FROM STRING_SPLIT(COALESCE(@LabelIds, ''), ',')
WHERE NULLIF(LTRIM(RTRIM(value)), '') IS NOT NULL;

IF EXISTS
(
    SELECT 1 FROM @Selected AS s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Label AS l WHERE l.Id = s.LabelId)
)
    THROW 50011, 'A selected label no longer exists.', 1;

DELETE dbo.ProjectLabel WHERE ProjectId = @Id;
INSERT dbo.ProjectLabel (ProjectId, LabelId)
SELECT @Id, LabelId FROM @Selected;
```

Here null or empty input explicitly means **clear all labels**. For imports or partial-update APIs, decide whether omitted input means clear or preserve before adopting this contract; the write command sends missing fields as SQL null.

For deletion, remove child relationships before deleting the project, or use a reviewed cascade constraint. Do not run replacement logic after deleting the parent. Use `SET XACT_ABORT ON`, `TRY/CATCH`, commit on success and rollback/rethrow on failure. Account for an existing transaction if the procedure can also run inside a bulk command transaction. Return the saved key after successful completion and set the usual `@RET` operation code.

## Check the result

Save two labels, reopen the form, remove one and then clear the selection. Try duplicate identifiers, unknown identifiers and malformed input. Force a relationship failure and verify that the base project change rolls back too. Check deletion and import explicitly.

If relationship editing needs per-item quantities or dates, edit those records in a separate dictionary or a [composed document](composed-document.md) instead of encoding them into a single field.
