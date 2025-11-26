using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

internal sealed class InsertSelectionAction : GridTableAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    internal const string ActionName = "insert-selection";

    internal InsertSelectionAction()
    {
        Name = ActionName;
        IsDefaultOption = true;
        Icon = FontAwesomeIcon.CaretRight;
        Order = -1;
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}