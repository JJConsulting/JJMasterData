using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Abstractions;
using JJConsulting.Html.Bootstrap.Models;

namespace JJConsulting.Html.Bootstrap.Components;

/// <summary>
/// Represents a <see cref="JJAlert"/> with error messages.
/// </summary>
public class JJValidationSummary : HtmlComponent
{
    public string? Title { get; set; }

    public List<string> Errors { get; }

    /// <summary>
    /// Enable close panel
    /// (Default = True)
    /// </summary>
    public bool ShowCloseButton { get; set; }

    public JJValidationSummary()
    {
        Visible = true;
        Errors = [];
        ShowCloseButton = true;
    }

    public JJValidationSummary(IEnumerable<string> errors) : this()
    {
        Errors.AddRange(errors);
    }

    public JJValidationSummary(Dictionary<string,string> errors) : this()
    {
        foreach (var error in errors)
        {
            Errors.Add(error.Value);
        }
    }

    protected override HtmlBuilder BuildHtml()
    {
        var alert = new JJAlert
        {
            Color = BootstrapColor.Danger,
            Icon = FontAwesomeIcon.ExclamationTriangle,
            Title = Title,
            ShowCloseButton = ShowCloseButton,
        };

        alert.Messages.AddRange(Errors);

        return alert.GetHtmlBuilder();
    }
}