# Rules

Rules allow you to run custom scripts before `insert` and `update` operations in a `FormElement`.

Use them when field-level validations are not enough, and you need to validate combinations of values, check data in the database, or apply custom business rules.

## Where to configure

In the Data Dictionary UI, open your element and go to the `Rules` tab.

Each rule has:

- **Name**: Friendly identification for the rule
- **Rule Type**: Script language used by the rule
- **Script**: The validation logic itself

Currently supported languages:

- `SQL`
- `JavaScript`

## How errors work

Rules must produce validation errors only when something is wrong.

If a rule does not produce any error, the operation continues normally.

## SQL rules

SQL rules execute a query and interpret the returned rows as validation errors.

### Result contract

- Return **no rows**: validation succeeded
- Return **1 column**: general validation error
- Return **2 columns**: field error, where:
  - first column = field name
  - second column = error message
- Column names do not matter

### Available parameters

You can use form values as parameters with the syntax:

```sql
{FieldName}
```

### SQL examples

General error:

```sql
if exists (
    select 1
    from Customer
    where Document = {Document}
      and Id <> isnull({Id}, 0)
)
    select 'There is already another customer with this document.'
```

Field error:

```sql
if exists (
    select 1
    from Customer
    where Email = {Email}
      and Id <> isnull({Id}, 0)
)
    select 'Email', 'This email is already in use.'
```

Multiple errors:

```sql
-- Validation: required
IF @Nome IS NULL OR LTRIM(RTRIM(@Nome)) = ''
BEGIN
    SELECT 'Name', 'Name is required';
END;

-- Validation: minimum and maximum length
IF LEN(@Nome) < 2 OR LEN(@Nome) > 100
BEGIN
    SELECT 'Name must be between 2 and 100 characters'; 
END;

-- Validation: only letters and spaces (no numbers or special characters)
IF @Nome LIKE '%[^A-Za-zÀ-ÿ ]%'
BEGIN
    SELECT 'Name', 'Name contains invalid characters';
END;

-- Validation: avoid multiple consecutive spaces
IF @Nome LIKE '%  %'
BEGIN
    SELECT 'Name', 'Name cannot contain consecutive spaces';
END;

-- Validation: blacklist (business rule example)
IF UPPER(@Nome) IN ('ADMIN', 'ROOT', 'SYSTEM')
BEGIN
    SELECT 'Name', 'Name not allowed';
END;

-- Validation: avoid unrealistically short names
IF LEN(REPLACE(@Nome, ' ', '')) < 2
BEGIN
    SELECT 'Name', 'Invalid name';
END;

-- Specific validation (example business rule)
IF @Nome = 'Bola'
BEGIN
    SELECT 'Name', 'Name blocked by internal rule';
END;

```

## JavaScript rules

JavaScript rules run with [Jint](https://github.com/sebastienros/jint).
They are executed **server-side**, not in the browser.

The script receives:

- `values`: object containing form values
- `addError(message)`: adds a general error
- `addError(name, message)`: adds a field error

### JavaScript examples

General error:

```javascript
if (!values.Name && !values.CompanyName) {
    addError("Either Name or CompanyName must be filled.");
}
```

Field error:

```javascript
if (!values.Email) {
    addError("Email", "Email is required.");
}
```

Multiple errors:

```javascript
if (!values.StartDate) {
    addError("StartDate", "Start date is required.");
}

if (!values.EndDate) {
    addError("EndDate", "End date is required.");
}

if (values.StartDate && values.EndDate && values.StartDate > values.EndDate) {
    addError("EndDate", "End date must be greater than or equal to start date.");
}
```

Cross-field validation:

```javascript
if (values.Type === "Company" && !values.Document) {
    addError("Document", "Document is required for companies.");
}
```

## Choosing between SQL and JavaScript

Use `SQL` when:

- validation depends on database queries
- you want to reuse database-side logic
- you need to validate against existing persisted data

Use `JavaScript` when:

- validation depends only on current form values
- you want simpler cross-field logic
- you want a more expressive scripting syntax for conditional rules

## Notes

- Rule field names must match the dictionary field names when you add field errors
- General errors are shown as form-level validation messages
- Field errors are attached to the corresponding field when possible
