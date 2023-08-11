using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface IWriter
{
    Task GenerateDocument(Stream stream, CancellationToken token);
}