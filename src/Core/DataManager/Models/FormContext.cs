using System.Collections;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

internal record FormContext(IDictionary<string, object> Values, IDictionary<string, object>? Errors, PageState PageState);