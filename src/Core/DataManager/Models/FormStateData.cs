#nullable enable

using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Core.DataManager;

public class FormStateData
{
    public required IDictionary<string,dynamic>? UserValues { get; init; }

    public required IDictionary<string,dynamic> FormValues { get; init; }

    public required PageState PageState { get; init; }
    
    [SetsRequiredMembers]
    public FormStateData(
        IDictionary<string, dynamic> formValues, 
        IDictionary<string, dynamic>? userValues,
        PageState pageState)
    {
        UserValues = userValues.DeepCopy();
        FormValues = formValues;
        PageState = pageState;
    }

    [SetsRequiredMembers]
    public FormStateData(
        IDictionary<string, dynamic> formValues,
        PageState pageState)
    {
        FormValues = formValues;
        PageState = pageState;
    }

}