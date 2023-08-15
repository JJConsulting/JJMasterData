using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.Imports;

public record DataImportationContext(
    FormElement FormElement,
    DataContext DataContext,
    string RawData,
    char Separator);