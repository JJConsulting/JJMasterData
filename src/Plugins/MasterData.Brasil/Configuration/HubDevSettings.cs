namespace JJMasterData.Brasil.Configuration;

public class HubDevSettings
{
    public string Url { get; set; } = "ws.hubdodesenvolvedor.com.br/v2/";
    public required string ApiKey { get; set; }
}