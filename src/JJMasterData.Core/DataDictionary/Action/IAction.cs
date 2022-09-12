namespace JJMasterData.Core.DataDictionary.Action;

public interface IAction
{
    string Name { get; set; }

    string Text { get; set; }

    string ToolTip { get; set; }

    bool IsDefaultOption { get; set; }

    bool IsGroup { get; set; }

    bool DividerLine { get; set; }

    bool ShowAsButton { get; set; }

    string CssClass { get; set; }

}