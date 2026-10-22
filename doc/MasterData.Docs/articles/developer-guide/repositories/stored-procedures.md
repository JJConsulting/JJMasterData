# Stored procedures

A dictionary can independently route reads and writes through stored procedures. This page describes the **SQL Server provider** contract. Other providers have their own implementations; do not reuse SQL Server parameter conventions without checking their support.

## Enable and name procedures

Configure these properties on the dictionary's `Element` / `FormElement`:

| Property | Purpose |
| --- | --- |
| `UseReadProcedure` | Execute the read procedure instead of generated SQL text. |
| `ReadProcedureName` | Override the read procedure's name. |
| `UseWriteProcedure` | Execute the write procedure instead of generated SQL text. |
| `WriteProcedureName` | Override the write procedure's name. |
| `Schema` | Schema used to qualify both procedure names; defaults to `dbo` when null. |
| `ConnectionId` | Connection used to execute the operation. |

Name overrides are **unqualified names**: use `OrderRead`, not `dbo.OrderRead`. The SQL Server provider adds `[Schema].[Name]` itself. Without an override, `JJMasterData:ReadProcedurePattern` and `JJMasterData:WriteProcedurePattern` replace `{tablename}` with `Element.TableName`. Their defaults are `{tablename}Get` and `{tablename}Set`.

For example, table `WorkOrder` in schema `app` resolves to `[app].[WorkOrderGet]` and `[app].[WorkOrderSet]`. Changing the dictionary's `Name` does not rename those procedures.

Use the editor's **Database Scripts** to generate a baseline, or obtain scripts through `IEntityRepository.GetReadProcedureScript(element)` and `GetWriteProcedureScript(element)`. Generation returns SQL; deploying it is a separate step. A metadata save does not create or alter the procedures.

## Read contract

| Parameter | Meaning |
| --- | --- |
| `@orderby` | Comma-separated field names and directions, such as `Name ASC,Id DESC`. |
| `@regporpag` | Records per page. |
| `@pag` | Page number, starting at **1**. |
| `@qtdtotal INT OUTPUT` | Total matching records before pagination; sent as input/output by the repository. |
| Field parameters | Primary keys and fields with a filter configured. |
| `@Field_from`, `@Field_to` | Parameters for a field with `FilterMode.Range`. |

Use the generated signature as the authority for types and sizes. A non-range field with `FilterMode.None` is still sent if it is a primary key. Changing filter metadata can change the parameter list even if no column was added to the table.

Return rows with column names matching the dictionary's fields, including primary keys. `ViewOnly` fields need an actual expression or joined column: the generator initially projects `NULL AS [Field]` for them and leaves customization markers for their filters.

The generated procedure counts when `@qtdtotal` is null or zero. A value of `-1` asks it to skip counting. Keep this convention when customizing it. Compute the count using the same joins and predicates as the data query, without its page limit. Use a stable order with a unique tie-breaker and calculate the SQL Server offset as `(@pag - 1) * @regporpag`.

The read procedure is also used outside grid rendering, including record lookup and repository-based export. Support primary-key filters, all requested pages and requested ordering. Avoid returning an extra diagnostic `SELECT` before the rows.

See [Customize a read procedure](../../cookbook/read-procedure.md) for a joined projection and its acceptance checks.

## Write contract

| Parameter | Meaning |
| --- | --- |
| `@action VARCHAR(1)` | `I` inserts, `A` updates, `E` deletes; an empty action lets the procedure choose insert/update. |
| Field parameters | Fields whose `DataBehavior` is `Real` or `WriteOnly`, including identity fields. |
| `@RET INT OUTPUT` | `0` for insert, `1` for update, `2` for delete (`CommandOperation`). |

The generated procedure resolves an empty action by checking whether the primary key exists. Preserve that behavior if callers use `SetValuesAsync`, including imports. Preserve all parts of a composite key in updates and deletes.

`@RET` describes the operation, **not** the number of rows changed. It is an output parameter, not the T-SQL `RETURN` value. Throw a database error on failure rather than using a custom error number in `@RET`.

On insert, return generated keys as a result row with aliases matching the dictionary fields, for example `OUTPUT INSERTED.Id AS Id`. The repository merges returned known fields into the values dictionary. Do not place another result set before this row. See [Customize a write procedure](../../cookbook/write-procedure.md).

A write procedure can update related tables, but it must explicitly manage the transaction needed to keep those changes atomic. An after-event runs after repository persistence and does not extend the procedure's transaction.

## Deploy customized scripts

Keep customized SQL in your application's database migrations or database project. Regenerating and deploying the baseline procedure can overwrite custom joins, validations and writes. When fields or filters change, compare the new generated signature with the maintained script and update both together.

Verify insert, update, delete, empty-action upsert, lookup by key, paging, ordering, count and export as applicable. Test import separately because it uses its own validation hooks. The [entity repository](entities.md) uses these procedures but direct writes do not run form validation or audit processing.
