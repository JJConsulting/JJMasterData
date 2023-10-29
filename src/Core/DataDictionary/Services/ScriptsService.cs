using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ScriptsService
{
    private readonly IEntityRepository _entityRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;

    public ScriptsService(IEntityRepository entityRepository, IDataDictionaryRepository dataDictionaryRepository)
    {
        _entityRepository = entityRepository;
        _dataDictionaryRepository = dataDictionaryRepository;
    }
    public async Task<List<string>> GetScriptsListAsync(FormElement formElement)
    {
        var listScripts = new List<string>
        {
            _entityRepository.GetScriptCreateTable(formElement),
            formElement.UseReadProcedure ? _entityRepository.GetScriptReadProcedure(formElement) : null,
            formElement.UseWriteProcedure ? _entityRepository.GetScriptWriteProcedure(formElement) : null,
            await _entityRepository.GetAlterTableScriptAsync(formElement),
        };

        return listScripts;
    }
    
    public async Task ExecuteScriptsAsync(string id, string scriptOption)
    {
        var formElement = await _dataDictionaryRepository.GetFormElementAsync(id);

        switch (scriptOption)
        {
            case "ExecuteProcedures":
                var sql = new StringBuilder();
                sql.AppendLine(_entityRepository.GetScriptWriteProcedure(formElement));
                sql.AppendLine(_entityRepository.GetScriptReadProcedure(formElement));
                await _entityRepository.ExecuteBatchAsync(sql.ToString());
                break;
            case "ExecuteCreateDataModel":
                await _entityRepository.CreateDataModelAsync(formElement);
                break;
            case "ExecuteAlterTable":
                var alterTableScript = await _entityRepository.GetAlterTableScriptAsync(formElement);
                await _entityRepository.ExecuteBatchAsync(alterTableScript);
                break;
        }
    }
}