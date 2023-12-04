using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public interface IExportationWriter
{
    Task GenerateDocument(Stream stream, CancellationToken token);
}