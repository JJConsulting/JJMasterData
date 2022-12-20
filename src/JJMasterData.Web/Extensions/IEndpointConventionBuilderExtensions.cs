using Microsoft.AspNetCore.Builder;

namespace JJMasterData.Web.Extensions
{
    internal static class IEndpointConventionBuilderExtensions
    {
        public static TBuilder WithAttributes<TBuilder>(this TBuilder builder, object[] attributes)
            where TBuilder : IEndpointConventionBuilder
        {
            builder.WithMetadata(attributes);

            return builder;
        }
    }
}
