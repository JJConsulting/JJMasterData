using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Importation;

public record DataImportationContext(
    FormElement FormElement,
    DataContext DataContext,
    IDictionary<string,object> RelationValues,
    string RawData,
    char Separator);