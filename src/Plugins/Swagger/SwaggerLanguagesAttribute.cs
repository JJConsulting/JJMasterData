using System;

namespace JJMasterData.Swagger
{
    /// <summary>
    /// SwaggerResponseContentTypeAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerLanguageAttribute : Attribute
    {
        /// <summary>
        /// SwaggerResponseContentTypeAttribute
        /// </summary>
        public SwaggerLanguageAttribute(string defaultLanguage = null)
        {
            DefaultLanguage = defaultLanguage;
        }

        public string DefaultLanguage { get; }
    }
}