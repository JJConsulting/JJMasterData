namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Filter Type in Grid
/// </summary>
public enum FilterMode
{
    None = 1,
    Equal = 2,
    Contain = 3,
    Range = 4,
    
    MultValuesContain = 5,
    MultValuesEqual = 6
}

public static class FilterModeExtensions
{
    extension(FilterMode mode)
    {
        public bool IsMultiValues => mode is FilterMode.MultValuesContain or FilterMode.MultValuesEqual;
    }
}