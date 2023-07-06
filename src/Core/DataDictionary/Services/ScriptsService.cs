using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class ScriptsService
{
    private readonly IEntityRepository _entityRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;

    public ScriptsService(IEntityRepository entityRepository, IDataDictionaryRepository _dataDictionaryRepository)
    {
        _entityRepository = entityRepository;
        this._dataDictionaryRepository = _dataDictionaryRepository;
    }
    public async Task<List<string>> GetScriptsListAsync(string id)
    {
        var formElement = await _dataDictionaryRepository.GetMetadataAsync(id);
        Element element = formElement;

        var addedFields = await GetAddedFieldsAsync(element).ToListAsync();
        
        var listScripts = new List<string?>
        {
            _entityRepository.GetScriptCreateTable(element),
            _entityRepository.GetScriptReadProcedure(element),
            _entityRepository.GetScriptWriteProcedure(element),
            _entityRepository.GetAlterTableScript(element, addedFields),
        };

        return listScripts;
    }
    
    public async IAsyncEnumerable<ElementField> GetAddedFieldsAsync(Element element)
    {
        if (!await _entityRepository.TableExistsAsync(element.TableName))
            yield break;
        
        foreach (var field in element.Fields.Where(f => f.DataBehavior == FieldBehavior.Real))
        {
            if (!await _entityRepository.ColumnExistsAsync(element.TableName, field.Name))
            {
                yield return field;
            }
        }
    }
    
    public async Task ExecuteScriptsAsync(string id, string scriptOption)
    {
        var formElement = await _dataDictionaryRepository.GetMetadataAsync(id);

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
                var addedFields = await GetAddedFieldsAsync(formElement).ToListAsync();
                await _entityRepository.ExecuteBatchAsync(_entityRepository.GetAlterTableScript(formElement,addedFields));
                break;
        }
    }
}