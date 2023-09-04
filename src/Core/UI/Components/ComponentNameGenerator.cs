#nullable  enable

using JJMasterData.Commons.Util;

namespace JJMasterData.Core.UI.Components;

public static class ComponentNameGenerator
{
    public static string Create(string elementName)
    {
        return StringManager.ToParamCase(elementName);
    }
}