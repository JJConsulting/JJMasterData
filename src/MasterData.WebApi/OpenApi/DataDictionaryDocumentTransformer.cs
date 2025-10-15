using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.OpenApi.Models;

namespace JJMasterData.WebApi.OpenApi;

public class DataDictionaryDocumentTransformer(IServiceProvider serviceProvider) 
{
    public async Task AddDataDictionaryEndpoints(OpenApiDocument document, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dataDictionaryRepository = scope.ServiceProvider.GetRequiredService<IDataDictionaryRepository>();
        
        var formElements = await dataDictionaryRepository.GetFormElementListAsync();
        
        foreach (var formElement in formElements)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var defaultPathItem = new DataDictionaryPathItem($"/api/masterdata/{formElement.Name}");
            var detailPathItem = new DataDictionaryPathItem($"{defaultPathItem.Key}/{{id}}");
            var factory = new DataDictionaryOperationFactory(formElement, formElement.ApiOptions);

            if (formElement.ApiOptions.EnableGetAll)
                defaultPathItem.AddOperation(OperationType.Get, factory.GetAll());

            if (formElement.ApiOptions.EnableGetDetail)
                detailPathItem.AddOperation(OperationType.Get, factory.Get());

            if (formElement.ApiOptions.EnableAdd)
                defaultPathItem.AddOperation(OperationType.Post, factory.Post());

            if (formElement.ApiOptions.EnableUpdate)
                detailPathItem.AddOperation(OperationType.Put, factory.Put());

            if (formElement.ApiOptions.EnableUpdatePart)
                detailPathItem.AddOperation(OperationType.Patch, factory.Patch());

            if (formElement.ApiOptions.EnableDel)
                detailPathItem.AddOperation(OperationType.Delete, factory.Delete());

            document.Paths.AddDataDictionaryPath(defaultPathItem);
            document.Paths.AddDataDictionaryPath(detailPathItem);

            foreach (var field in formElement.Fields)
            {
                if (field.Component != FormComponent.File || field.DataFile == null)
                    continue;

                var filePathItem = new DataDictionaryPathItem($"/api/masterdata/{formElement.Name}/{{id}}/{field.Name}/file");
                var fileDetailPathItem = new DataDictionaryPathItem($"{filePathItem.Key}/{{fileName}}");

                if (formElement.ApiOptions.EnableGetDetail)
                    fileDetailPathItem.AddOperation(OperationType.Get, factory.GetFile(field));

                if (formElement.ApiOptions.EnableAdd && formElement.ApiOptions.EnableUpdate)
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
