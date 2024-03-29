# Data Item {SearchId} and {SearchText}
- SearchId is the current record primary key
- SearchText is the typed text by the user at a SearchBox, ignore it at ComboBox and Lookup

## {SearchId} is required at queries not using GridBehavior.Id.

If you work at JJConsulting you are probably very surprised seeing this error in a previously working ComboBox or SearchBox.
This error code was introduced at Mars Version (3.0).<br/>
The reason this validation was created is because using any GridBehavior except GridBehavior.Id causes the component to
execute the SQL query at **every row**, causing slowness on huge tables.
You have 3 options to solve this error.

- ### Adding {SearchId} and {SearchText} to your SQL query
Simply add a WHERE clause with your primary key comparing it.<br/>
In a nutshell:
```sql
EXEC SP_SEARCH_FOO '{SearchId}','{SearchText}'
```

Example procedure
```sql
CREATE PROCEDURE SP_SEARCH_FOO
    @SEARCHID VARCHAR(MAX),
    @SEARCHTEXT VARCHAR(MAX)
AS
BEGIN
    IF @SEARCHID <> ''
    BEGIN
        SELECT ID, DESCRI
        FROM FOO
        WHERE ID = @SEARCHID
    END
    ELSE IF @SEARCHTEXT <> ''
    BEGIN
        SELECT ID, DESCRI
        FROM FOO
        WHERE DESCRI LIKE '%' + @SEARCHTEXT + '%'
        END
    END
END
```

Remember that '{SearchId}' and '{SearchText}' always are replaced in SQL as parameters.<br>
SearchText = When user search any text<br>
SearchId = When the system need recover the description

- ### Use GridBehavior.Id
Simply use GridBehavior.Id returning the ID instead of the description or create a ViewOnly field, 
customizing your Get Procedure adding the rule to render your description.

- ### Use ElementMap
Map another element instead of manually writing your SQL query. We will automatically optimize everything for you.

