# Customize a write procedure

A category form must normalize its name in the database and return its generated key to JJMasterData. This recipe replaces the write SQL while retaining the generated form and the repository's insert/update/delete contract.

## Prepare the schema and metadata

This SQL Server example assumes:

```sql
CREATE TABLE dbo.Category
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);
```

Define `Category` with `Id` (`Real`, primary key, identity) and `Name` (`Real`, required, Unicode string, size 100). Set `UseWriteProcedure = true`, `WriteProcedureName = "CategoryWrite"` and `Schema = "dbo"`. A disabled identity editor remains a write parameter; its insertion value is ignored by the procedure.

## Implement the write contract

Start from the generated signature. For exactly the metadata above, a complete procedure is:

```sql
CREATE OR ALTER PROCEDURE dbo.CategoryWrite
    @action VARCHAR(1),
    @Id INT = NULL,
    @Name NVARCHAR(100) = NULL,
    @RET INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @operation VARCHAR(1) = @action;
    IF NULLIF(LTRIM(RTRIM(@operation)), '') IS NULL
    BEGIN
        SET @operation = CASE WHEN EXISTS
            (SELECT 1 FROM dbo.Category WHERE Id = @Id)
            THEN 'A' ELSE 'I' END;
    END;

    IF @operation IN ('I', 'A')
    BEGIN
        SET @Name = NULLIF(LTRIM(RTRIM(@Name)), N'');
        IF @Name IS NULL
            THROW 50001, 'Enter a category name.', 1;
    END;

    IF @operation = 'I'
    BEGIN
        INSERT dbo.Category (Name)
        OUTPUT INSERTED.Id AS Id, INSERTED.Name AS Name
        VALUES (@Name);
        SET @RET = 0;
    END
    ELSE IF @operation = 'A'
    BEGIN
        UPDATE dbo.Category SET Name = @Name WHERE Id = @Id;
        IF @@ROWCOUNT = 0
            THROW 50002, 'Category no longer exists.', 1;
        SET @RET = 1;
    END
    ELSE IF @operation = 'E'
    BEGIN
        DELETE dbo.Category WHERE Id = @Id;
        IF @@ROWCOUNT = 0
            THROW 50002, 'Category no longer exists.', 1;
        SET @RET = 2;
    END
    ELSE
        THROW 50003, 'Unsupported operation.', 1;
END;
```

The empty action is needed by repository upserts. The returned `Id` and `Name` row allows the repository to merge generated values after insertion. On an update this example does not return normalized fields; reload the record when you need its database-normalized value immediately.

The existence check chooses the operation; it does not provide a general concurrency guarantee. Enforce uniqueness through database constraints and use an appropriate transaction strategy if concurrent upserts must be serialized.

## Extend it with business rules

Keep friendly input checks in [form events](business-validation.md), and database invariants in constraints or the procedure when all writers must obey them. If saving a record also modifies relationships, put the writes into one transaction and return the generated-key result only after success. See [Persist multiple selections](multiple-selections.md).

`@RET = 1` means update even if a trigger changes more than one row. Never substitute `@@ROWCOUNT` for the operation code. Avoid diagnostic result sets because the repository reads the first returned row.

## Check the result

Insert a category and confirm that its generated key is available for subsequent navigation. Update and delete it, then exercise `SetValuesAsync` with both a new and an existing key. Test blank names and missing records; they must fail rather than report success. If imports are enabled, verify both new and existing rows.

Keep this script under version control and merge future generated signature changes into it. See [Stored procedures](../developer-guide/repositories/stored-procedures.md) for naming and deployment details.
