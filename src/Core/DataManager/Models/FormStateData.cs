﻿#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Models;

public class FormStateData
{
    public required Dictionary<string, object?> Values { get; init; }
    public required Dictionary<string, object?>? UserValues { get; init; }
    public required PageState PageState { get; init; }

    public FormStateData()
    {
        
    }
    
    [SetsRequiredMembers]
    public FormStateData(
        Dictionary<string, object?> values, 
        Dictionary<string, object?>? userValues,
        PageState pageState)
    {
        UserValues = ObjectCloner.DeepCopy(userValues);
        Values = values;
        PageState = pageState;
    }

    [SetsRequiredMembers]
    public FormStateData(
        Dictionary<string, object?> values,
        PageState pageState)
    {
        Values = values;
        PageState = pageState;
    }

    public void Deconstruct(
        out Dictionary<string, object?> values,
        out Dictionary<string, object?>? userValues,
        out PageState pageState)
    {
        values = Values;
        userValues = UserValues;
        pageState = PageState;
    }
}