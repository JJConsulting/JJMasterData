#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;

using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// Represents a field with a value from another FormElement accessed via modal.
public class JJLookup : ControlBase
{
    private IHttpRequest HttpRequest { get; }
    private RouteContextFactory RouteContextFactory { get; }
    private FormValuesService FormValuesService { get; }
    private IEncryptionService EncryptionService { get; }
    private LookupService LookupService { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private IComponentFactory ComponentFactory { get; }

    #region "Properties"

    private object? _selectedValue;
    private string? _text;

    internal FormStateData? FormStateData { get; set; }

    public bool AutoReloadFormFields { get; set; }

    public bool OnlyNumbers { get; set; }

    public ModalSize ModalSize
    {
        get
        {
            string val = GetAttribute("modal-size");
            if (string.IsNullOrEmpty(val))
                return ModalSize.ExtraLarge;
            return (ModalSize)int.Parse(val);
        }
        set => SetAttribute("modal-size", ((int)value).ToString());
    }

    public string ModalTitle
    {
        get => GetAttribute("modal-title");
        set => SetAttribute("modal-title", value);
    }
    
    public new string? Text
    {
        get
        {
            if (AutoReloadFormFields && _text is not null && FormValues.ContainsFormValues())
            {
                _text = FormValues[Name];
            }

            return _text;
        }
        set => _text = value;
    }
    
    public object? SelectedValue
    {
        get
        {
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue?.ToString()))
            {
                _selectedValue = LookupService.GetSelectedValue(Name);
            }

            return _selectedValue;
        }
        set => _selectedValue = value;
    }


    public DataElementMap ElementMap { get; }
    
    public string? FieldName { get; set; }
    
    internal string? ElementName { get; set; }
    internal string? ParentElementName { get; set; }
    
    protected RouteContext RouteContext { get; }
    
    internal ComponentContext ComponentContext => RouteContext.ComponentContext;
    #endregion

    #region "Constructors"

    internal JJLookup(
        FormElement formElement,
        FormElementField field,
        ControlContext controlContext,
        IHttpRequest httpRequest,
        RouteContextFactory routeContextFactory,
        FormValuesService formValuesService,
        IEncryptionService encryptionService,
        LookupService lookupService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IComponentFactory componentFactory) : base(httpRequest.Form)
    {
        RouteContext = routeContextFactory.Create();
        ElementMap = field.DataItem?.ElementMap ?? new DataElementMap
        {
            ElementName = formElement.Name
        };
        ElementName = formElement.ParentName;
        FieldName = field.Name;
        HttpRequest = httpRequest;
        RouteContextFactory = routeContextFactory;
        FormValuesService = formValuesService;
        EncryptionService = encryptionService;
        LookupService = lookupService;
        StringLocalizer = stringLocalizer;
        ComponentFactory = componentFactory;
        Enabled = true;
        AutoReloadFormFields = false;
        Name = field?.Name!;
        ModalSize = ModalSize.Large;
        ModalTitle = "Search";
        FormStateData = controlContext.FormStateData;
        UserValues = controlContext.FormStateData.UserValues ?? new(StringComparer.InvariantCultureIgnoreCase);
        SelectedValue = controlContext.Value?.ToString();

        if (field != null)
        {
            SetAttributes(field.Attributes);

            if (field.DataType is FieldType.Int)
            {
                OnlyNumbers = true;
                MaxLength = 11;
            }
            else
            {
                MaxLength = field.Size;
            }
        }
    }

    #endregion
    
    protected override async ValueTask<ComponentResult> BuildResultAsync()
    {
        if (ComponentContext is ComponentContext.LookupDescription && HttpRequest.QueryString["fieldName"] == FieldName)
        {
            return await GetLookupDescription();
        }
        return new RenderedComponentResult(await GetHtmlBuilderAsync());
    }
    
    public async Task<JsonComponentResult> GetLookupDescription()
    {
        var selectedValue = LookupService.GetSelectedValue(Name);
        var description = await LookupService.GetDescriptionAsync(ElementMap, FormStateData, selectedValue, false);
            return new JsonComponentResult(new LookupResultDto(selectedValue!, description!));
    }

    protected internal override async ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        if (ElementMap is null)
            throw new ArgumentNullException(nameof(ElementMap));
        if (FieldName is null)
            throw new ArgumentNullException(nameof(FieldName));
        object? inputValue = SelectedValue;
        string? description = Text;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(inputValue?.ToString()))
            description = await LookupService.GetDescriptionAsync(ElementMap, FormStateData, inputValue?.ToString(), OnlyNumbers);
        
        Attributes["lookup-field-name"] = FieldName;
        
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("input-group d-flex" );

        var routeContext = new RouteContext(ElementName, ParentElementName,
            ComponentContext.LookupDescription);
        
        Attributes["route-context"] = EncryptionService.EncryptObject(routeContext);

        var flexLayout = GetFlexLayout();

        var hasDescription = !string.IsNullOrEmpty(ElementMap.DescriptionFieldName);
        
        var idTextBox = ComponentFactory.Controls.TextBox.Create();
        idTextBox.Name = Name;
        
        var icon = GetFeedbackIcon(inputValue?.ToString(), description, hasDescription);

        idTextBox.CssClass = $"form-control jj-lookup {icon} {CssClass}";
        idTextBox.InputType = OnlyNumbers ? InputType.Number : InputType.Text;
        idTextBox.MaxLength = MaxLength;
        idTextBox.Text = SelectedValue?.ToString() ?? string.Empty;
        
        
        idTextBox.Attributes = new Dictionary<string, string>(Attributes);
        idTextBox.Tooltip = Tooltip;
        idTextBox.ReadOnly = ReadOnly;
        idTextBox.Enabled = Enabled;

        idTextBox.Attributes["style"] = $"flex:{flexLayout.Item1}";

        div.Append(idTextBox.GetHtmlBuilder());
        
        if (hasDescription)
        {
            if (BootstrapHelper.Version == 3)
            {
                div.AppendSpan(span =>
                {
                    span.WithCssClass("input-group-btn");
                    span.WithStyle( "width:0px;");
                });
            }

            var descriptionTextBox = ComponentFactory.Controls.TextBox.Create();
            descriptionTextBox.Name = $"{Name}-description";
            descriptionTextBox.CssClass = $"form-control jj-lookup {CssClass}";
            descriptionTextBox.InputType = InputType.Text;
            descriptionTextBox.MaxLength = MaxLength;
            descriptionTextBox.Text = description;
            descriptionTextBox.Attributes = new Dictionary<string, string>(Attributes);
            descriptionTextBox.Tooltip = Tooltip;
            descriptionTextBox.Enabled = false;

            descriptionTextBox.Attributes["style"] = $"flex:{flexLayout.Item2}";
            
            div.Append(descriptionTextBox.GetHtmlBuilder());
        }

        var formViewUrl = LookupService.GetFormViewUrl(ElementMap, FormStateData, Name);
        
        var button = new JJLinkButton();
        button.Name = $"btn_{Name}";
        button.Enabled = Enabled;
        button.ShowAsButton = true;
        button.OnClientClick = $"defaultModal.showIframe('{formViewUrl}', '{StringLocalizer[ModalTitle]}', '{(int)ModalSize}')";
        button.IconClass = "fa fa-search";

        if (BootstrapHelper.Version == 3)
        {
            div.AppendDiv(div =>
            {
                div.WithCssClass("input-group-btn");
                div.AppendComponent(button);
            });
        }
        else
        {
            div.AppendComponent(button);
        }       

        
        return div;
    }

    private static string? GetFeedbackIcon(string? inputValue, string? description, bool hasDescription)
    {
        return string.IsNullOrEmpty(inputValue) switch
        {
            false when !string.IsNullOrEmpty(description) || !hasDescription => " jj-icon-success ",
            false when string.IsNullOrEmpty(description) || hasDescription => " jj-icon-warning",
            _ => null
        };
    }

    private (int,int) GetFlexLayout()
    {
        return MaxLength switch
        {
            <= 4 => (2, 10),
            <= 8 => (3, 9),
            <= 12 => (4, 8),
            <= 16  => (5, 7),
            _  => (6, 6)
        };
    }
}