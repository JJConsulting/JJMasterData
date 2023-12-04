#nullable enable


namespace JJMasterData.Core.UI.Components;

public static class ComponentNameGenerator
{
    public static string Create(string elementName)
    {
        return elementName.ToLower();
    }
}