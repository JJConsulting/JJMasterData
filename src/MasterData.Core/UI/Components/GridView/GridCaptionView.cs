using System.Linq;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class GridCaptionView(
    string title,
    IControlFactory<JJComboBox> comboBoxFactory,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public bool ShowAsModal { get; init; }
    public required string Name { get; init; }
    public required FormElement FormElement { get; init; }

    public Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        if (ShowAsModal)
        {
            return GetModalHtmlBuilder();
        }

        var field = GetCaptionField();

        return GetCaptionHtmlBuilder(field);
    }

    private async Task<HtmlBuilder> GetCaptionHtmlBuilder(FormElementField field)
    {
        var div = new HtmlBuilder(HtmlTag.Div);

        if (field != null)
        {
            var cbo = comboBoxFactory.Create();
            cbo.Name = field.Name;
            
            if (field.DataItem != null)
                cbo.DataItem = field.DataItem;

            var values = await cbo.GetValuesAsync();

            if (values is not { Count: > 0 })
                return div;
            
            foreach (var item in values)
            {
                div.Append(HtmlTag.Div, div =>
                {
                    div.WithStyle( "height:2.5rem");

                    div.AppendComponent(new JJIcon(item.Icon, item.IconColor)
                    {
                        CssClass = "fa-fw fa-2x"
                    });
                    div.AppendText(" ");
                    div.AppendText(stringLocalizer[item.Description]);
                    div.Append(HtmlTag.Br);
                });
            }
        }
        else
        {
            div.Append(HtmlTag.Br);
            div.AppendText(stringLocalizer["There is no caption to be displayed"]);
        }

        return div;
    }

    private async Task<HtmlBuilder> GetModalHtmlBuilder()
    {
        var field = GetCaptionField();

        var form = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("form-horizontal")
            .WithAttribute("role", "form")
            .Append(await GetCaptionHtmlBuilder(field));
        
        var dialog = new JJModalDialog
        {
            Name = $"{Name}-caption-modal",
            Title = stringLocalizer[title],
            HtmlBuilderContent = form,
            Buttons = 
            [
                new JJLinkButton
                {
                    Name = $"{Name}-caption-modal-close-btn",
                    Icon = IconType.SolidXmark,
                    Text = stringLocalizer["Close"],
                    ShowAsButton = true,
                    OnClientClick = BootstrapHelper.GetCloseModalScript($"{Name}-caption-modal")
                }
            ]
        };
        
        return dialog.BuildHtml();
    }
    
    private FormElementField GetCaptionField()
    {
        return FormElement.Fields.FirstOrDefault(f 
            => f.Component is FormComponent.ComboBox or FormComponent.RadioButtonGroup && (f.DataItem?.ShowIcon ?? false));
    }

}
