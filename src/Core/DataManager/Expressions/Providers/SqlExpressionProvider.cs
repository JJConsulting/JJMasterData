#nullable enable

using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Expressions.Providers;

internal class SqlExpressionProvider : IAsyncExpressionProvider
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
    
    public async Task<object?> EvaluateAsync(string expression, FormStateData formStateData)
    {
        var parsedSql = _expressionParser.ParseExpression(expression, formStateData);
        if (parsedSql == null)
            return null;
        var result = await _entityRepository.GetResultAsync(new DataAccessCommand(parsedSql));
        return result?.ToString() ?? null;

    }
}
