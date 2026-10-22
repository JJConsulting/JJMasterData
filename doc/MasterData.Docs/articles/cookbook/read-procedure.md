# Customize a read procedure

A work-order grid needs the customer's display name and a calculated outstanding amount. Neither belongs in the writable `WorkOrder` columns. A read procedure can supply both while the form continues to save the base fields.

## Prepare the dictionary

Use a fictional `WorkOrder` table with `Id`, `CustomerId`, `Amount` and `PaidAmount`, plus a `Customer` table with `Id` and `Name`.

| Dictionary field | Data behavior | Purpose |
| --- | --- | --- |
| `Id` | `Real`, primary key | Stable row identity and record lookup. |
| `CustomerId`, `Amount`, `PaidAmount` | `Real` | Writable base-table fields. |
| `CustomerName` | `ViewOnly` | Joined description. |
| `OutstandingAmount` | `ViewOnly` | Calculated decimal value. |

Match field types and decimal precision to the database. Set `UseReadProcedure = true` and `ReadProcedureName = "WorkOrderRead"`; set `Schema` to the schema containing your procedure. Keep write configuration independent.

## Customize the generated SQL

Generate the read procedure through **Database Scripts**. Keep its parameters and paging structure. In the generated `@sqlColumn` string, replace the column projection with this fragment (the fragment belongs inside the generated SQL string):

```sql
w.Id AS Id,
w.CustomerId AS CustomerId,
w.Amount AS Amount,
w.PaidAmount AS PaidAmount,
c.Name AS CustomerName,
w.Amount - w.PaidAmount AS OutstandingAmount
```

Replace the generated `@sqlTable` assignment with:

```sql
SET @sqlTable = N'FROM dbo.WorkOrder AS w
                 INNER JOIN dbo.Customer AS c ON c.Id = w.CustomerId';
```

These fragments assume `Schema = "dbo"`. Use your actual schema consistently. An inner join excludes orders without a matching customer; use a left join if those records must remain visible.

Qualify base-field predicates with `w` where names such as `Id` would otherwise be ambiguous. If `CustomerName` has a contains filter, implement its generated filter placeholder with:

```sql
IF @CustomerName IS NOT NULL
    SET @sqlWhere = @sqlWhere + N' AND c.Name LIKE N''%'' + @CustomerName + N''%''';
```

Keep `@CustomerName` in the generated `sp_executesql` parameter declarations and bindings for **both** the row query and the count query. Pass values as parameters. If an expression such as `OutstandingAmount` is filterable, use `w.Amount - w.PaidAmount` in the predicate; its select alias is unavailable in `WHERE`.

Joined column names and computed aliases also affect ordering. Map allowed dictionary field names to known SQL expressions, and retain a primary-key tie-breaker. Do not concatenate an unvalidated order string from a request into SQL.

The generated count and data queries share `@sqlTable` and `@sqlWhere`, so keep these variables consistent. If a join has multiple matches per order, aggregate it first or otherwise ensure one result row per key. Duplicates break row actions and pagination as well as counts.

## Check the result

1. Filter by a joined description and verify that the displayed total matches the full result.
2. Sort by the calculated amount and move between pages; tied values must not shuffle records.
3. Open an order by its key and update a base field. Joined fields must not appear as write parameters.
4. Export a filtered result spanning several pages. Confirm that the joined and calculated columns are present when configured for export.
5. Change a filter definition and compare the generated signature with the deployed procedure before releasing.

Use [grid rendering events](../developer-guide/customizations/grid-events.md) for a badge or color only. Supply business values in the read result when export and repository consumers also need them. See the complete [procedure contract](../developer-guide/repositories/stored-procedures.md).
