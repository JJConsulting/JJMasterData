using JJMasterData.Core.DataDictionary;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Swagger.AspNetCore;

public class DataDictionaryDocumentFilter : IDocumentFilter
{
    private readonly IDataDictionaryRepository _dataDictionaryRepository;

    public DataDictionaryDocumentFilter(IDataDictionaryRepository dataDictionaryRepository)
    {
        _dataDictionaryRepository = dataDictionaryRepository;
    }
    
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        document.Info.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        var dictionaries = _dataDictionaryRepository.GetMetadataList(true);

        foreach (var formElement in dictionaries)
        {

            var defaultPathItem = new DataDictionaryPathItem($"/MasterApi/{formElement.Name}");
            var detailPathItem = new DataDictionaryPathItem($"{defaultPathItem.Key}/{{id}}");
            var factory = new DataDictionaryOperationFactory(formElement, formElement.ApiOptions);

            if (formElement.ApiOptions.EnableGetAll)
                defaultPathItem.AddOperation(OperationType.Get, factory.GetAll());

            if (formElement.ApiOptions.EnableGetDetail)
                detailPathItem.AddOperation(OperationType.Get, factory.Get());
            
            if (formElement.ApiOptions.EnableAdd)
                defaultPathItem.AddOperation(OperationType.Post, factory.Post());

            if (formElement.ApiOptions.EnableUpdate)
                defaultPathItem.AddOperation(OperationType.Put, factory.Put());

            if (formElement.ApiOptions.EnableUpdatePart)
                defaultPathItem.AddOperation(OperationType.Patch, factory.Patch());

            if (formElement.ApiOptions.EnableDel)
                detailPathItem.AddOperation(OperationType.Delete, factory.Delete());

            document.Paths.AddDataDictionaryPath(defaultPathItem);
            document.Paths.AddDataDictionaryPath(detailPathItem);
            
            foreach (var field in formElement.Fields)
            {
                if (field.Component != FormComponent.File || field.DataFile == null) 
                    continue;
                
                var filePathItem = new DataDictionaryPathItem($"/MasterApi/{formElement.Name}/{{id}}/{field.Name}/file");
                var fileDetailPathItem = new DataDictionaryPathItem($"{filePathItem.Key}/{{fileName}}");
                
                if (formElement.ApiOptions.EnableGetDetail)
                    fileDetailPathItem.AddOperation(OperationType.Get, factory.GetFile(field));
                    
                if (formElement.ApiOptions is { EnableAdd: true, EnableUpdate: true })
                    filePathItem.AddOperation(OperationType.Post, factory.PostFile(field));
                        
                if (formElement.ApiOptions.EnableUpdatePart)
                    fileDetailPathItem.AddOperation(OperationType.Patch, factory.RenameFile(field));
                
                if (formElement.ApiOptions.EnableDel)
                    fileDetailPathItem.AddOperation(OperationType.Delete, factory.DeleteFile(field));
                
                document.Paths.AddDataDictionaryPath(filePathItem);
                document.Paths.AddDataDictionaryPath(fileDetailPathItem);
            }
            
        }
    }
}