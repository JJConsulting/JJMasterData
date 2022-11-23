using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace JJMasterData.Swagger.AspNetCore;

public class DataDictionaryDocumentFilter : IDocumentFilter
{
    private readonly IDictionaryRepository _dictionaryRepository;

    public DataDictionaryDocumentFilter(IDictionaryRepository dictionaryRepository)
    {
        _dictionaryRepository = dictionaryRepository;
    }

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        document.Info.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        var dictionaries = _dictionaryRepository.GetMetadataList(true);

        foreach (var metadata in dictionaries)
        {
            FormElement formElement = metadata.GetFormElement();

            DataDictionaryPathItem defaultPathItem = new($"/MasterApi/{formElement.Name}");
            DataDictionaryPathItem detailPathItem = new($"/MasterApi/{formElement.Name}/{{id}}");
            DataDictionaryOperationFactory factory = new(formElement, metadata.Api);

            if (metadata.Api.EnableGetAll)
                defaultPathItem.AddOperation(OperationType.Get, factory.GetAll());

            if (metadata.Api.EnableGetDetail)
                detailPathItem.AddOperation(OperationType.Get, factory.Get());

            if (metadata.Api.EnableAdd)
                defaultPathItem.AddOperation(OperationType.Post, factory.Post());

            if (metadata.Api.EnableUpdate)
                defaultPathItem.AddOperation(OperationType.Put, factory.Put());

            if (metadata.Api.EnableUpdatePart)
                defaultPathItem.AddOperation(OperationType.Patch, factory.Patch());

            if (metadata.Api.EnableDel)
                detailPathItem.AddOperation(OperationType.Delete, factory.Delete());

            document.Paths.AddDataDictionaryPath(defaultPathItem);
            document.Paths.AddDataDictionaryPath(detailPathItem);
        }
    }
}