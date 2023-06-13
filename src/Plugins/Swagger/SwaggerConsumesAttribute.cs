using System;
using System.Collections.Generic;

namespace JJMasterData.Swagger
{
    /// <summary>
    /// SwaggerResponseContentTypeAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerConsumesAttribute : Attribute
    {
        /// <summary>
        /// SwaggerResponseContentTypeAttribute
        /// </summary>
        public SwaggerConsumesAttribute(params string[] contentTypes)
        {
            ContentTypes = contentTypes;
        }

        public IEnumerable<string> ContentTypes { get; }
    }
}