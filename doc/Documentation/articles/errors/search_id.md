# {SearchId} is required at queries using ReplaceTextOnGrid.

If you work at JJConsulting you are probably very surprised seeing this error in a previously working ComboBox or SearchBox.
This error code was introduced at Mars Version (3.0).<br/>
The reason this validation was created is because ReplaceTextOnGrid causes the component to
execute the SQL query at **every row**, causing slowness on huge tables.
You have 2 options to solve this error.

## Add {SearchId} to your SQL query
Simply add a WHERE clause with your primary key comparing it with {SearchId}.<br/>
In a nutshell:
```sql
IF '{SearchId}' <> ''  --This condition is fulfilled at the grid
    SELECT ID, DESCRIPTION
    FROM TABLE
    WHERE
    ID = '{SearchId}'
ELSE --This condition is fulfilled at the form.
    SELECT ID,DESCRIPTION
    FROM TABLE
```
You can also:
```sql
EXEC SP_GETALL_FOO '{SearchId}' --Fulfill your conditions at the procedure
```

## Disable ReplaceTextOnGrid
Simply disable ReplaceTextOnGrid returning the ID instead of the description or create a ViewOnly field, 
customizing your Get Procedure adding the rule to render your description.

## Use ElementMap
Map another element instead of manually writing your SQL query. We will automatically optimize everything for you.