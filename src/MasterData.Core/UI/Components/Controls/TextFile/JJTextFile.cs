using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Storage;
using Microsoft.AspNetCore.Http;

using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public sealed class JJTextFile(IHttpContextAccessor request,
        IComponentFactory componentFactory,
        IFileStorage fileStorage,
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
            
            field = componentFactory.UploadView.Create();
            field.Name = $"{FormElementField.Name}-upload-view";
            field.ParentName = FormElement.ParentName ?? ParentName;
            field.Title = string.Empty;
            field.AutoSave = false;
            field.JsCallback = Scripts.GetRefreshScript();
            field.RenameAction.SetVisible(true);
            
            if (HasPk())
                field.FolderPath = GetFolderPath();
            
            var dataFile = FormElementField.DataFile!;
            field.UploadArea.Multiple = dataFile.MultipleFile;
            field.UploadArea.MaxFileSize = dataFile.MaxFileSize;
            field.UploadArea.ShowFileSize = dataFile.ExportAsLink;
            field.UploadArea.AllowedTypes = dataFile.AllowedTypes;
            field.UploadArea.EnableCopyPaste = dataFile.AllowPasting;
            field.UploadArea.RouteContext.ElementName = FormElement.Name;
            field.UploadArea.RouteContext.ParentElementName = FormElement.ParentName;
            field.UploadArea.RouteContext.ComponentContext = ComponentContext.TextFileFileUpload;
            field.UploadArea.QueryStringParams["fieldName"] = FieldName;
            field.ViewGallery = dataFile.ViewGallery;
            
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
        
        return html;
    }

    private async Task<ComponentResult> GetUploadViewResultAsync()
    {
        var result = await UploadView.GetUploadViewResult();

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
        var draftId = UploadView.DraftId;
        var html = new HtmlBuilder();
        html.Append(HtmlTag.Input, input =>
        {
            input.WithAttribute("type", "hidden")
            .WithNameAndId(Name)
            .WithAttribute("value", fileName);
        });
        html.Append(HtmlTag.Input, input =>
        {
            input.WithAttribute("type", "hidden")
                .WithNameAndId(GetDraftInputName())
                .WithAttribute("value", draftId);
        });
        return html;
    }

    public Task PromoteTemporaryFilesAsync()
    {
        var uploadView = UploadView;
        return uploadView.PromoteTemporaryFilesAsync(GetFolderPath());
    }

    public Task DeleteAllAsync()
    {
        var uploadView = UploadView;
        uploadView.FolderPath = GetFolderPath();
        return uploadView.DeleteAllAsync();
    }
    private bool HasPk()
    {
        var pkFields = FormElement.Fields.FindAll(x => x.IsPk);
        if (pkFields.Count == 0)
            return false;

        return pkFields.All(pkField => FormStateValues.ContainsKey(pkField.Name) && !string.IsNullOrEmpty(FormStateValues[pkField.Name]?.ToString()));
    }

    public string GetFolderPath()
    {
        return fileStorage.GetFolderPath(FormElement, FormElementField, FormStateValues);
    }

    internal string GetDraftInputName() => $"{FormElementField.Name}-upload-view-files-draft-id";

    internal async Task<string> GetFileNameAsync()
    {
        string fileNames = string.Empty;
        var listFile = await UploadView.GetFilesAsync();
        foreach (var file in listFile)
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

        string[] files = Text.Split(',');
        if (files.Length == 1)
        {
            var btn = GetLinkButton(files[0]);
            return btn.GetHtmlBuilder();
        }

        var caretHtml = new HtmlBuilder();
        caretHtml.AppendComponent(new JJIcon(FontAwesomeIcon.CloudDownload)).WithCssClass("me-1");
        caretHtml.AppendText($"{files.Length} {StringLocalizer["Files"]}");
        var btnGroup = new JJLinkButtonGroup
        {
            CaretHtml = caretHtml
        };

        btnGroup.Attributes.Add("onclick","event.stopPropagation()");
   
        foreach (var filename in files)
        {
            btnGroup.Actions.Add(GetLinkButton(filename));
        }

        return btnGroup.GetHtmlBuilder();
    }

    private JJLinkButton GetLinkButton(string filename)
    {
        return new JJLinkButton
        {
            IconClass = "fa fa-cloud-download",
            Text = filename,
            UrlAction = GetDownloadLink(filename),
            IsGroup = true,
            Attributes =
            {
                {"onclick", "event.stopPropagation()"}
            }
        };
    }

    private string GetDownloadLink(string fileName)
    {
        var fileDownloader = componentFactory.Downloader.Create();
        fileDownloader.File = new FileStorageItemKey(GetFolderPath(), fileName, false);
        return fileDownloader.GetDownloadUrl();
    }
}
