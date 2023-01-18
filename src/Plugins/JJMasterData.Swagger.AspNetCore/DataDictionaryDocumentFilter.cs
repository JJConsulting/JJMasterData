using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DI;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Swagger.AspNetCore;

public class DataDictionaryDocumentFilter : IDocumentFilter
{
    private readonly IDataDictionaryRepository _dataDictionaryRepository;

    public DataDictionaryDocumentFilter()
    {
        _dataDictionaryRepository = JJServiceCore.DataDictionaryRepository;
    }

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        document.Info.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        var dictionaries = _dataDictionaryRepository.GetMetadataList(true);

        foreach (var metadata in dictionaries)
        {
            var formElement = metadata.GetFormElement();

            var defaultPathItem = new DataDictionaryPathItem($"/MasterApi/{formElement.Name}");
            var detailPathItem = new DataDictionaryPathItem($"{defaultPathItem.Key}/{{id}}");
            var factory = new DataDictionaryOperationFactory(formElement, metadata.ApiOptions);

            if (metadata.ApiOptions.EnableGetAll)
                defaultPathItem.AddOperation(OperationType.Get, factory.GetAll());

            if (metadata.ApiOptions.EnableGetDetail)
                detailPathItem.AddOperation(OperationType.Get, factory.Get());
            
            if (metadata.ApiOptions.EnableAdd)
                defaultPathItem.AddOperation(OperationType.Post, factory.Post());

            if (metadata.ApiOptions.EnableUpdate)
                defaultPathItem.AddOperation(OperationType.Put, factory.Put());

            if (metadata.ApiOptions.EnableUpdatePart)
                defaultPathItem.AddOperation(OperationType.Patch, factory.Patch());

            if (metadata.ApiOptions.EnableDel)
                detailPathItem.AddOperation(OperationType.Delete, factory.Delete());

            document.Paths.AddDataDictionaryPath(defaultPathItem);
            document.Paths.AddDataDictionaryPath(detailPathItem);
            
            foreach (var field in formElement.Fields)
            {
                if (field.Component != FormComponent.File || field.DataFile == null) 
                    continue;
                
                var filePathItem = new DataDictionaryPathItem($"/MasterApi/{formElement.Name}/{{id}}/{field.Name}/file");
                var fileDetailPathItem = new DataDictionaryPathItem($"{filePathItem.Key}/{{fileName}}");
                
                if (metadata.ApiOptions.EnableGetDetail)
                    fileDetailPathItem.AddOperation(OperationType.Get, factory.GetFile(field));
                    
                if (metadata.ApiOptions.EnableAdd && metadata.ApiOptions.EnableUpdate)
                    filePathItem.AddOperation(OperationType.Post, factory.PostFile(field));
                        
                if (metadata.ApiOptions.EnableUpdatePart)
                    fileDetailPathItem.AddOperation(OperationType.Patch, factory.RenameFile(field));
                
                if (metadata.ApiOptions.EnableDel)
                    fileDetailPathItem.AddOperation(OperationType.Delete, factory.DeleteFile(field));
                
                document.Paths.AddDataDictionaryPath(filePathItem);
                document.Paths.AddDataDictionaryPath(fileDetailPathItem);
            }
            
        }
    }
}