using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Importation;

public record DataImportationContext(
    FormElement FormElement,
    DataContext DataContext,
    string RawData,
    char Separator);