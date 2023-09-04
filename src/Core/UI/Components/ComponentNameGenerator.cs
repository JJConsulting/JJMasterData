#nullable  enable

using JJMasterData.Commons.Hashing;

namespace JJMasterData.Core.UI.Components;

public static class ComponentNameGenerator
{
    public static string Create(string elementName)
    {
        return GuidGenerator.FromValue(elementName).ToString();
    }
}