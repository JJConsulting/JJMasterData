# JJMasterData Command Line Tool

## Installation

```bash
dotnet tool install --global JJMasterData.CommandLine
```

---

## Why use it
Typical use cases include:

* Automating dictionary deployment in CI/CD pipelines
* Creating backups of dictionary data
* Comparing environments (for example, staging vs production)

---

## Core Concepts

Most commands require two parameters:

* `--path` (`-p`): Directory containing dictionary files
* `--connection` (`-c`): Database connection string

You can use either the short or long form:

```bash
-p | --path
-c | --connection
```

---

## Commands

### Import

Imports dictionaries from files into the database.

```bash
jjmasterdata import --path ./dictionaries --connection "<connection_string>"
```

---

### Export

Exports dictionaries from the database into files.

```bash
jjmasterdata export --path ./dictionaries --connection "<connection_string>"
```

---

### Diff

Compares local dictionary files with the database.

```bash
jjmasterdata diff --path ./dictionaries --connection "<connection_string>"
```

---

## CI/CD Usage

Avoid hardcoding connection strings. Instead, use environment variables provided by your CI system.

Example:

```bash
jjmasterdata import \
  --path ./dictionaries \
  --connection "$DB_CONNECTION"
```

Where `DB_CONNECTION` is defined in your CI pipeline (e.g., GitHub Actions, Azure DevOps, GitLab CI).

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

This mode guides you through available commands and parameters.

## Special Thanks

Special thanks to [Spectre.Console](https://spectreconsole.net) for providing a robust and elegant foundation for building our tool.