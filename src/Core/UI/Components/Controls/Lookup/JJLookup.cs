using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components.Abstractions;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

/// Represents a field with a value from another Data Dictionary accessed via popup.
public class JJLookup : AsyncControl
{
    internal FormElement FormElement { get; set; }
    private ILookupService LookupService { get; }
    private IEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private ILogger<JJLookup> Logger { get; }

    #region "Properties"

    private string _selectedValue;
    private string _text;

    internal FormStateData FormStateData { get; set; }

    public bool AutoReloadFormFields { get; set; }

    public bool OnlyNumbers { get; set; }

    public PopupSize PopSize
    {
        get
        {
            string val = GetAttr("popupsize");
            if (string.IsNullOrEmpty(val))
                return PopupSize.Full;
            return (PopupSize)int.Parse(val);
        }
        set => SetAttr("popupsize", value);
    }

    public string PopTitle
    {
        get => GetAttr("popuptitle");
        set => SetAttr("popuptitle", value);
    }

    public new string Text
    {
        get
        {
            if (AutoReloadFormFields && _text == null && CurrentContext.IsPost)
            {
                _text = CurrentContext.Request[Name];
            }

            return _text;
        }
        set => _text = value;
    }

    public string SelectedValue
    {
        get
        {
            if (AutoReloadFormFields && string.IsNullOrEmpty(_selectedValue))
            {
                _selectedValue = LookupService.GetSelectedValue(Name).ToString();
            }

            return _selectedValue;
        }
        set => _selectedValue = value;
    }


    public DataElementMap ElementMap { get; }
    
    public string FieldName { get; }
    #endregion

    #region "Constructors"

    internal JJLookup(
        FormElement formElement,
        FormElementField field,
        IHttpContext httpContext,
        ILookupService lookupService,
        IEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper,
        ILogger<JJLookup> logger) : base(httpContext)
    {
        FormElement = formElement;
        ElementMap = field.DataItem?.ElementMap ?? throw new ArgumentException("ElementMap cannot be null.");
        FieldName = field.Name;
        LookupService = lookupService;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
        Logger = logger;
        Enabled = true;
        AutoReloadFormFields = true;
        Name = "jjlookup1";
        PopSize = PopupSize.Full;
        PopTitle = "Search";
    }

    #endregion


    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (!IsLookupRoute())
            return new RenderedComponentResult(await GetLookupHtml());

        if (IsAjaxGetDescription())
            return new JsonComponentResult(await GetResultAsync());
        
        return new EmptyComponentResult();
    }

    private async Task<HtmlBuilder> GetLookupHtml()
    {
        string inputValue = SelectedValue;
        string description = Text;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(inputValue))
            description = await LookupService.GetDescriptionAsync(ElementMap, FormStateData, inputValue, OnlyNumbers);

        var div = new HtmlBuilder(HtmlTag.Div);

        Attributes["lookup-url"] = LookupService.GetLookupUrl(ElementMap, FormStateData, Name);

        if (IsExternalRoute)
        {
            var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(FormElement.Name);
            var componentName = Attributes["pnlname"];
            Attributes["data-panel-reload-url"] = UrlHelper.GetUrl("ReloadPanel", "Form",
                "MasterData", new { dictionaryName = encryptedDictionaryName, componentName });
            Attributes["lookup-result-url"] = UrlHelper.GetUrl("GetResult", "Lookup","MasterData", 
                new
                {
                    dictionaryName = encryptedDictionaryName,
                    componentName = Name,
                    fieldName = FieldName,
                    pageState = FormStateData.PageState
                });
        }

        var textGroup = new JJTextGroup(CurrentContext)
        {
            Name = Name,
            CssClass = $"form-control jjlookup {GetFeedbackIcon(inputValue, description)} {CssClass}",
            InputType = OnlyNumbers ? InputType.Number : InputType.Text,
            MaxLength = MaxLength,
            Text = description,
            Attributes = Attributes,
            ToolTip = ToolTip,
            ReadOnly = ReadOnly, //|| Enabled && !string.IsNullOrEmpty(description),
            Enabled = Enabled,
            Actions = new List<JJLinkButton>
            {
                new()
                {
                    Name = $"btn_{Name}",
                    Enabled = Enabled,
                    ShowAsButton = true,
                    IconClass = "fa fa-search"
                }
            }
        };


        div.AppendComponent(textGroup);
        div.AppendHiddenInput($"id_{Name}", SelectedValue);
        return div;
    }

    private string GetFeedbackIcon(string value, string description)
    {
        if (!string.IsNullOrEmpty(value) & !string.IsNullOrEmpty(description))
            return " jj-icon-success ";
        if (!string.IsNullOrEmpty(value) & string.IsNullOrEmpty(description))
            return " jj-icon-warning";
        return null;
    }


    private async Task<LookupResultDto> GetResultDto()
    {
        LookupResultDto dto = null;
        try
        {
            string searchId = CurrentContext.Request["lkid"];
            string description = await GetDescriptionAsync();
            dto = new LookupResultDto(searchId, description);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,"Error while recovering Lookup description");
        }

        return dto;
    }

    /// <summary>
    /// Recovers the description based on the selected value
    /// </summary>
    /// <returns>Returns the description of the id</returns>
    public async Task<string> GetDescriptionAsync()
    {
        return await LookupService.GetDescriptionAsync(ElementMap, FormStateData, SelectedValue, OnlyNumbers);
    }

    private bool IsAjaxGetDescription()
    {
        string lkaction = CurrentContext.Request.QueryString("lkaction");
        return "ajax".Equals(lkaction);
    }


    private bool IsLookupRoute()
    {
        string pnlName = string.Empty;
        if (Attributes.TryGetValue("pnlname", out var attribute))
            pnlName = attribute?.ToString();

        string lookupRoute = CurrentContext.Request.QueryString("jjlookup_" + pnlName);
        return Name.Equals(lookupRoute);
    }

    public static bool IsLookupRoute(ComponentBase view, IHttpContext context)
    {
        string dataPanelName = string.Empty;
        if (view is JJFormView formView)
            dataPanelName = formView.DataPanel.Name;
        else if (view is JJDataPanel dataPanel)
            dataPanelName = dataPanel.Name;

        string lookupRoute = context.Request.QueryString("jjlookup_" + dataPanelName);
        return !string.IsNullOrEmpty(lookupRoute);
    }

    public static async Task<ComponentResult> GetResultFromPanel(JJDataPanel view)
    {
        string lookupRoute = view.CurrentContext.Request.QueryString("jjlookup_" + view.Name);
        if (string.IsNullOrEmpty(lookupRoute))
            return null;

        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(lookupRoute));
        if (field == null)
            return null;
        var lookup = await view.ComponentFactory.Controls
            .CreateAsync(view.FormElement, field, new FormStateData(view.Values,view.PageState), view.FieldNamePrefix, view.Name) as JJLookup;
        return await lookup!.GetResultAsync();
    }
}