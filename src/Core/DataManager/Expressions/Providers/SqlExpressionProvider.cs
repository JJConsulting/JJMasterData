using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

public class SqlExpressionProvider : IExpressionProvider
{
    private readonly IEntityRepository _entityRepository;
    private readonly ExpressionParser _expressionParser;

    public SqlExpressionProvider(IEntityRepository entityRepository, ExpressionParser expressionParser)
    {
        _entityRepository = entityRepository;
        _expressionParser = expressionParser;
    }

    public string Prefix => "sql";
    public string Title => "T-SQL";
    
    public async Task<object> EvaluateAsync(string expression, FormStateData formStateData)
    {
        var parsedSql = _expressionParser.ParseExpression(expression, formStateData, false);
        var obj = await _entityRepository.GetResultAsync(new DataAccessCommand(parsedSql!));
        return obj?.ToString() ?? string.Empty;
    }
}
