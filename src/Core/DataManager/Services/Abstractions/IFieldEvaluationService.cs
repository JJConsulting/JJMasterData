using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public interface IFieldEvaluationService
{
    IExpressionsService ExpressionsService { get; }
    bool IsVisible(BasicAction action, PageState state, IDictionary<string,dynamic>formValues);
    bool IsVisible(FormElementField field, PageState state, IDictionary<string,dynamic>formValues);
    bool IsEnabled(FormElementField field, PageState state, IDictionary<string,dynamic>formValues);
}