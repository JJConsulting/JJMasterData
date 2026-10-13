using System;

namespace JJConsulting.Html.Extensions;

public static partial class HtmlBuilderAppendExtensions
{
    extension(HtmlBuilder htmlBuilder)
    {
        public HtmlBuilder Append(HtmlTag tag, Action<HtmlBuilder>? builderAction = null)
        {
            var child = new HtmlBuilder(tag);
            builderAction?.Invoke(child);
            htmlBuilder.Append(child);
            return htmlBuilder;
        }

        public HtmlBuilder AppendIf(bool condition, Func<HtmlBuilder> func)
        {
            if (condition)
                htmlBuilder.Append(func.Invoke());

            return htmlBuilder;
        }

        public HtmlBuilder AppendIf(bool condition, HtmlTag tag, Action<HtmlBuilder>? builderAction = null)
        {
            if (condition)
                htmlBuilder.Append(tag, builderAction);

            return htmlBuilder;
        }

        public HtmlBuilder AppendText(string? rawText)
        {
            if (rawText != null && !string.IsNullOrEmpty(rawText))
            {
                htmlBuilder.Append(new HtmlText(rawText, encode:true));
            }

            return htmlBuilder;
        }

        public HtmlBuilder AppendTextIf(bool condition, string? rawText)
        {
            if (condition)
                htmlBuilder.AppendText(rawText);

            return htmlBuilder;
        }

        public HtmlBuilder AppendHiddenInput(string name, string value)
        {
            var input = new HtmlBuilder(HtmlTag.Input);
            input.WithAttribute("hidden", "hidden");
            input.WithNameAndId(name);
            input.WithValue(value);

            return htmlBuilder.Append(input);
        }

        public HtmlBuilder AppendHiddenInput(string name)
        {
            return htmlBuilder.AppendHiddenInput(name, string.Empty);
        }

        public HtmlBuilder AppendStyle(string rawCss)
        {
            var child = new HtmlBuilder(HtmlTag.Style)
                .Append(new HtmlText(rawCss, encode:false));

            return htmlBuilder.Append(child);
        }

        public HtmlBuilder AppendStyleIf(bool condition, string rawCss)
        {
            if (!condition)
                return htmlBuilder;

            return htmlBuilder.AppendStyle(rawCss);
        }

        public HtmlBuilder AppendScript(string rawScript)
        {
            var child = new HtmlBuilder(HtmlTag.Script)
                .WithAttribute("type", "text/javascript")
                .Append(new HtmlText(rawScript, encode: false));

            return htmlBuilder.Append(child);
        }

        public HtmlBuilder AppendScriptIf(bool condition, string rawScript)
        {
            if (!condition)
                return htmlBuilder;

            return htmlBuilder.AppendScript(rawScript);
        }
    }
}