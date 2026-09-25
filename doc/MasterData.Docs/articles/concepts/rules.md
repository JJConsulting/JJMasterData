# Validation rules

A dictionary's `Rules` collection contains `FormElementRule` objects. `FormService` uses these rules to validate values before persistence. Any returned error prevents the write.

In the Data Dictionary UI, open the element and select the **Rules** tab. Give each rule a name, select SQL or JavaScript as its language, and enter its script. Use rules when field-level validation is not enough, such as checking combinations of values or existing records.

Choose the execution language according to the validation:

- **SQL:** query existing data using the dictionary's connection and return rows describing errors.
- **JavaScript:** check submitted values on the server through Jint, using `values` and `addError`. Browser APIs are not available.

For example, validate `Amount > 0` with a rule instead of only disabling the Save button. The validation also participates in imports when `RunOnBeforeImport` is enabled.

Rules run before insert, update and import by default. Delete is opt-in through `RunOnBeforeDelete`; design delete rules around the record keys supplied to that operation.

Use field names as error keys to associate messages with the corresponding editor. Rules still run when individual field validation is skipped because an editor is hidden or disabled.

Use [expressions](expressions.md) for calculated values and UI conditions. When validation requires injected application services, use a [before-event](../developer-guide/form-events.md).

## Rule syntax and operation flags

Set each `FormElementRule`'s `Name`, `Language` (`Sql` or `JavaScript`) and `Script`. Scripts do not use the `sql:` or `exp:` expression prefixes.

| Operation flag | Default |
| --- | --- |
| `RunOnBeforeInsert` | `true` |
| `RunOnBeforeUpdate` | `true` |
| `RunOnBeforeImport` | `true` |
| `RunOnBeforeDelete` | `false` |

### SQL

SQL runs through `IEntityRepository` using the dictionary's `ConnectionId`. Field placeholders become command parameters:

```sql
SELECT 'Amount', 'Amount must be positive.'
WHERE {Amount} IS NULL OR {Amount} <= 0
```

Return no rows for success. With two columns, the first is the field/error key and the second is the message. A one-column result produces a general error. Empty messages are ignored; duplicate keys within a rule keep the first message.

Unlike field expressions, SQL rules receive the operation's values directly; do not assume session or claim placeholders are available.

### JavaScript

Scripts execute on the server through Jint with `values`, `formElement` and `addError`:

```javascript
if (values.Amount == null || values.Amount <= 0) {
    addError("Amount", "Amount must be positive.");
}
```

Use `addError("Message")` for a general error, or `addError("FieldName", "Message")` for a field error. No errors means success. Messages pass through the library localizer during rule validation.

For validation requiring dependency injection, use [form events](../developer-guide/form-events.md).

### More validation examples

#### Reject a duplicate value

Return a field error only when another customer already uses the submitted email:

```sql
IF EXISTS (
    SELECT 1
    FROM Customer
    WHERE Email = {Email}
      AND Id <> ISNULL({Id}, 0)
)
    SELECT 'Email', 'This email is already in use.'
```

This SQL Server example assumes `Customer` has `Id` and `Email` columns, with positive identifiers.

#### Validate related fields

A JavaScript rule can report more than one error:

```javascript
if (!values.StartDate) {
    addError("StartDate", "Start date is required.");
}
if (!values.EndDate) {
    addError("EndDate", "End date is required.");
}
if (!values.Name && !values.CompanyName) {
    addError("Either Name or CompanyName must be filled.");
}
if (values.Type === "Company" && !values.Document) {
    addError("Document", "Document is required for companies.");
}
```

Field names passed to `addError` must match the dictionary. General errors appear as form-level validation messages.
