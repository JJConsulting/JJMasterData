# JJMasterData Command Line Tool

## Install

```bash
dotnet tool install --global JJMasterData.CommandLine
```

## Why use it?
This command line tool is useful for importing and exporting dictionaries to and from a database. 
It's useful on:
- CI/CD pipelines
- Backups
- Checking diff between dictionaries on different environments

## Commands

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

## Help

If you need help, use the command:
```bash
jjmasterdata --help
```

You can also use an interactive mode that contains all options: 
```bash
jjmasterdata
```
