#nullable enable
namespace JJMasterData.Core.DataManager.Services;

public class ExpressionManagerInterval
{
    public char Begin { get; set; }
    public char End { get; set; }

    public ExpressionManagerInterval(char begin, char end)
    {
        Begin = begin;
        End = end;
    }
}