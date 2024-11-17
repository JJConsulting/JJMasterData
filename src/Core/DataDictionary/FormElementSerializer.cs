#nullable enable
using System;
using System.Text.Json;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataDictionary;

public static class FormElementSerializer
{
    public static string Serialize(FormElement formElement)
    {
        return JsonSerializer.Serialize(formElement);
    }

    public static FormElement Deserialize(string json)
    {
        return JsonSerializer.Deserialize<FormElement>(json)!;
    }
}