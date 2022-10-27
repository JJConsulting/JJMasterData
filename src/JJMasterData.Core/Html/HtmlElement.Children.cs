using JJMasterData.Core.WebComponents;
using System;
using System.Collections.Generic;

namespace JJMasterData.Core.Html
{
    public partial class HtmlElement
    {
        /// <summary>
        /// Insert a HTML element as a child of caller element.
        /// </summary>
        public HtmlElement AppendElement(HtmlElement element)
        {
            if (element != null)
                _children.Add(element);

            return this;
        }

        /// <summary>
        /// Insert a list of HTML element as a child of caller element.
        /// </summary>
        public HtmlElement AppendRange(IList<HtmlElement> listelement)
        {
            if (listelement != null)
            {
                foreach (var item in listelement)
                    AppendElement(item);
            }

            return this;
        }

        /// <summary>
        /// Insert a HTML element as a child of caller element.
        /// </summary>
        public HtmlElement AppendElement(HtmlTag tag, Action<HtmlElement> elementAction = null)
        {
            var childElement = new HtmlElement(tag);
            elementAction?.Invoke(childElement);
            AppendElement(childElement);
            return this;
        }

        /// <summary>
        /// Conditional insert a HTML element as a child of caller element. 
        /// </summary>
        // public HtmlElement AppendElementIf(bool condition, HtmlElement htmlElement = null)
        // {
        //     if (condition)
        //         AppendElement(htmlElement);
        //
        //     return this;
        // }
        
        /// <summary>
        /// Conditional insert a HTML element from a return of a function. Use this to improve performance.
        /// </summary>
        public HtmlElement AppendElementIf(bool condition, Func<HtmlElement> func)
        {
            if (condition)
                AppendElement(func.Invoke());

            return this;
        }

        /// <summary>
        /// Conditional insert HTML element as a child of caller element.
        /// </summary>
        public HtmlElement AppendElementIf(bool condition, HtmlTag tag, Action<HtmlElement> elementAction = null)
        {
            if (condition)
                AppendElement(tag, elementAction);

            return this;
        }

        /// <summary>
        /// Insert raw text as a child of caller element.
        /// </summary>
        public HtmlElement AppendText(string rawText)
        {
            var childElement = new HtmlElement(rawText);
            AppendElement(childElement);
            return this;
        }

        /// <summary>
        /// Insert string representation of the integer as a child of caller element.
        /// </summary>
        public HtmlElement AppendText(int value)
        {
            var childElement = new HtmlElement(value.ToString());
            AppendElement(childElement);
            return this;
        }

        /// <summary>
        /// Conditional insert raw text as a child of caller element.
        /// </summary>
        public HtmlElement AppendTextIf(bool condition, string rawText)
        {
            if (condition)
                AppendText(rawText);

            return this;
        }

        /// <summary>
        /// Append a hidden input to the Element tree.
        /// </summary>
        public HtmlElement AppendHiddenInput(string name, string value)
        {
            return AppendElement(HtmlTag.Input, input =>
            {
                input.WithAttribute("hidden", "hidden");
                input.WithNameAndId(name);
                input.WithValue(value);
            });
        }

        /// <summary>
        /// Append a hidden input to the Element tree.
        /// </summary>
        public HtmlElement AppendHiddenInput(string name)
        {
            return AppendHiddenInput(name, string.Empty);
        }

        /// <summary>
        /// Insert a script tag with a rawScript
        /// </summary>
        public HtmlElement AppendScript(string rawScript)
        {
            var childElement = new HtmlElement(HtmlTag.Script)
                .WithAttribute("type", "text/javascript")
                .AppendText(rawScript);

            return AppendElement(childElement);
        }

        /// <summary>
        /// Insert a JJ component as a child of caller element.
        /// </summary>
        public HtmlElement AppendElement(JJBaseView component)
        {
            if (component != null)
                AppendElement(component.GetHtmlElement());

            return this;
        }

    }
}
