namespace JJMasterData.Core.DataDictionary.Models.Actions;

internal class InsertSelectionAction : GridTableAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    internal const string ActionName = "insert-selection";

    internal InsertSelectionAction()
    {
        Name = ActionName;
        IsDefaultOption = true;
        Icon = IconType.CaretRight;
        Order = -1;
    }
}