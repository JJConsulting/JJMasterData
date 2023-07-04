using System.Collections;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

internal record FormContext(IDictionary<string,dynamic>Values, IDictionary<string,dynamic>Errors, PageState PageState);