using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataManager.Exportation;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface IExportFormat<in TOptions> where TOptions : ExportFormatOptions, new()
{
    Task WriteAsync(ExportContext context, TOptions options, Stream output, CancellationToken cancellationToken);
}
