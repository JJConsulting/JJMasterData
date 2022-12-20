using System.IO;
using System.Threading;

namespace JJMasterData.Core.DataManager.Exports.Abstractions;

public interface IWriter
{
    void GenerateDocument(Stream stream, CancellationToken token);
}