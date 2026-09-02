using System.Collections.Generic;
using System.IO;
using System.Threading;
using JJMasterData.Core.DataManager.Importation;

namespace JJMasterData.Core.DataManager.Importation.Abstractions;

public interface IImportReader<in TOptions> where TOptions : class, new()
{
    ImportFormatDefinition Definition { get; }
    IAsyncEnumerable<ImportRecord> ReadAsync(
        ImportContext context,
        TOptions options,
        Stream input,
        CancellationToken cancellationToken);
}
