using Microsoft.OpenApi.Models;

namespace JJMasterData.WebApi.Swagger;

internal static class SwaggerExtensions
{
    internal static void AddDataDictionaryPath(this OpenApiPaths paths, DataDictionaryPathItem pathItem)
    {
        paths.Add(pathItem.Key, pathItem.PathItem);
    }

    internal static void AddDefaultValues(this OpenApiResponses responses)
    {

        responses.Add("207",
            new OpenApiResponse
            {
                Description = "Multi Status"
            }
        );
        responses.Add("400",
            new OpenApiResponse
            {
                Description = "Bad Request"
            }
        );

        responses.Add("401",
            new OpenApiResponse
            {
                Description = "Unauthorized"
            }
        );

        responses.Add("403",
            new OpenApiResponse
            {
                Description = "Token Expired"
            }
        );

        responses.Add("404",
            new OpenApiResponse
            {
                Description = "Not Found"
            }
        );

        responses.Add("500",
            new OpenApiResponse
            {
                Description = "Internal Server Error"
            }
        );
    }
}
