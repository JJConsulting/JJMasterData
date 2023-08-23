#nullable enable
namespace JJMasterData.Core.DataManager.Services;

public class ExpressionParserInterval
{
    public char Begin { get; set; }
    public char End { get; set; }

    public ExpressionParserInterval(char begin, char end)
    {
        Begin = begin;
        End = end;
    }
}