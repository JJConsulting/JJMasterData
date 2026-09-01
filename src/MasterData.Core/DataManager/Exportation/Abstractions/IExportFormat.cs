using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface IExportFormat<in TOptions> where TOptions : class, new()
{
    ExportFormatConfiguration Configuration { get; }
    Task WriteAsync(ExportContext context, TOptions options, Stream output, CancellationToken cancellationToken);
}