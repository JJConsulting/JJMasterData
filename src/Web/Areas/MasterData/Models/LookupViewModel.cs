namespace JJMasterData.Web.Areas.MasterData.Models;

public sealed class LookupViewModel
{
    public required string EncryptedLookupParameters { get; set; } = null!;
    public required string LookupFormViewHtml { get; set; } = null!;
}