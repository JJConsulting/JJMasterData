using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Utils;

namespace JJConsulting.Html.Bootstrap.Extensions;

public static class HtmlBuilderExtensions
{
    extension(HtmlBuilder htmlBuilder)
    {
        public HtmlBuilder PrependComponent<TComponent>(TComponent? component) where TComponent : HtmlComponent
        {
            if (component != null)
                htmlBuilder.Prepend(component.GetHtmlBuilder());

            return htmlBuilder;
        }

        /// <summary>
        /// Insert a <see cref="HtmlComponent"/> as a child of caller builder.
        /// </summary>
        public HtmlBuilder AppendComponent<TComponent>(TComponent? component) where TComponent : HtmlComponent
        {
            if (component is not null)
                htmlBuilder.Append(component.GetHtmlBuilder());

            return htmlBuilder;
        }

        /// <summary>
        /// Set a custom Bootstrap data attribute to HTML builder.
        /// </summary>
        public HtmlBuilder WithDataAttribute(string name, string value)
        {
            var attributeName = BootstrapHelper.Version >= 5 ? $"data-bs-{name}" : $"data-{name}";
            return htmlBuilder.WithAttribute(attributeName, value);
        }

        public HtmlBuilder WithAttribute(string attributeName)
        {
            htmlBuilder.WithAttribute(attributeName, attributeName);
            return htmlBuilder;
        }

        /// <summary>
        /// Sets a tooltip to the HTML Tag
        /// </summary>
        public HtmlBuilder WithToolTip(string? tooltip)
        {
            if (string.IsNullOrEmpty(tooltip))
                return htmlBuilder;

            htmlBuilder.WithAttribute("title", tooltip);
            htmlBuilder.WithAttribute(BootstrapHelper.DataToggle, "tooltip");

            return htmlBuilder;
        }
    }
}