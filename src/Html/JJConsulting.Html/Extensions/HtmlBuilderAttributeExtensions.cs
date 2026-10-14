namespace JJConsulting.Html.Extensions;

using System;
using System.Collections.Generic;

public static class HtmlBuilderAttributeExtensions
{
    extension(HtmlBuilder htmlBuilder)
    {
        public HtmlBuilder WithNameAndId(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
                htmlBuilder.WithId(id).WithName(id);

            return htmlBuilder;
        }

        public HtmlBuilder WithId(string id)
        {
            htmlBuilder.WithAttribute("id", id);
            return htmlBuilder;
        }

        public HtmlBuilder WithName(string name)
        {
            htmlBuilder.WithAttribute("name", name);
            return htmlBuilder;
        }

        public HtmlBuilder WithAttributeIf(bool condition, string name, string value)
        {
            if (condition)
                htmlBuilder.WithAttribute(name, value);

            return htmlBuilder;
        }

        public HtmlBuilder WithAttributeIfNotEmpty(string name, string? value)
        {
            if (value != null && !string.IsNullOrEmpty(value))
                htmlBuilder.WithAttribute(name, value);

            return htmlBuilder;
        }

        public HtmlBuilder WithAttributeIf(bool condition, string nameAndValue)
        {
            return htmlBuilder.WithAttributeIf(condition, nameAndValue, nameAndValue);
        }

        public HtmlBuilder WithCssClass(string? classes)
        {
            if (classes == null || string.IsNullOrWhiteSpace(classes))
                return htmlBuilder;

            if (!htmlBuilder.TryGetAttribute("class", out var existingClasses))
                return htmlBuilder.WithAttribute("class", classes);

            var classSet = new HashSet<string>(existingClasses?.Split(' ') ?? [], StringComparer.InvariantCultureIgnoreCase);

            foreach (var cssClass in classes.Split(' '))
                classSet.Add(cssClass);

            htmlBuilder.WithAttribute("class", string.Join(" ", classSet));

            return htmlBuilder;
        }

        public HtmlBuilder WithCssClassIf(bool conditional, string? classes)
        {
            if (conditional)
                htmlBuilder.WithCssClass(classes);

            return htmlBuilder;
        }

        public HtmlBuilder WithAttributes(Dictionary<string, string> attributes)
        {
            foreach (var v in attributes)
                htmlBuilder.WithAttribute(v.Key, v.Value);

            return htmlBuilder;
        }

        public HtmlBuilder WithValue(string value)
        {
            return htmlBuilder.WithAttribute("value", value);
        }

        public HtmlBuilder WithOnChange(string value)
        {
            htmlBuilder.WithAttribute("onchange", value);
            return htmlBuilder;
        }

        public HtmlBuilder WithOnClick(string value)
        {
            htmlBuilder.WithAttribute("onclick", value);
            return htmlBuilder;
        }

        public HtmlBuilder WithStyle(string value)
        {
            htmlBuilder.WithAttribute("style", value);
            return htmlBuilder;
        }

        public HtmlBuilder WithHref(string link)
        {
            htmlBuilder.WithAttribute("href", link);
            return htmlBuilder;
        }
    }
}