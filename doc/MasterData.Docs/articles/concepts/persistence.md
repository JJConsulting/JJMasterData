# Persistence and version control

`IDataDictionaryRepository` stores `FormElement` metadata. The default SQL implementation uses `tb_masterdata`; the filesystem implementation stores one `<Name>.json` file per dictionary.

To use versioned files directly:

```csharp
using JJMasterData.Core.Configuration;
using JJMasterData.Web.Configuration;

builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithFileSystemDataDictionary(options =>
        options.FolderPath = Path.Combine(builder.Environment.ContentRootPath, "Dictionaries"));
```

The editor writes into this folder, so it needs write access when administrators save changes. Startup seeding creates the folder if missing. Business records still use the entity connection.

For a database-backed deployment, export JSON, commit and review it, then import it into the target metadata database using the CLI below. Review database schema scripts separately: dictionary import is not a database migration.

## Command-line tool

Install the tool with:

```bash
dotnet tool install --global JJMasterData.CommandLine
```

Use it to automate dictionary deployment in CI/CD pipelines, back up dictionary data, or compare environments.

Most commands require:

| Option | Purpose |
| --- | --- |
| `--path` (`-p`) | Directory containing dictionary files. |
| `--connection` (`-c`) | Database connection string. |
| `--table` (`-t`) | Data dictionary table in the format `<schema>.<table>`. |

The default table is `dbo.MasterData`.

### Import, export and diff

```bash
jjmasterdata import --path ./dictionaries --connection "<connection_string>" --table "dbo.MasterData"
jjmasterdata export --path ./dictionaries --connection "<connection_string>" --table "dbo.MasterData"
jjmasterdata diff --path ./dictionaries --connection "<connection_string>" --table "dbo.MasterData"
```

Avoid hardcoding connection strings in CI/CD. Pass them as environment variables instead:

```bash
jjmasterdata import --path ./dictionaries --connection "$DB_CONNECTION" --table "$MASTERDATA_TABLE"
```

Use `jjmasterdata --help` to display all options, or run `jjmasterdata` for interactive mode.
