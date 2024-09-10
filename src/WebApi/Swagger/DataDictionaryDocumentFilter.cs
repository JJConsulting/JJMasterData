using System.Reflection;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Tasks;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JJMasterData.WebApi.Swagger;

public class DataDictionaryDocumentFilter(
    IHttpContextAccessor httpContextAccessor, 
    IServiceProvider serviceProvider) : IDocumentFilter
{
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;
    private static string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

    protected virtual bool IsAuthenticated(HttpContext httpContext)
    {
        return httpContext.User.Identity?.IsAuthenticated is true;
    }
    
    protected virtual ValueTask<bool> IsElementAllowedAsync(string elementName)
    {
        return new(true);
    }
    
    //TODO: Swagger 6.8 not released
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        AsyncHelper.RunSync(()=>ApplyAsync(swaggerDoc));
    }

    private async Task ApplyAsync(OpenApiDocument document)
    {
        if (!IsAuthenticated(httpContextAccessor.HttpContext!))
            return;
        
        document.Info.Version = Version;
        
        using var scope = ServiceProvider.CreateScope();

        var dataDictionaryRepository = scope.ServiceProvider.GetRequiredService<IDataDictionaryRepository>();
        
        var formElements = await dataDictionaryRepository.GetFormElementListAsync();

        foreach (var formElement in formElements)
        {
            if (!await IsElementAllowedAsync(formElement.Name))
                continue;
            
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