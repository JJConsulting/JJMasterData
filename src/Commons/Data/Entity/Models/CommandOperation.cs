﻿namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Command executed on the database
/// </summary>
public enum CommandOperation
{
    None = -1,
    Insert = 0,
    Update = 1,
    Delete = 2
}