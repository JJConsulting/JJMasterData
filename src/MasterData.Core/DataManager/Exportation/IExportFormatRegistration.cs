using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Exportation;

internal interface IExportFormatRegistration
{
    ExportFormatConfiguration Configuration { get; }
    void ValidateOptions(Dictionary<string, string?> options);
    Task WriteAsync(
        ExportContext context,
        Dictionary<string, string?> options,
        Stream output,
        CancellationToken cancellationToken);
}
