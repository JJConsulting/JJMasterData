using System.Net;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.WebApi.Attributes;
using JJMasterData.WebApi.Models;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Endpoints;

public static class MasterDataApiEndpoints
{
    public static IEndpointConventionBuilder MapMasterDataApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/masterdata/{elementName}")
            .ExcludeFromDescription()
            .WithMetadata(new MasterDataApiAttribute())
            .WithTags("MasterData");

        group.MapGet("/", async (
            string elementName,
            [FromQuery] int? pag,
            [FromQuery] int? regporpag,
            [FromQuery] string? orderby,
            [FromQuery] int? tot,
            HttpRequest request,
            MasterApiService service) =>
        {
            var currentPage = pag ?? 1;
            var recordsPerPage = regporpag ?? 5;
            
            if (request.Headers.Accept.ToString().Contains("text/csv"))
            {
                var text = await service.GetListFieldAsTextAsync(elementName, currentPage, recordsPerPage, orderby);
                return Results.Text(text, "text/csv");
            }

            var response = await service.GetListFieldsAsync(elementName, currentPage, recordsPerPage, orderby, tot ?? 0);
            return Results.Ok(response);
        })
        .Produces<MasterApiListResponse>();

        group.MapGet("/{id}", async (string elementName, string id, MasterApiService service) =>
        {
            var result = await service.GetFieldsAsync(elementName, id);
            return Results.Ok(result);
        })
        .Produces<Dictionary<string, object>>();

        group.MapPost("/", async (
            [FromBody] Dictionary<string, object?>[] listParam,
            string elementName,
            [FromQuery] bool replace,
            MasterApiService service) =>
        {
            var result = await service.SetFieldsAsync(listParam, elementName, replace);
            return GetResponseMessage(result);
        });

        group.MapPut("/", async (
            [FromBody] Dictionary<string, object?>[] listParam,
            string elementName,
            MasterApiService service) =>
        {
            var result = await service.UpdateFieldsAsync(listParam, elementName);
            return GetResponseMessage(result);
        });

        group.MapPatch("/", async (
            [FromBody] Dictionary<string, object?>[] listParam,
            string elementName,
            MasterApiService service) =>
        {
            var result = await service.UpdatePartAsync(listParam, elementName);
            return GetResponseMessage(result);
        });

        group.MapDelete("/{id}", async (string elementName, string id, MasterApiService service) =>
        {
            var result = await service.DeleteAsync(elementName, id);
            return Results.Ok(result);
        });

        group.MapPost("/trigger/{pageState?}/{objname?}", async (
            string elementName,
            [FromBody] Dictionary<string, object>? paramValues,
            PageState? pageState,
            string? objname,
            MasterApiService service) =>
        {
            var result = await service.PostTriggerAsync(elementName, paramValues, pageState ?? PageState.List, objname);
            return Results.Ok(result);
        })
        .Produces<FormValues[]>();

        group.MapFileApi();
        
        return group;
    }

    private static IResult GetResponseMessage(List<ResponseLetter> responseLetters)
    {
        var list = responseLetters.ToList();
        if (list.Count == 0)
            throw new ArgumentNullException(nameof(responseLetters), "Response not found");

        if (list.Count == 1)
            return Results.Json(responseLetters, statusCode: list[0].Status);

        var total = list.Count;
        var inserts = list.Count(x => x.Status == (int)HttpStatusCode.Created);
        if (total == inserts)
            return Results.Json(responseLetters, statusCode: (int)HttpStatusCode.Created);

        var updates = list.Count(x => x.Status == (int)HttpStatusCode.OK);
        if (total == updates)
            return Results.Ok(responseLetters);

        var errors = total - inserts - updates;
        if (total == errors)
            return Results.BadRequest(responseLetters);

        return Results.Json(responseLetters, statusCode: (int)HttpStatusCode.MultiStatus);
    }
}
