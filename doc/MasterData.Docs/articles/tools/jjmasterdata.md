# JJMasterData Command Line Tool

## Install

```bash
dotnet tool install --global JJMasterData.CommandLine
```

## Commands

### Interactive mode

Run `jjmasterdata` with no arguments to open the interactive menu. The menu exposes `Import`, `Export`, and `Diff`, and prompts for the dictionary path and the database connection string.

### Import

```bash
jjmasterdata import -p ./dictionaries -c "Server=localhost;Database=JJMasterData;Trusted_Connection=True"
```

### Export

```bash
jjmasterdata export -p ./dictionaries -c "Server=localhost;Database=JJMasterData;Trusted_Connection=True"
```

### Diff

```bash
jjmasterdata diff -p ./dictionaries -c "Server=localhost;Database=JJMasterData;Trusted_Connection=True"
```

## Notes

- The command name is `jjmasterdata`.
- Use `-p` or `--path` for the dictionary folder and `-c` or `--connection` for the database connection string.
