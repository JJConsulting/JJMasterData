using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Controls;
using JJMasterData.Core.UI.Components.Importation;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public class JJTextFile : ControlBase
{
    private IDictionary<string, object> _formValues;
    private FormFilePathBuilder _pathBuilder;
    private RouteContext _routeContext;
    private TextFileScripts _scripts;
    private JJUploadView _uploadView;
    private IComponentFactory<JJUploadView> UploadViewFactory { get; }
    private IControlFactory<JJTextGroup> TextBoxFactory { get; }
    internal IEncryptionService EncryptionService { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private IComponentFactory<JJFileDownloader> FileDownloaderFactory { get; }


    public string FieldName { get; set; }
    
    public IDictionary<string, object> FormValues
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
            
            _uploadView = UploadViewFactory.Create();
            _uploadView.Name = $"{FormElementField.Name}-upload-view";
            _uploadView.Title = string.Empty;
            _uploadView.AutoSave = false;
            _uploadView.ShowAddFiles = PageState is not PageState.View;
            _uploadView.JsCallback = Scripts.GetShowScript();
            _uploadView.RenameAction.SetVisible(true);
            
            if (HasPk())
                _uploadView.FolderPath = GetFolderPath();
            
            _uploadView.GridView.ShowToolbar = false;
            
            var dataFile = FormElementField.DataFile!;
            _uploadView.UploadArea.Multiple = dataFile.MultipleFile;
            _uploadView.UploadArea.MaxFileSize = dataFile.MaxFileSize;
            _uploadView.UploadArea.ShowFileSize = dataFile.ExportAsLink;
            _uploadView.UploadArea.AllowedTypes = dataFile.AllowedTypes;
            _uploadView.UploadArea.RouteContext.ComponentContext = ComponentContext.TextFileFileUpload;
            _uploadView.UploadArea.QueryStringParams["fieldName"] = FieldName;
            
            _uploadView.ViewGallery = dataFile.ViewGallery;

            if (!Enabled)
                _uploadView.Disable();

            return _uploadView;
        }
    }

    

    public JJTextFile(
        IHttpRequest httpRequest,
        IComponentFactory<JJUploadView> uploadViewFactory,
        IControlFactory<JJTextGroup> textBoxFactory,
        IStringLocalizer<JJMasterDataResources> stringLocalizer, 
        IComponentFactory<JJFileDownloader> fileDownloaderFactory,
        IEncryptionService encryptionService) : base(httpRequest)
    {
        UploadViewFactory = uploadViewFactory;
        TextBoxFactory = textBoxFactory;
        StringLocalizer = stringLocalizer;
        FileDownloaderFactory = fileDownloaderFactory;
        EncryptionService = encryptionService;
    }

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        switch (RouteContext.ComponentContext)
        {
            case ComponentContext.TextFileUploadView:
                return await GetUploadViewResultAsync();
            case ComponentContext.TextFileFileUpload:
                return await UploadView.UploadArea.GetFileUploadResultAsync();
            default:
                return new RenderedComponentResult(await GetHtmlTextGroup());
        }
    }

    private async Task<ComponentResult> GetUploadViewResultAsync()
    {

        var html = new HtmlBuilder();

        var result = await UploadView.GetUploadViewResult();

        if (result is RenderedComponentResult uploadViewResult)
        {
            html.Append(uploadViewResult.HtmlBuilder);
            html.AppendScript(Scripts.GetRefreshScript());
        }
        else 
        {
            return result;
        }
        
        return HtmlComponentResult.FromHtmlBuilder(html);
    }

    private async Task<HtmlBuilder> GetHtmlTextGroup()
    {
        if (!Enabled)
            UploadView.ClearMemoryFiles();

        var textGroup = TextBoxFactory.Create();
        textGroup.CssClass = CssClass;
        textGroup.Name = $"{Name}-presentation";
        textGroup.ReadOnly = true;
        textGroup.Tooltip = Tooltip;
        textGroup.Attributes = Attributes;
        textGroup.Text = GetPresentationText();

        var btn = new JJLinkButton
        {
            ShowAsButton = true,
            OnClientClick = Scripts.GetShowScript(),
            Tooltip = FormElementField.DataFile!.MultipleFile ? StringLocalizer["Manage Files"] : StringLocalizer["Manage File"],
            IconClass = IconType.Paperclip.GetCssClass()
        };
        
        textGroup.Actions.Add(btn);

        var textGroupHtml = await textGroup.GetHtmlBuilderAsync();
        
        var html = new HtmlBuilder(HtmlTag.Div)
            .Append(textGroupHtml)
            .Append(HtmlTag.Input, i =>
            {
                i.WithAttribute("type", "hidden")
                    .WithNameAndId(Name)
                    .WithAttribute("value", GetFileName());
            });

        return html;
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

        return pkFields.All(pkField => FormValues.ContainsKey(pkField.Name) && !string.IsNullOrEmpty(FormValues[pkField.Name]?.ToString()));
    }

    public string GetFolderPath()
    {
        return PathBuilder.GetFolderPath(FormElementField, FormValues);
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

        var btnGroup = new JJLinkButtonGroup
        {
            CaretText = $"{files.Length}&nbsp;{StringLocalizer["Files"]}"
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
        var btn = new JJLinkButton
        {
            IconClass = IconType.CloudDownload.GetCssClass(),
            Text = filename
        };
        btn.Attributes.Add("onclick", "event.stopPropagation()");
        btn.UrlAction = GetDownloadLink(filename);
        btn.IsGroup = true;

        return btn;
    }
    public string GetDownloadLink(string fileName, bool isExternalLink = false)
    {
        var filePath = GetFolderPath() + fileName;

        var fileDownloader = FileDownloaderFactory.Create();
        
        return fileDownloader.GetDownloadUrl(filePath);
    }
}