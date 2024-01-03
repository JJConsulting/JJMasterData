using NCalc;

namespace JJMasterData.NCalc.Configuration;

public class NCalcExpressionProviderOptions
{
    /// <summary>
    /// Expressions starting with exp: will be executed by NCalc
    /// </summary>
    public bool ReplaceDefaultExpressionProvider { get; set; } = false;

    /// <summary>
    /// EvaluateOptions [Flags] enum. Check NCalc wiki for more information
    /// </summary>
    public EvaluateOptions EvaluateOptions { get; set; } = EvaluateOptions.IgnoreCase | EvaluateOptions.CaseInsensitiveComparer;

    /// <summary>
    /// Additional functions to be used at expressions. Check NCalc wiki for more information
    /// </summary>
    public List<EvaluateFunctionHandler> AdditionalFunctions { get; set; } = [];
}