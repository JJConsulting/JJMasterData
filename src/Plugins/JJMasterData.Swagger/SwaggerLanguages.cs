using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace JJMasterData.Swagger
{
    public class SwaggerLanguages : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var attribute = apiDescription.GetControllerAndActionAttributes<SwaggerLanguageAttribute>().SingleOrDefault();
            if (attribute == null)
            {
                return;
            }

            string language;
            if (string.IsNullOrEmpty(attribute.DefaultLanguage))
                language = Thread.CurrentThread.CurrentUICulture.Name;
            else
                language = attribute.DefaultLanguage;

            if (operation.parameters == null)
                operation.parameters = new List<Parameter>();

            operation.parameters.Add(new Parameter
            {
                name = "Accept-Language",
                @in = "header",
                type = "string",
                @default = language,
                description = "Culture Code",
                required = false
            });

        }
    }

}