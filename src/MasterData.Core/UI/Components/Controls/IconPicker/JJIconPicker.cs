#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;

using Microsoft.Extensions.Localization;


namespace JJMasterData.Core.UI.Components;

public class JJIconPicker(
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IUrlHelper urlHelper,
    IControlFactory<JJComboBox> comboBoxFactory,
    IFormValues formValues) : ControlBase(formValues)
{
    public FontAwesomeIcon? SelectedIcon { get; set; }
    public string? Id { get; set; }

    private static readonly List<DataItemValue> Items;
    
    static JJIconPicker()
    {
        Items = FontAwesomeIcon.GetAll().Select(icon => new DataItemValue
        {
            Id = ((int)icon).ToString(),
            Description = icon.ToString(),
            Icon = icon
        }).ToList();
    }

    protected internal override async ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var id = Id ?? Name;
        var comboBox = comboBoxFactory.Create();
        comboBox.Name = Name;
        comboBox.Id =  id;
        comboBox.Enabled = Enabled;
        if(SelectedIcon is not null)
        {
            comboBox.SelectedValue = ((int)SelectedIcon).ToString();
        }
        comboBox.DataItem = new FormElementDataItem
        {
            DataItemType = DataItemType.Manual,
            Items = Items,
            FirstOption = FirstOptionMode.Choose,
            ShowIcon = Enabled,
        };

        comboBox.EnableLocalization = false;
        
        comboBox.Attributes["data-live-search"] = "true";
        comboBox.Attributes["data-virtual-scroll"] = "true";
        comboBox.Attributes["data-size"] = "false";
        comboBox.Attributes["data-sanitize"] = "false";
        comboBox.Attributes["data-none-results-text"] = stringLocalizer["No icons found."];
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClassIf(Enabled,"input-group");
        div.Append(await comboBox.GetHtmlBuilderAsync());
        div.AppendIf(Enabled,HtmlTag.Div,div =>
        {
            var tooltip = stringLocalizer["Search Icon"];
            div.WithCssClass(BootstrapHelper.BtnDefault);
            div.WithToolTip(tooltip);
            div.AppendComponent(new JJIcon(FontAwesomeIcon.Search));
            var url = urlHelper.Action("Index", "Icons", new { inputId = id, Area="MasterData" });
            div.WithOnClick( $"iconsModal.showUrl('{url}', '{tooltip}', '{(int)ModalSize.ExtraLarge}')");
        });
        return div;
    }
}