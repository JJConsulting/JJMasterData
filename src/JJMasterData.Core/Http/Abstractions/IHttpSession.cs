namespace JJMasterData.Core.Http.Abstractions;

public interface IHttpSession
{
    string this[string key] { get; set; }
    void SetSessionValue(string key, object value);
    T GetSessionValue<T>(string key);
}