using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Options.Abstractions;

public interface IWritableOptions<out T> : IOptionsSnapshot<T> where T : class, new()
{
    string FilePath { get; }
    Task UpdateAsync(Action<T> applyChanges);
}