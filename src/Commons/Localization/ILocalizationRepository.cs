using System.Collections.Generic;

namespace JJMasterData.Commons.Localization;

public interface ILocalizationRepository
{
    public Dictionary<string,string> GetAllStrings(string culture);
}