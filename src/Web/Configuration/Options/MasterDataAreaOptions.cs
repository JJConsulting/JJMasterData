namespace JJMasterData.Web.Configuration.Options;

public class MasterDataAreaOptions
{
    /// <summary>
    /// The first route path before the Areas from the JJMasterData.Web Razor Class Library. If <see cref="EnableCultureProvider"/> is set to true, will be added after {culture}.
    /// </summary>
    public string? Prefix { get; set; } 
    
    /// <summary>
    /// If true, will add before the first route path, a {culture} route value used for l10n.
    /// </summary>
    public bool EnableCultureProvider { get; set; } = true;
}