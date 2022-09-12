using System.Globalization;

namespace JJMasterData.Commons.Resources;

public static class JJMasterDataResources
{
    public static CultureInfo[] SupportedCultures => new[]
    {
        new CultureInfo("pt-BR"),
        new CultureInfo("en-US")
    };
}