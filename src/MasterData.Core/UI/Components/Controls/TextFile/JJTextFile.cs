using System.Collections.Generic;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public sealed class JJTextFile(
    IHttpContextAccessor request,
    IComponentFactory componentFactory,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    IEncryptionService encryptionService)
    : ControlBase(request)
{
    internal IEncryptionService EncryptionService { get; } = encryptionService;
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    internal string ParentName { get; set; }
    public string FieldName { get; set; }

    public Dictionary<string, object> FormStateValues
    {
        get => field ??= new Dictionary<string, object>();
        set;
    }

    private TextFileScripts Scripts => field ??= new TextFileScripts(this);

    public override string Tooltip
    {
        get => FormElementField.HelpDescription;
        set => FormElementField.HelpDescription = value;
    }

    public PageState PageState { get; set; }

    public FormElementField FormElementField { get; set; }

    public FormElement FormElement { get; set; }

    private RouteContext RouteContext
    {
        get
        {
            if (field != null)
                return field;

            var factory = new RouteContextFactory(request, EncryptionService);
            field = factory.Create();

            return field;
        }
    }

    public JJUploadView UploadView
    {
        get
        {
            if (field is not null)
                return field;

            field = componentFactory.UploadView.Create(FormElement, FormElementField, FormStateValues);
            field.ParentName = FormElement.ParentName ?? ParentName;
            field.JsCallback = Scripts.GetRefreshScript();

            if (!Enabled || PageState is PageState.View)
                field.Disable();

            return field;
        }
    }

    protected override async ValueTask<ComponentResult> BuildResultAsync()
    {
        switch (RouteContext.ComponentContext)
        {
            case ComponentContext.TextFileUploadView:
            {
                var result = await GetUploadViewResultAsync();

                if (result is HtmlComponentResult htmlComponentResult)
                    return new ContentComponentResult(htmlComponentResult.HtmlBuilder);

                return result;
            }
            case ComponentContext.TextFileFileUpload:
                return await UploadView.UploadArea.GetResultAsync();
            default:
                return new RenderedComponentResult(await GetHtmlBuilderAsync());
        }
    }

    protected internal override async ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        if (FormElementField.DataFile!.ShowAsUploadView)
        {
            var uploadViewHtml = ((RenderedComponentResult)await GetUploadViewResultAsync()).HtmlBuilder;
            uploadViewHtml.Append(await GetHiddenInputsHtmlAsync());
            return uploadViewHtml;
        }

        if (!Enabled)
            await UploadView.ClearTemporaryFilesAsync();

        var textGroup = componentFactory.Controls.TextGroup.Create();
        textGroup.CssClass = CssClass;
        textGroup.Name = $"{Name}-presentation";
        textGroup.ReadOnly = true;
        textGroup.Tooltip = Tooltip;
        textGroup.Attributes = Attributes;
        textGroup.Text = await GetPresentationTextAsync();

        var button = new JJLinkButton
        {
            ShowAsButton = true,
            OnClientClick = Scripts.GetShowScript(),
            Tooltip = FormElementField.DataFile!.MultipleFile ? StringLocalizer["Manage Files"] : StringLocalizer["Manage File"],
            IconClass = "fa fa-paperclip"
        };

        textGroup.Actions.Add(button);

        var html = new HtmlBuilder();
        html.Append(await textGroup.GetHtmlBuilderAsync());
        html.Append(await GetHiddenInputsHtmlAsync());
        html.Append(UploadView.GetHiddenInputsHtmlAsync());
        
        return html;
    }

    private async Task<ComponentResult> GetUploadViewResultAsync()
    {
        var result = await UploadView.GetUploadViewResult(appendHiddenInputs: false);

        if (result is not RenderedComponentResult uploadViewResult)
            return result;
        
        var html = new HtmlBuilder();
        html.Append(uploadViewResult.HtmlBuilder);
        html.AppendScript(await Scripts.GetRefreshInputsScriptAsync());
        return new RenderedComponentResult(html);
    }

    private async Task<HtmlBuilder> GetHiddenInputsHtmlAsync()
    {
        var fileName = await GetFileNameAsync();
        var html = new HtmlBuilder();
        var fileInput = new HtmlBuilder(HtmlTag.Input);
        fileInput.WithAttribute("type", "hidden")
            .WithNameAndId(Name)
            .WithAttribute("value", fileName);
        html.Append(fileInput);

        return html;
    }

    public string GetFolderPath()
    {
        return FileStoragePath.GetFolderPath(FormElement, FormElementField, FormStateValues);
    }

    internal string GetDraftInputName() => $"{FormElementField.Name}-upload-view-files-draft-id";

    internal async Task<string> GetFileNameAsync()
    {
        var fileNames = string.Empty;
        var files = await UploadView.GetFilesAsync();

        foreach (var file in files)
        {
            if (fileNames != string.Empty)
                fileNames += ",";

            fileNames += file.FileName;
        }

        return fileNames;
    }

    internal async Task<string> GetPresentationTextAsync()
    {
        var files = await UploadView.GetFilesAsync();

        return files.Count switch
        {
            0 => string.Empty,
            1 => files[0].FileName,
            _ => StringLocalizer["{0} Selected Files", files.Count]
        };
    }

    internal HtmlBuilder GetButtonGroupHtml()
    {
        if (string.IsNullOrEmpty(Text))
            return new HtmlBuilder(string.Empty);

        var files = Text.Split(',');
        if (files.Length == 1)
            return GetLinkButton(files[0]).GetHtmlBuilder();

        var caretHtml = new HtmlBuilder();
        caretHtml.AppendComponent(new JJIcon(FontAwesomeIcon.CloudDownload)).WithCssClass("me-1");
        caretHtml.AppendText($"{files.Length} {StringLocalizer["Files"]}");

        var btnGroup = new JJLinkButtonGroup
        {
            CaretHtml = caretHtml
        };

        btnGroup.Attributes.Add("onclick", "event.stopPropagation()");

        foreach (var fileName in files)
        {
            btnGroup.Actions.Add(GetLinkButton(fileName));
        }

        return btnGroup.GetHtmlBuilder();
    }

    private JJLinkButton GetLinkButton(string fileName)
    {
        return new JJLinkButton
        {
            IconClass = "fa fa-cloud-download",
            Text = fileName,
            UrlAction = GetDownloadLink(fileName),
            IsGroup = true,
            Attributes =
            {
                { "onclick", "event.stopPropagation()" }
            }
        };
    }

    private string GetDownloadLink(string fileName)
    {
        var fullPath = FileStoragePath.Combine(UploadView.FolderPath, fileName);
        var fileDownloader = componentFactory.Downloader.Create(fullPath);
        return fileDownloader.GetDownloadUrl();
    }
}
