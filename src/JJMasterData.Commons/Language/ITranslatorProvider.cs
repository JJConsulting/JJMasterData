using System.Collections.Generic;

namespace JJMasterData.Commons.Language;

public interface ITranslatorProvider
{
    /// <summary>
    /// Recovers the resources using a Dictionary 
    /// <para>Ex.: Object = Objeto</para>
    /// </summary>
    /// <param name="culture">
    /// Name of the culture (ex. pt-BR)
    /// </param>
    /// <returns>
    /// A dictionary with the translated strings
    /// </returns>
    IDictionary<string, string> GetLocalizedStrings(string culture);
}