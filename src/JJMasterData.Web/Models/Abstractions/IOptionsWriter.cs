using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Models.Abstractions;

public interface IOptionsWriter<out T> : IOptionsSnapshot<T> where T : class, new()
{
    string FilePath { get; }
    Task UpdateAsync(Action<T> applyChanges);
}