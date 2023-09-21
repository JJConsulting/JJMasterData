namespace JJMasterData.Web.Areas.MasterData.Models;

public class LookupViewModel
{
    public required string EncryptedLookupParameters { get; set; } = null!;
    public required string LookupFormViewHtml { get; set; } = null!;
}