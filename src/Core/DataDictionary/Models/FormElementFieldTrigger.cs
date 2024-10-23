using System;

namespace JJMasterData.Core.DataDictionary.Models;

public enum FormElementFieldTrigger
{
    OnChange,
    OnKeyUp
}

public static class FormElementFieldTriggerExtensions
{
    public static string GetEvent(this FormElementFieldTrigger trigger)
    {
        return trigger switch
        {
            FormElementFieldTrigger.OnChange => "onchange",
            FormElementFieldTrigger.OnKeyUp => "onkeyup",
            _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null)
        };
    }
}