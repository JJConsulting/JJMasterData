using System.Collections.Generic;

namespace JJMasterData.Commons.Language;

public interface ITranslator
{
    /// <summary>
    /// Recovers the resources using a Dictionary 
    /// <para>Ex.: Object = Objeto</para>
    /// </summary>
    /// <param name="culture">
    /// Name of the culture (ex. pt-br)
    /// </param>
    /// <returns>
    /// A dictionary with the translated strings
    /// </returns>
    Dictionary<string, string> GetDictionaryStrings(string culture);
}