using System.Collections.Generic;

namespace JJMasterData.Core.Web.Html
{
    public partial class HtmlBuilder
    {
        /// <summary>
        /// Set HTML builder name and ID.
        /// </summary>
        public HtmlBuilder WithNameAndId(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
                WithAttribute("id", id).WithAttribute("name", id);

            return this;
        }

        /// <summary>
        /// Set attribute to the HTML builder.
        /// </summary>
        public HtmlBuilder WithAttribute(string name, string value)
        {
            
            _attributes[name] = value;
            return this;
        }

        /// <summary>
        /// Set attribute to the HTML builder on condition.
        /// </summary>
        public HtmlBuilder WithAttributeIf(bool condition, string name, string value)
        {
            if (condition)
                WithAttribute(name, value);

            return this;
        }

        /// <summary>
        /// Set attribute to the HTML builder.
        /// </summary>
        public HtmlBuilder WithSingleAttribute(string nameAndValue)
        {
            return WithAttribute(nameAndValue, nameAndValue);
        }

        /// <summary>
        /// Set attribute to the HTML builder on condition.
        /// </summary>
        public HtmlBuilder WithAttributeIf(bool condition, string nameAndValue)
        {
            return WithAttributeIf(condition, nameAndValue, nameAndValue);
        }

        /// <summary>
        /// Set CSS classes attributes, if already exists it will be ignored.
        /// </summary>
        public HtmlBuilder WithCssClass(string classes)
        {
            if (string.IsNullOrWhiteSpace(classes))
                return this;

            if (!_attributes.ContainsKey("class"))
                return WithAttribute("class", classes);

            var classList = new List<string>();
            classList.AddRange(_attributes["class"].Split(' '));
            foreach (string cssClass in classes.Split(' '))
            {
                if (!classList.Contains(cssClass))
                    classList.Add(cssClass);
            }

            _attributes["class"] = string.Join(" ", classList);

            return this;
        }

        /// <summary>
        /// Conditional to set classes attributes, if already exists it will be ignored.
        /// </summary>
        public HtmlBuilder WithCssClassIf(bool conditional, string classes)
        {
            if (conditional)
                WithCssClass(classes);

            return this;
        }

        /// <summary>
        /// Set range of attrs
        /// </summary>
        internal HtmlBuilder WithAttributes(IDictionary<string,dynamic> attributes)
        {
            foreach (var v in attributes)
            {
                _attributes.Add(v.Key.ToString(), v.Value.ToString());
            }

            return this;
        }

        /// <summary>
        /// Sets a tooltip to the HTML Element
        /// </summary>
        public HtmlBuilder WithToolTip(string tooltip)
        {
            if (!string.IsNullOrEmpty(tooltip))
            {
                if (_attributes.ContainsKey("title"))
                    _attributes["title"] = tooltip;
                else
                    _attributes.Add("title", tooltip);

                if (_attributes.ContainsKey(BootstrapHelper.DataToggle))
                    _attributes[BootstrapHelper.DataToggle] = "tooltip";
                else
                    _attributes.Add(BootstrapHelper.DataToggle, "tooltip");
            }

            return this;
        }

        /// <summary>
        /// Set custom data attribute to HTML builder.
        /// </summary>
        public HtmlBuilder WithValue(string @value)
        {
            return WithAttribute("value", @value);
        }

        /// <summary>
        /// Set a custom Bootstrap data attribute to HTML builder.
        /// </summary>
        public HtmlBuilder WithDataAttribute(string name, string value)
        {
            string attributeName = BootstrapHelper.Version >= 5 ? "data-bs-" : "data-";
            attributeName += name;
            return WithAttribute(attributeName, value);
        }

    }
}
