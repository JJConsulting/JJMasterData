using System.Collections;
using System.Text;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.WebComponents;

public class JJSlider : JJBaseControl
{
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public int? Value { get; set; }
    public bool ShowInput { get; set; } = true;

    public JJSlider(float minValue = 0, float maxValue = 100)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
    
    public static JJBaseView GetInstance(FormElementField field, object value, bool enable, bool readOnly, string fieldName)
    {
        var slider = new JJSlider(field.MinValue ?? 0f, field.MaxValue ?? 100)
        {
            Name = fieldName ?? field.Name,
            Value = !string.IsNullOrEmpty(value?.ToString()) ? int.Parse(value.ToString()) : null,
            Enable = enable,
            ReadOnly = readOnly
        };
        return slider;
    }

    protected override string RenderHtml()
    {
        var html = new StringBuilder();

        html.Append($"<div class=\"{CssClass}\">");
        html.Append("   <div class=\"row\">");
        html.Append($"<div class=\"{(ShowInput ? "col-sm-9" : "col-sm-12")}\">");
        html.Append(
            "<input " +
            $"name=\"{Name}\"" +
            " type=\"range\" " +

            $" min=\"{MinValue}\"" +
            "  step=\"1\"" +
            $" max=\"{MaxValue}\" " +
            $" value=\"{Value}\"" +
            " class=\"jjslider form-range\" ");
        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append('"');
            }
            html.Append(' ');
        }
        html.Append($" id=\"{Name}\"/>");
        html.Append("</div>");

        if (ShowInput)
        {
            html.Append("<div class=\"col-sm-3\">");
            html.Append(
                "  <input class=\"jjslider-value form-control\" " +
                $"  id=\"{Name}-value\" " +
                $"  min={MinValue} max={MaxValue}" +
                "   type=\"number\"" +
                $"  value=\"{Value}\"" +
                $"  name=\"{Name}-value\"/>");
            html.Append("</div>");
        }

        html.Append("   </div>");
        html.Append("</div>");
        return html.ToString();
    }


}