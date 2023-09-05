namespace JJMasterData.Core.Web.Http.Abstractions;

public interface IQueryString
{
    string this[string key] { get; }
    public string Value { get; }
}