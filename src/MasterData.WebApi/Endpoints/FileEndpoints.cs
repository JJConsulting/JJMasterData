using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Endpoints;

public static class FileEndpoints
{
    internal static IEndpointRouteBuilder MapFileApi(this RouteGroupBuilder parentGroup)
    {
        var group = parentGroup.MapGroup("{id}/{fieldName}/file")
            .WithTags("File");

        group.MapGet("/{fileName}", async (
            string elementName,
            string fieldName,
            string id,
            string fileName,
            ElementFileService service) =>
        {
            var fileStream = await service.GetElementFileAsync(elementName, id, fieldName, fileName);
            if (fileStream == null)
                return Results.NotFound();

            return Results.File(fileStream, "application/octet-stream", fileName);
        });

        group.MapPost("/", async (
            string elementName,
            string fieldName,
            string id,
            IFormFile file,
            ElementFileService service) =>
        {
            await service.SetElementFileAsync(elementName, fieldName, id, file);
            return Results.Created(
                $"api/masterdata/{elementName}/{id}/{fieldName}/{file.FileName}/file",
                "File successfully created.");
        });

        group.MapPatch("/{fileName}", async (
            string elementName,
            string fieldName,
            string id,
            string fileName,
            [FromQuery] string newName,
            ElementFileService service) =>
        {
            await service.RenameFileAsync(elementName, fieldName, id, fileName, newName);
            return Results.Ok($"File sucessfuly renamed from {fileName} to {newName}");
        });

        group.MapDelete("/{fileName}", async (
            string elementName,
            string fieldName,
            string id,
            string fileName,
            ElementFileService service) =>
        {
            await service.DeleteFileAsync(elementName, fieldName, id, fileName);
            return Results.Ok("File successfully deleted.");
        });

        return group;
    }
}
