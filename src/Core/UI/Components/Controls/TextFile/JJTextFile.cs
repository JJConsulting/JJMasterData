using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public class JJTextFile : JJBaseControl
{
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IComponentFactory<JJUploadView> UploadViewFactory { get; }
    private IControlFactory<JJTextGroup> TextBoxFactory { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private const string UploadViewParameterName = "jjuploadview_";
    private IDictionary<string, dynamic> _formValues;
    private FormFilePathBuilder _pathBuiler;

    public IDictionary<string, dynamic> FormValues
    {
        get => _formValues ??= new Dictionary<string, dynamic>();
        set => _formValues = value;
    }

    public new string ToolTip
    {
        get => FormElementField.HelpDescription;
        set => FormElementField.HelpDescription = value;
    }

    public PageState PageState { get; set; }

    public FormElementField FormElementField { get; set; }

    public FormElement FormElement { get; set; }

    internal FormFilePathBuilder PathBuilder => _pathBuiler ??= new FormFilePathBuilder(FormElement);


    public JJTextFile(
        IHttpContext currentContext,
        JJMasterDataUrlHelper urlHelper,
        IComponentFactory<JJUploadView> uploadViewFactory,
        IControlFactory<JJTextGroup> textBoxFactory,
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer) : base(currentContext)
    {
        UrlHelper = urlHelper;
        UploadViewFactory = uploadViewFactory;
        TextBoxFactory = textBoxFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }

    internal override HtmlBuilder RenderHtml()
    {
        if (IsFormUploadRoute())
            return GetFormUploadHtmlBuilder();

        return GetHtmlTextGroup();
    }

    internal HtmlBuilder GetFormUploadHtmlBuilder()
    {
        //Ao abrir uma nova pagina no iframe o "jumi da india" não conseguiu fazer o iframe via post 
        //por esse motivo passamos os valores nessários do form anterior por parametro o:)
        LoadValuesFromQuery();

        var formUpload = GetFormUpload();

        var html = new HtmlBuilder();
        html.AppendComponent(formUpload);
        html.AppendScript(GetRefreshScript(formUpload));
        return html;
    }

    private HtmlBuilder GetHtmlTextGroup()
    {
        var formUpload = GetFormUpload();

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
            OnClientClick = GetUploadViewAction(),
            ToolTip = "Manage Files",
            IconClass = IconType.Paperclip.GetCssClass()
        };
        textGroup.Actions.Add(btn);

        var html = new HtmlBuilder(HtmlTag.Div)
            .AppendComponent(textGroup)
            .Append(HtmlTag.Input, i =>
            {
                i.WithAttribute("type", "hidden")
                    .WithNameAndId(Name)
                    .WithAttribute("value", GetFileName(formUpload));
            });

        return html;
    }

    private string GetRefreshScript(JJUploadView uploadView)
    {
        return $$"""
            $(document).ready(function () {
                window.parent.$("#v_{{Name}}").val("{{GetPresentationText(uploadView)}}");
                window.parent.$("#{{Name}}").val("{{GetFileName(uploadView)}}");
            });
        """;
    }

    private string GetUploadViewAction()
    {
        var parms = new UploadViewParams
        {
            PageState = PageState,
            Enable = Enabled && !ReadOnly
        };

        if (PageState != PageState.Insert)
            parms.PkValues = DataHelper.ParsePkValues(FormElement, FormValues, '|');

        var json = JsonConvert.SerializeObject(parms);
        var values = EncryptionService.EncryptStringWithUrlEncode(json);

        var title = FormElementField.Label;
        title = title == null ? "Manage Files" : title.Replace('\'', '`').Replace('\"', ' ');

        title = StringLocalizer[title];

        var url = string.Empty;

        if (IsExternalRoute)
        {
            var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEncode(FormElement.Name);
            url = UrlHelper.GetUrl("GetUploadView", "TextFile",
                new
                {
                    dictionaryName = encryptedDictionaryName, 
                    fieldName = FormElementField.Name, 
                    componentName = Name,
                    uploadViewParams = values
                });
        }

        return $"UploadView.open('{Name}','{title}','{values}', '{url}');";
    }

    private void LoadValuesFromQuery()
    {
        var uploadViewParams = CurrentContext.Request.QueryString("uploadViewParams");
        if (string.IsNullOrEmpty(uploadViewParams))
            throw new ArgumentNullException(nameof(uploadViewParams));

        var json = EncryptionService.DecryptStringWithUrlDecode(uploadViewParams);
        var @params = JsonConvert.DeserializeObject<UploadViewParams>(json);
        if (@params == null)
            throw new JJMasterDataException("Invalid parameters when opening file upload");

        PageState = @params.PageState;
        Enabled = @params.Enable;

        if (!string.IsNullOrEmpty(@params.PkValues))
        {
            var values = @params.PkValues.Split('|');
            var pkFields = FormElement.Fields.ToList().FindAll(x => x.IsPk);
            for (int i = 0; i < pkFields.Count; i++)
            {
                FormValues[pkFields[i].Name] = values[i];
            }
        }
    }

    public void SaveMemoryFiles()
    {
        string folderPath = GetFolderPath();
        JJUploadView uploadView = GetFormUpload();
        uploadView.SaveMemoryFiles(folderPath);
    }

    public void DeleteAll()
    {
        JJUploadView uploadView = GetFormUpload();
        uploadView.FolderPath = GetFolderPath();
        uploadView.DeleteAll();
    }

    private JJUploadView GetFormUpload()
    {
        var form = UploadViewFactory.Create();
        var dataFile = FormElementField.DataFile;
        form.Name = FormElementField.Name + "_uploadview"; //this is important
        form.Title = "";
        form.IsExternalRoute = IsExternalRoute;
        form.AutoSave = false;
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

        foreach (var pkField in pkFields)
        {
            if (!FormValues.ContainsKey(pkField.Name))
                return false;

            string value = FormValues[pkField.Name]!.ToString();
            if (!Validate.ValidFileName(value))
                return false;
        }

        return true;
    }

    public string GetFolderPath()
    {
        return PathBuilder.GetFolderPath(FormElementField, FormValues);
    }

    private string GetPanelName()
    {
        string pnlName = string.Empty;
        if (Attributes.TryGetValue("pnlname", out var attribute))
            pnlName = attribute?.ToString();

        return pnlName;
    }

    private string GetFileName(JJUploadView uploadView)
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

    private string GetPresentationText(JJUploadView uploadView)
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

        foreach (var filename in files)
        {
            btnGroup.Actions.Add(GetLinkButton(filename));
        }

        return btnGroup.GetHtmlBuilder();
    }

    private JJLinkButton GetLinkButton(string filename)
    {
        var btn = new JJLinkButton();
        btn.IconClass = IconType.CloudDownload.GetCssClass();
        btn.Text = filename;
        btn.Attributes.Add("onclick", "event.stopPropagation()");
        btn.UrlAction = GetDownloadLink(filename);
        btn.IsGroup = true;

        return btn;
    }

    //AbsoluteUri needs to be via parameter here, because a external thread on exportation don't have access to Context.
    public string GetDownloadLink(string fileName, bool isExternalLink = false, string absoluteUri = null)
    {
        string filePath = GetFolderPath() + fileName;
        string url = absoluteUri ?? HttpContext.Current.Request.Url.AbsoluteUri;
        if (url.Contains('?'))
            url += "&";
        else
            url += "?";

        if (isExternalLink)
            url += JJFileDownloader.DirectDownloadParameter;
        else
            url += JJFileDownloader.DownloadParameter;

        url += "=";
        url += EncryptionService.EncryptStringWithUrlEncode(filePath);

        return url;
    }

    private bool IsFormUploadRoute()
    {
        string pnlName = GetPanelName();
        string lookupRoute = CurrentContext.Request.QueryString(UploadViewParameterName + pnlName);
        return Name.Equals(lookupRoute);
    }

    public static bool IsFormUploadRoute(JJBaseView view, IHttpContext httpContext)
    {
        string dataPanelName;
        if (view is JJFormView formView)
            dataPanelName = formView.DataPanel.Name;
        else if (view is JJDataPanel dataPanel)
            dataPanelName = dataPanel.Name;
        else
            dataPanelName = string.Empty;

        return httpContext.Request.QueryString(UploadViewParameterName + dataPanelName) != null;
    }

    public static HtmlBuilder ResponseRoute(JJDataPanel view)
    {
        string uploadFormRoute = view.CurrentContext.Request.QueryString(UploadViewParameterName + view.Name);
        if (uploadFormRoute == null)
            return null;

        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(uploadFormRoute));
        if (field == null)
            return null;
        ;

        var upload = view.ControlFactory.CreateAsync(view.FormElement, field, null, view.Values, view.PageState, view.Name).GetAwaiter().GetResult();
        return upload.GetHtmlBuilder();
    }
}