using Microsoft.AspNetCore.Builder;

namespace JJMasterData.Web.Extensions
{
    internal static class IEndpointConventionBuilderExtensions
    {
        public static TBuilder WithAttributes<TBuilder>(this TBuilder builder, Attribute[]? attributes)
            where TBuilder : IEndpointConventionBuilder
        {
            if (attributes != null)
            {
                builder.WithMetadata(attributes);
            }

            return builder;
        }
    }
}
