using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components.Abstractions;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Components;

/// Represents a field with a value from another Data Dictionary accessed via popup.
public class JJLookup : JJAsyncBaseControl
{
    private ILookupService LookupService { get; }
    private ILogger<JJLookup> Logger { get; }


    #region "Properties"

    private string _selectedValue;
    private string _text;
    private FormElementDataItem _dataItem;

    internal IDictionary<string, dynamic> FormValues { get; set; }

    internal PageState PageState { get; set; }

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


    public FormElementDataItem DataItem
    {
        get => _dataItem ??= new FormElementDataItem();
        set => _dataItem = value;
    }

    #endregion

    #region "Constructors"

    public JJLookup(
        IHttpContext httpContext,
        ILookupService lookupService,
        ILogger<JJLookup> logger) : base(httpContext)
    {
        LookupService = lookupService;
        Logger = logger;
        Enabled = true;
        AutoReloadFormFields = true;
        Name = "jjlookup1";
        PageState = PageState.List;
        PopSize = PopupSize.Full;
        PopTitle = "Search";
    }


    #endregion

    internal override HtmlBuilder RenderHtml()
    {
        return RenderHtmlAsync().GetAwaiter().GetResult();
    }

    protected override async Task<HtmlBuilder> RenderHtmlAsync()
    {
        if (!IsLookupRoute())
            return await GetLookupHtmlElement();

        if (IsAjaxGetDescription())
            await SendResult();
        else
            SendLookupUrlDto();

        return null;
    }

    private async Task<HtmlBuilder> GetLookupHtmlElement()
    {
        string inputValue = SelectedValue;

        string description = Text;

        if (string.IsNullOrEmpty(description) && !string.IsNullOrEmpty(inputValue))
            description = await LookupService.GetDescriptionAsync(DataItem,inputValue,PageState,FormValues,OnlyNumbers);

        var div = new HtmlBuilder(HtmlTag.Div);

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

        div.AppendElement(textGroup);
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

    public void SendLookupUrlDto()
    {
        LookupUrlDto dto = LookupService.GetLookupUrlDto(DataItem,Name,PageState,FormValues);

        CurrentContext.Response.SendResponse(dto.ToJson(), "application/json");
    }



    private async Task SendResult()
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
            Logger.LogError(ex, ex.Message);
        }

        CurrentContext.Response.SendResponse(dto?.ToJson(), "application/json");
    }

    /// <summary>
    /// Recovers the description based on the selected value
    /// </summary>
    /// <returns>Returns the description of the id</returns>
    public async Task<string> GetDescriptionAsync()
    {
        return await LookupService.GetDescriptionAsync(DataItem, SelectedValue, PageState, FormValues, OnlyNumbers);
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

    public static bool IsLookupRoute(JJBaseView view, IHttpContext context)
    {
        string dataPanelName = string.Empty;
        if (view is JJFormView formView)
            dataPanelName = formView.DataPanel.Name;
        else if (view is JJDataPanel dataPanel)
            dataPanelName = dataPanel.Name;

        string lookupRoute = context.Request.QueryString("jjlookup_" + dataPanelName);
        return !string.IsNullOrEmpty(lookupRoute);
    }

    public static HtmlBuilder ResponseRoute(JJDataPanel view)
    {
        string lookupRoute = view.CurrentContext.Request.QueryString("jjlookup_" + view.Name);
        if (string.IsNullOrEmpty(lookupRoute)) 
            return null;

        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(lookupRoute));
        if (field == null) 
            return null;
        var lookup = view.ControlFactory.Create(view.FormElement,field, null, view.Values, view.PageState, view.Name);
        return lookup.GetHtmlBuilder();

    }
}