using NCalc;
using NCalc.Handlers;

namespace JJMasterData.NCalc.Configuration;

public class NCalcExpressionProviderOptions
{
    /// <summary>
    /// Expressions starting with exp: will be executed by NCalc
    /// </summary>
    public bool ReplaceDefaultExpressionProvider { get; set; } = false;

    /// <summary>
    /// Context of the expression. Declare here custom parameters and functions.
    /// </summary>
    public ExpressionContext Context { get; set; } = new();
}