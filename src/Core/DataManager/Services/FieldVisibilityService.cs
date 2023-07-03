using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class FieldVisibilityService : IFieldVisibilityService
{
    public IExpressionsService ExpressionsService { get; }

    public FieldVisibilityService(IExpressionsService expressionsService)
    {
        ExpressionsService = expressionsService;
    }
    
    [Obsolete("Move this to another action service?")]
    public bool IsVisible(BasicAction action, PageState state, IDictionary<string,dynamic>formValues)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action), "Action cannot be null");

        return ExpressionsService.GetBoolValue(action.VisibleExpression, action.Name, state, formValues);
    }

    public bool IsVisible(FormElementField field, PageState state, IDictionary<string,dynamic>formValues)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField cannot be null");

        return ExpressionsService.GetBoolValue(field.VisibleExpression, field.Name, state, formValues);
    }
    
    public bool IsEnabled(FormElementField field, PageState state, IDictionary<string,dynamic>formValues)
    {
        if (state == PageState.View)
            return false;

        if (field == null)
            throw new ArgumentNullException(nameof(field), "FormElementField cannot be null");

        return ExpressionsService.GetBoolValue(field.EnableExpression, field.Name, state, formValues);
    }
}