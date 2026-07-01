# JJMasterData Command Line Tool

## Installation

```bash
dotnet tool install --global JJMasterData.CommandLine
```

---

## Why use it

Typical use cases include:

- Automating dictionary deployment in CI/CD pipelines
- Creating backups of dictionary data
- Comparing environments (for example, staging vs production)

---

## Core Concepts

Most commands require three parameters:

- `--path` (`-p`): Directory containing dictionary files
- `--connection` (`-c`): Database connection string
- `--table` (`-t`): Data dictionary table in the format `<schema>.<table>`

You can use either the short or long form:

```text
-p | --path
-c | --connection
-t | --table
```

Example:

```bash
--table dbo.MasterData
```

If omitted, the default table is:

```text
dbo.MasterData
```

---

## Commands

### Import

Imports dictionaries from files into the database.

```bash
jjmasterdata import \
  --path ./dictionaries \
  --connection "<connection_string>" \
  --table "dbo.MasterData"
```

---

### Export

Exports dictionaries from the database into files.

```bash
jjmasterdata export \
  --path ./dictionaries \
  --connection "<connection_string>" \
  --table "dbo.MasterData"
```

---

### Diff

Compares local dictionary files with the database.

```bash
jjmasterdata diff \
  --path ./dictionaries \
  --connection "<connection_string>" \
  --table "dbo.MasterData"
```

---

## CI/CD Usage

Avoid hardcoding connection strings. Instead, use environment variables provided by your CI system.

Example:

```bash
jjmasterdata import \
  --path ./dictionaries \
  --connection "$DB_CONNECTION" \
  --table "$MASTERDATA_TABLE"
```

Where:

- `DB_CONNECTION` is your database connection string.
- `MASTERDATA_TABLE` contains the data dictionary table name (for example, `dbo.MasterData`).

These variables can be defined in your CI pipeline (GitHub Actions, Azure DevOps, GitLab CI, etc.).

---

## Help and Interactive Mode

Display all available options:

```bash
jjmasterdata --help
```

Run in interactive mode:

```bash
jjmasterdata
```

This mode guides you through the available commands and required parameters.

---

## Special Thanks

Special thanks to [Spectre.Console](https://spectreconsole.net) for providing a robust and elegant foundation for building the command-line interface.