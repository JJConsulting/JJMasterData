using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class JJTextFile(IHttpRequest request,
        IComponentFactory componentFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IEncryptionService encryptionService)
    : ControlBase(request.Form)
{
    private IDictionary<string, object> _formValues;
    private FormFilePathBuilder _pathBuilder;
    private RouteContext _routeContext;
    private TextFileScripts _scripts;
    private JJUploadView _uploadView;
    private IHttpRequest Request { get; } = request;
    private IComponentFactory ComponentFactory { get; } = componentFactory;
    internal IEncryptionService EncryptionService { get; } = encryptionService;
    internal IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    internal string ParentName { get; set; }
    public string FieldName { get; set; }
    
    public IDictionary<string, object> FormStateValues
    {
        get => _formValues ??= new Dictionary<string, object>();
        set => _formValues = value;
    }

    private TextFileScripts Scripts => _scripts ??= new TextFileScripts(this);

    public override string Tooltip
    {
        
        get => FormElementField.HelpDescription;
        set => FormElementField.HelpDescription = value;
    }

    public PageState PageState { get; set; }

    public FormElementField FormElementField { get; set; }

    public FormElement FormElement { get; set; }

    private FormFilePathBuilder PathBuilder => _pathBuilder ??= new FormFilePathBuilder(FormElement);
    
    private RouteContext RouteContext
    {
        get
        {
            if (_routeContext != null)
                return _routeContext;

            var factory = new RouteContextFactory(Request.QueryString, EncryptionService);
            _routeContext = factory.Create();
            
            return _routeContext;
        }
    }

    public JJUploadView UploadView
    {
        get
        {
            if (_uploadView is not null) 
                return _uploadView;
            
            _uploadView = ComponentFactory.UploadView.Create();
            _uploadView.Name = $"{FormElementField.Name}-upload-view";
            _uploadView.ParentName = FormElement.ParentName ?? ParentName;
            _uploadView.Title = string.Empty;
            _uploadView.AutoSave = false;
            _uploadView.JsCallback = Scripts.GetRefreshScript();
            _uploadView.RenameAction.SetVisible(true);
            
            if (HasPk())
                _uploadView.FolderPath = GetFolderPath();
            
            var dataFile = FormElementField.DataFile!;
            _uploadView.UploadArea.Multiple = dataFile.MultipleFile;
            _uploadView.UploadArea.MaxFileSize = dataFile.MaxFileSize;
            _uploadView.UploadArea.ShowFileSize = dataFile.ExportAsLink;
            _uploadView.UploadArea.AllowedTypes = dataFile.AllowedTypes;
            _uploadView.UploadArea.EnableCopyPaste = dataFile.AllowPasting;
            _uploadView.UploadArea.RouteContext.ElementName = FormElement.Name;
            _uploadView.UploadArea.RouteContext.ParentElementName = FormElement.ParentName;
            _uploadView.UploadArea.RouteContext.ComponentContext = ComponentContext.TextFileFileUpload;
            _uploadView.UploadArea.QueryStringParams["fieldName"] = FieldName;
            _uploadView.ViewGallery = dataFile.ViewGallery;

            if (dataFile.ShowAsUploadView)
                _uploadView.GridView.EmptyDataText = null;
            
            if (!Enabled || PageState is PageState.View)
                _uploadView.Disable();

            return _uploadView;
        }
    }

    protected override async Task<ComponentResult> BuildResultAsync()
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
                return new RenderedComponentResult(await GetRenderedComponentHtml());
        }
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

    private async Task<HtmlBuilder> GetRenderedComponentHtml()
    {
        if (FormElementField.DataFile!.ShowAsUploadView)
        {
            var uploadViewHtml = ((RenderedComponentResult)await GetUploadViewResultAsync()).HtmlBuilder;
            uploadViewHtml.Append(GetHiddenInputHtml());
            return uploadViewHtml;
        }
        
        if (!Enabled)
            UploadView.ClearMemoryFiles();

        var textGroup = ComponentFactory.Controls.TextGroup.Create();
        textGroup.CssClass = CssClass;
        textGroup.Name = $"{Name}-presentation";
        textGroup.ReadOnly = true;
        textGroup.Tooltip = Tooltip;
        textGroup.Attributes = Attributes;
        textGroup.Text = GetPresentationText();

        var button = ComponentFactory.Html.LinkButton.Create();
        button.ShowAsButton = true;
        button.OnClientClick = Scripts.GetShowScript();
        button.Tooltip = FormElementField.DataFile!.MultipleFile ? StringLocalizer["Manage Files"] : StringLocalizer["Manage File"];
        button.IconClass = IconType.Paperclip.GetCssClass();

        textGroup.Actions.Add(button);

        var html = new HtmlBuilder();
        html.Append(await textGroup.GetHtmlBuilderAsync());
        html.Append(GetHiddenInputHtml());
        
        return html;
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
        var pkFields = FormElement.Fields.ToList().FindAll(x => x.IsPk);
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

        var btnGroup = ComponentFactory.Html.LinkButtonGroup.Create();
        btnGroup.CaretText = $"{new JJIcon(IconType.CloudDownload).GetHtml()} {files.Length}&nbsp;{StringLocalizer["Files"]}";

        btnGroup.Attributes.Add("onclick","event.stopPropagation()");
   
        foreach (var filename in files)
        {
            btnGroup.Actions.Add(GetLinkButton(filename));
        }

        return btnGroup.GetHtmlBuilder();
    }

    private JJLinkButton GetLinkButton(string filename)
    {
        var btn = ComponentFactory.Html.LinkButton.Create();
        btn.IconClass = IconType.CloudDownload.GetCssClass();
        btn.Text = filename;
        btn.Attributes.Add("onclick", "event.stopPropagation()");
        btn.UrlAction = GetDownloadLink(filename);
        btn.IsGroup = true;

        return btn;
    }

    private string GetDownloadLink(string fileName)
    {
        var filePath = GetFolderPath() + fileName;
        var fileDownloader = ComponentFactory.Downloader.Create();
        fileDownloader.FilePath = filePath;
        return fileDownloader.GetDownloadUrl();
    }
}