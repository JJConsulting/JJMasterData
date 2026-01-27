using System.Collections.Generic;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJMasterData.Core.Http.Abstractions;


namespace JJMasterData.Core.UI.Components;

public abstract class ControlBase(IFormValues formValues) : ComponentBase
{
    /// <summary>
    /// Property to check if the control is enabled.
    /// (Default = true)
    /// </summary>
    public bool Enabled { get; set; } = true;

    public Dictionary<string, object> UserValues { get; set; } = new();
    
    /// <summary>
    /// Property to tell if the control is readonly, but the value is sent to the server.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Text shown when the component don't have any content.
    /// </summary>
    public string PlaceHolder { get; set; }

    /// <summary>
    /// Text shown when the component is hovered.
    /// </summary>
    public virtual string Tooltip { get; set; }
    
    public int MaxLength { get; set; }

    internal IFormValues FormValues { get; } = formValues;

    /// <summary>
    /// Value of the component.
    /// </summary>
    public string Text
    {
        get
        {
            if (field == null && FormValues.ContainsFormValues())
            {
                field = FormValues[Name];
            }

            return field;
        }
        set;
    }

    public ValueTask<ComponentResult> GetResultAsync()
    {
        if (Visible)
            return BuildResultAsync();

        return new ValueTask<ComponentResult>(EmptyComponentResult.Value);
    }

    protected virtual async ValueTask<ComponentResult> BuildResultAsync()
    {
        var html = await GetHtmlBuilderAsync();
        
        return new RenderedComponentResult(html);
    }
    
    protected internal abstract ValueTask<HtmlBuilder> GetHtmlBuilderAsync();
}
