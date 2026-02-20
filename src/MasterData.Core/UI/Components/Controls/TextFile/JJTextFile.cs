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
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.Http.Abstractions;

using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public sealed class JJTextFile(IHttpRequest request,
        IComponentFactory componentFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IEncryptionService encryptionService)
    : ControlBase(request.Form)
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

    private FormFilePathBuilder PathBuilder => field ??= new FormFilePathBuilder(FormElement);
    
    private RouteContext RouteContext
    {
        get
        {
            if (field != null)
                return field;

            var factory = new RouteContextFactory(request.QueryString, EncryptionService);
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

            if (dataFile.ShowAsUploadView)
                field.GridView.EmptyDataText = null;
            
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
            uploadViewHtml.Append(GetHiddenInputHtml());
            return uploadViewHtml;
        }
        
        if (!Enabled)
            UploadView.ClearMemoryFiles();

        var textGroup = componentFactory.Controls.TextGroup.Create();
        textGroup.CssClass = CssClass;
        textGroup.Name = $"{Name}-presentation";
        textGroup.ReadOnly = true;
        textGroup.Tooltip = Tooltip;
        textGroup.Attributes = Attributes;
        textGroup.Text = GetPresentationText();

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
        html.Append(GetHiddenInputHtml());
        
        return html;
    }

    private async Task<ComponentResult> GetUploadViewResultAsync()
    {
        var result = await UploadView.GetUploadViewResult();

        if (result is not RenderedComponentResult uploadViewResult)
            return result;
        
        var html = new HtmlBuilder();
        html.Append(uploadViewResult.HtmlBuilder);
        html.AppendScript(Scripts.GetRefreshInputsScript());
        return new RenderedComponentResult(html);
    }

    private HtmlBuilder GetHiddenInputHtml()
    {
        var input = new HtmlBuilder(HtmlTag.Input);
        input.WithAttribute("type", "hidden")
            .WithNameAndId(Name)
            .WithAttribute("value", GetFileName());
        return input;
    }

    public void SaveMemoryFiles()
    {
        string folderPath = GetFolderPath();
        var uploadView = UploadView;
        uploadView.SaveMemoryFiles(folderPath);
    }

    public void DeleteAll()
    {
        var uploadView = UploadView;
        uploadView.FolderPath = GetFolderPath();
        uploadView.DeleteAll();
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
        return PathBuilder.GetFolderPath(FormElementField, FormStateValues);
    }

    internal string GetFileName()
    {
        string fileNames = string.Empty;
        var listFile = UploadView.GetFiles().FindAll(x => !x.Deleted);
        foreach (var file in listFile)
        {
            if (fileNames != string.Empty)
                fileNames += ",";

            fileNames += file.Content.FileName;
        }

        return fileNames;
    }

    internal string GetPresentationText()
    {
        var files = UploadView.GetFiles().FindAll(x => !x.Deleted);

        return files.Count switch
        {
            0 => string.Empty,
            1 => files[0].Content.FileName,
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
        var btn = new JJLinkButton();
        btn.IconClass = "fa fa-cloud-download";
        btn.Text = filename;
        btn.Attributes.Add("onclick", "event.stopPropagation()");
        btn.UrlAction = GetDownloadLink(filename);
        btn.IsGroup = true;

        return btn;
    }

    private string GetDownloadLink(string fileName)
    {
        var filePath = GetFolderPath() + fileName;
        var fileDownloader = componentFactory.Downloader.Create();
        fileDownloader.FilePath = filePath;
        return fileDownloader.GetDownloadUrl();
    }
}