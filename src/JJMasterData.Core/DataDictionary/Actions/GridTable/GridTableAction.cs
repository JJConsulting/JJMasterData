using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Core.DataDictionary.Actions.GridTable;

public abstract class GridTableAction : BasicAction
{
    public override bool IsUserCreated => false;
}