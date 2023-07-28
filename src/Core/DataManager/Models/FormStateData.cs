using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JJMasterData.Commons.Data.Entity.Abstractions;

namespace JJMasterData.Core.DataManager;


public class FormStateData
{
    public required IDictionary<string,dynamic> UserValues { get; init; }

    public required IDictionary<string,dynamic> FormValues { get; init; }

    public required PageState PageState { get; init; }
    
    [SetsRequiredMembers]
    public FormStateData(IDictionary<string,dynamic> userValues, IDictionary<string,dynamic>formValues, PageState pageState)
    {
        UserValues = userValues.DeepCopy();
        FormValues = formValues;
        PageState = pageState;
    }
}