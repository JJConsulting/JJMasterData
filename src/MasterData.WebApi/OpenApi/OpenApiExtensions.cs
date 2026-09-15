using Microsoft.OpenApi;

namespace JJMasterData.WebApi.OpenApi;

internal static class OpenApiExtensions
{
    extension(OpenApiOperation operation)
    {
        public OpenApiOperation WithTag(OpenApiTagReference tagReference)
        {
            operation.Tags = new HashSet<OpenApiTagReference> { tagReference };
            return operation;
        }
    }
    
    extension(OpenApiPaths paths)
    {
        internal void AddDataDictionaryPath(DataDictionaryPathItem pathItem)
        {
            paths.Add(pathItem.Key, pathItem.PathItem);
        }
    }

    extension(OpenApiResponses responses)
    {
        internal void AddDefaultValues()
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
}
