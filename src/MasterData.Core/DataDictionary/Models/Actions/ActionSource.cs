using System;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public enum ActionSource
{
    GridToolbar = 1,
    GridTable = 2,
    Field = 3,
    FormToolbar = 4
}

public static class ActionSourceExtensions
{
    extension(ActionSource source)
    {
        public string Name => source switch
        {
            ActionSource.GridToolbar => nameof(ActionSource.GridToolbar),
            ActionSource.GridTable => nameof(ActionSource.GridTable),
            ActionSource.Field => nameof(ActionSource.Field),
            ActionSource.FormToolbar => nameof(ActionSource.FormToolbar),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}