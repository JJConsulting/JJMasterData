#nullable enable
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.Services;

public record SearchBoxContext(IDictionary<string, dynamic>? Values,
    IDictionary<string, dynamic?>? UserValues, PageState PageState);