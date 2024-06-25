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
    /// ExpressionOptions [Flags] enum. Check NCalc wiki for more information
    /// </summary>
    public ExpressionOptions ExpressionOptions { get; set; } = ExpressionOptions.IgnoreCase | ExpressionOptions.CaseInsensitiveStringComparer;

    /// <summary>
    /// Additional functions to be used at expressions. Check NCalc wiki for more information
    /// </summary>
    public List<EvaluateFunctionHandler> AdditionalFunctions { get; set; } = [];
}