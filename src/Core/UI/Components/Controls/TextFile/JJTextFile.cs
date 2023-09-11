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
    
    private IComponentFactory<JJUploadView> UploadViewFactory { get; }
    private IControlFactory<JJTextGroup> TextBoxFactory { get; }
    internal IEncryptionService EncryptionService { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    
    public string FieldName { get; set; }
    
    public IDictionary<string, object> FormValues
    {
        get => _formValues ??= new Dictionary<string, object>();
        set => _formValues = value;
    }

    private TextFileScripts Scripts => _scripts ??= new TextFileScripts(this);

    public override string ToolTip
    {
        get => FormElementField.HelpDescription;
        set => FormElementField.HelpDescription = value;
    }

    public PageState PageState { get; set; }

    public FormElementField FormElementField { get; set; }

    public FormElement FormElement { get; set; }

    internal FormFilePathBuilder PathBuilder => _pathBuilder ??= new FormFilePathBuilder(FormElement);

    
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


    public JJTextFile(
        IHttpRequest httpRequest,
        IComponentFactory<JJUploadView> uploadViewFactory,
        IControlFactory<JJTextGroup> textBoxFactory,
        IStringLocalizer<JJMasterDataResources> stringLocalizer, IEncryptionService encryptionService) : base(httpRequest)
    {
        UploadViewFactory = uploadViewFactory;
        TextBoxFactory = textBoxFactory;
        StringLocalizer = stringLocalizer;
        EncryptionService = encryptionService;
    }

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        if (IsUploadViewRoute())
            return await GetUploadViewResultAsync();

        return new RenderedComponentResult(await GetHtmlTextGroup());
    }

    internal async Task<ComponentResult> GetUploadViewResultAsync()
    {

        var uploadView = GetUploadView();

        var html = new HtmlBuilder();

        var result = await uploadView.GetResultAsync();

        if (result is RenderedComponentResult renderedUpload)
        {
            html.Append(renderedUpload.HtmlBuilder);
            html.AppendScript(Scripts.GetRefreshScript(uploadView));
        }
        else
        {
            return result;
        }
        
        return HtmlComponentResult.FromHtmlBuilder(html);
    }

    private async Task<HtmlBuilder> GetHtmlTextGroup()
    {
        var formUpload = GetUploadView();

        if (!Enabled)
            formUpload.ClearMemoryFiles();

        var textGroup = TextBoxFactory.Create();
        textGroup.CssClass = CssClass;
        textGroup.ReadOnly = true;
        textGroup.Name = $"v_{Name}";
        textGroup.ToolTip = ToolTip;
        textGroup.Attributes = Attributes;
        textGroup.Text = GetPresentationText(formUpload);

        var btn = new JJLinkButton
        {
            ShowAsButton = true,
            OnClientClick = Scripts.GetShowScript(),
            ToolTip = "Manage Files",
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
                    .WithAttribute("value", GetFileName(formUpload));
            });

        return html;
    }

    public void SaveMemoryFiles()
    {
        string folderPath = GetFolderPath();
        var uploadView = GetUploadView();
        uploadView.SaveMemoryFiles(folderPath);
    }

    public void DeleteAll()
    {
        var uploadView = GetUploadView();
        uploadView.FolderPath = GetFolderPath();
        uploadView.DeleteAll();
    }

    private JJUploadView GetUploadView()
    {
        var form = UploadViewFactory.Create();
        var dataFile = FormElementField.DataFile!;
        form.Name = FormElementField.Name + "-upload-view";
        form.Title = string.Empty;
        form.AutoSave = false;
        form.ShowAddFiles = PageState is not PageState.View;
        form.Upload.JsCallback = Scripts.GetShowScript();
        form.GridView.ShowToolbar = false;
        form.RenameAction.SetVisible(true);
        form.Upload.Multiple = dataFile.MultipleFile;
        form.Upload.MaxFileSize = dataFile.MaxFileSize;
        form.Upload.ShowFileSize = dataFile.ExportAsLink;
        form.Upload.AllowedTypes = dataFile.AllowedTypes;
        form.ViewGallery = dataFile.ViewGallery;

        if (HasPk())
            form.FolderPath = GetFolderPath();

        if (!Enabled)
            form.Disable();

        return form;
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

    internal static string GetFileName(JJUploadView uploadView)
    {
        string fileNames = string.Empty;
        var listFile = uploadView.GetFiles().FindAll(x => !x.Deleted);
        foreach (var file in listFile)
        {
            if (fileNames != string.Empty)
                fileNames += ",";

            fileNames += file.Content.FileName;
        }

        return fileNames;
    }

    internal string GetPresentationText(JJUploadView uploadView)
    {
        var files = uploadView.GetFiles().FindAll(x => !x.Deleted);

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
        var url = Request.AbsoluteUri;
        if (url.Contains('?'))
            url += "&";
        else
            url += "?";

        if (isExternalLink)
            url += JJFileDownloader.DirectDownloadParameter;
        else
            url += JJFileDownloader.DownloadParameter;

        url += "=";
        url += EncryptionService.EncryptStringWithUrlEscape(filePath);

        return url;
    }

    private bool IsUploadViewRoute()
    {
        return RouteContext.ComponentContext is ComponentContext.FileUpload || RouteContext.ComponentContext is ComponentContext.TextFile &&
               RouteContext.IsCurrentFormElement(FormElement.Name);
    }
}