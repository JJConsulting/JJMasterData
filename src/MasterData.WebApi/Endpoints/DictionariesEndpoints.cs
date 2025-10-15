using System.Diagnostics;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.WebApi.Models;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Endpoints;

public static class DictionariesEndpoints
{
    public static IEndpointRouteBuilder MapDictionaries(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/data-dictionary")
            .WithTags("Dictionaries");

        group.MapGet("/", async (IDataDictionaryRepository dataDictionaryRepository) =>
            {
                var dicList = await dataDictionaryRepository.GetFormElementListAsync(true);
                return dicList is null ? Results.NotFound() : Results.Ok(dicList);
            })
            .Produces<List<FormElement>>()
            .WithSummary("Get all dictionaries with sync enabled.")
            .WithDescription("Returns all dictionaries with synchronization enabled.");

        group.MapGet("/{id}", (string id, IDataDictionaryRepository dataDictionaryRepository) =>
                dataDictionaryRepository.GetFormElementAsync(id))
            .Produces<FormElement>()
            .WithSummary("Get a specific data dictionary.")
            .WithDescription("Returns a single data dictionary based on element name.");

        group.MapPost("/count", async ([FromBody] DicSyncParam[] param, DictionariesService dictionariesService) =>
            {
                var result = await dictionariesService.GetSyncInfoAsync(param, Debugger.IsAttached);
                return Results.Ok(result);
            })
            .Produces<DicSyncInfo>()
            .WithSummary("Returns the number of records modified from a specific date.")
            .WithDescription("Checks how many records were modified for each dictionary since the given date.");

        return group;
    }
}