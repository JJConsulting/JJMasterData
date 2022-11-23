using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Models.Abstractions;

public interface IOptionsWriter<out T> : IOptionsSnapshot<T> where T : class, new()
{
    Task UpdateAsync(Action<T> applyChanges);
}