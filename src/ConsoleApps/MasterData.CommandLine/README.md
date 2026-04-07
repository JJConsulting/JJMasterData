# JJMasterData Command Line

`MasterData.CommandLine` is the dotnet tool that exposes the `jjmasterdata` command.

## Install

```bash
dotnet tool install --global MasterData.CommandLine
```

If you are testing a local package feed, install it from that source instead:

```bash
dotnet tool install --global MasterData.CommandLine --add-source <path-to-nupkg-folder>
```

## Usage

Run the tool directly to open the interactive menu:

```bash
jjmasterdata
```

The interactive mode exposes `Import`, `Export`, and `Diff`, and prompts for the dictionary path and the database connection string.

### Import

```bash
jjmasterdata import --path ./dictionaries --connection "Server=localhost;Database=JJMasterData;Trusted_Connection=True"
```

Short options are also available:

```bash
jjmasterdata import -p ./dictionaries -c "Server=localhost;Database=JJMasterData;Trusted_Connection=True"
```

### Export

Export the current data dictionaries from the database to a folder:

```bash
jjmasterdata export --path ./dictionaries --connection "Server=localhost;Database=JJMasterData;Trusted_Connection=True"
```

### Diff

Compare the dictionaries on disk with the database state:

```bash
jjmasterdata diff --path ./dictionaries --connection "Server=localhost;Database=JJMasterData;Trusted_Connection=True"
```

## Notes

- The tool uses the `jjmasterdata` command name.
- The import, export, and diff commands all require the dictionary path and the connection string.
