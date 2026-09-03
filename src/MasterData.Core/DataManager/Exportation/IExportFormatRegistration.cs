using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Exportation;

internal interface IExportFormatRegistration
{
    ExportFormatMetadata Metadata { get; }
    Task WriteAsync(
        ExportContext context,
        Dictionary<string, string?> options,
        Stream output,
        CancellationToken cancellationToken);
}
