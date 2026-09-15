using System;
using System.Globalization;

namespace JJMasterData.Commons.Background;

public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _previousUiCulture = CultureInfo.CurrentUICulture;

    private CultureScope(string cultureName, string uiCultureName)
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(uiCultureName);
    }

    public static CultureScope Create(string cultureName, string uiCultureName) =>
        new(cultureName, uiCultureName);

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _previousCulture;
        CultureInfo.CurrentUICulture = _previousUiCulture;
    }
}
