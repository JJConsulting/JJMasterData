using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Html;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Web;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Core.WebComponents;

public class JJTextFile : JJBaseControl
{
    private const string UploadFormParameterName = "jjuploadform_";
    private Hashtable _formValues;
    private FormFilePathBuilder _pathBuiler;
    
    public Hashtable FormValues
    {
        get => _formValues ??= new Hashtable();
        set => _formValues = value;
    }

    public new string ToolTip
    {
        get => ElementField.HelpDescription;
        set => ElementField.HelpDescription = value;
    }

    public PageState PageState { get; set; }

    public FormElementField ElementField { get; set; }

    public FormElement FormElement { get; set; }

    internal FormFilePathBuilder PathBuilder
    {
        get
        {
            if (_pathBuiler == null)
                _pathBuiler = new FormFilePathBuilder(FormElement);

            return _pathBuiler;
        }
    }

    internal static JJTextFile GetInstance(FormElement formElement,
        FormElementField field, ExpressionOptions expOptions, object value, string panelName)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        if (field.DataFile == null)
            throw new ArgumentException(Translate.Key("Upload config not defined"), field.Name);

        var text = new JJTextFile
        {
            ElementField = field,
            PageState = expOptions.PageState,
            Text = value != null ? value.ToString() : "",
            FormValues = expOptions.FormValues,
            Name = field.Name,
        };

        text.Attributes.Add("pnlname", panelName);
        text.UserValues = expOptions.UserValues;
        text.FormElement = formElement;

        text.SetAttr(field.Attributes);

        return text;
    }

    internal override HtmlBuilder RenderHtml()
    {
        if (IsFormUploadRoute())
        {
            //Ao abrir uma nova pagina no iframe o "jumi da india" não conseguiu fazer o iframe via post 
            //por esse motivo passamos os valores nessários do form anterior por parametro o:)
            LoadDirectValues();
            var formUpload = GetFormUpload();

            var html = new HtmlBuilder();
            html.AppendElement(formUpload);
            html.AppendScript(GetRefreshScript(formUpload));
            return html;
        }
        else
        {
            return GetHtmlTextGroup();
        }
    }

    private HtmlBuilder GetHtmlTextGroup()
    {
        var formUpload = GetFormUpload();

        if (!Enabled)
            formUpload.ClearMemoryFiles();

        var textGroup = new JJTextGroup();
        textGroup.CssClass = CssClass;
        textGroup.ReadOnly = true;
        textGroup.Name = $"v_{Name}";
        textGroup.ToolTip = ToolTip;
        textGroup.Attributes = Attributes;
        textGroup.Text = GetPresentationText(formUpload);

        var btn = new JJLinkButton();
        btn.ShowAsButton = true;
        btn.OnClientClick = GetOpenUploadFormAction();
        btn.ToolTip = "Manage Files";
        btn.IconClass = IconType.Paperclip.GetCssClass();
        textGroup.Actions.Add(btn);

        var html = new HtmlBuilder(HtmlTag.Div)
            .AppendElement(textGroup)
            .AppendElement(HtmlTag.Input, i =>
                {
                    i.WithAttribute("type", "hidden")
                     .WithNameAndId(Name)
                     .WithAttribute("value", GetFileName(formUpload));
                });

        return html;
    }

    private string GetRefreshScript(JJFormUpload formUpload)
    {
        return $$"""
            $(document).ready(function () {
                window.parent.$("#v_{{Name}}").val("{{GetPresentationText(formUpload)}}");
                window.parent.$("#{{Name}}").val("{{GetFileName(formUpload)}}");
            });
        """;
    }

    private string GetOpenUploadFormAction()
    {
        var parms = new OpenFormParms
        {
            PageState = PageState,
            Enable = Enabled & !ReadOnly
        };

        if (PageState != PageState.Insert)
            parms.PkValues = DataHelper.ParsePkValues(FormElement, FormValues, '|');

        string json = JsonConvert.SerializeObject(parms);
        string value = Cript.Cript64(json);

        string title = ElementField.Label;
        if (title == null)
            title = "Manage Files";
        else
            title = title.Replace('\'', '`').Replace('\"', ' ');

        title = Translate.Key(title);
        return $"jjview.openUploadForm('{Name}','{title}','{value}');";
    }

    private void LoadDirectValues()
    {
        string uploadvalues = CurrentContext.Request.QueryString("uploadvalues");
        if (string.IsNullOrEmpty(uploadvalues))
            throw new ArgumentNullException(nameof(uploadvalues));

        string json = Cript.Descript64(uploadvalues);
        var parms = JsonConvert.DeserializeObject<OpenFormParms>(json);
        if (parms == null)
            throw new JJMasterDataException(Translate.Key("Invalid parameters when opening file upload"));

        PageState = parms.PageState;
        Enabled = parms.Enable;

        if (!string.IsNullOrEmpty(parms.PkValues))
        {
            string[] values = parms.PkValues.Split('|');
            var pkFields = FormElement.Fields.ToList().FindAll(x => x.IsPk);
            for (int i = 0; i < pkFields.Count; i++)
            {
                if (FormValues.ContainsKey(pkFields[i].Name))
                    FormValues[pkFields[i].Name] = values[i];
                else
                    FormValues.Add(pkFields[i].Name, values[i]);
            }
        }
    }

    public void SaveMemoryFiles()
    {
        string folderPath = GetFolderPath();
        JJFormUpload formUpload = GetFormUpload();
        formUpload.SaveMemoryFiles(folderPath);
    }

    public void DeleteAll()
    {
        JJFormUpload formUpload = GetFormUpload();
        formUpload.FolderPath = GetFolderPath();
        formUpload.DeleteAll();
    }

    private JJFormUpload GetFormUpload()
    {
        var form = new JJFormUpload();
        var dataFile = ElementField.DataFile;
        form.Name = ElementField.Name + "_formupload"; //this is important
        form.Title = "";
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
        return PathBuilder.GetFolderPath(ElementField, FormValues);
    }

    private string GetPanelName()
    {
        string pnlName = string.Empty;
        if (Attributes.ContainsKey("pnlname"))
            pnlName = Attributes["pnlname"]?.ToString();

        return pnlName;
    }

    private string GetFileName(JJFormUpload formUpload)
    {
        string fileNames = string.Empty;
        var listFile = formUpload.GetFiles().FindAll(x => !x.Deleted);
        foreach (var file in listFile)
        {
            if (fileNames != string.Empty)
                fileNames += ",";

            fileNames += file.Content.FileName;
        }

        return fileNames;

    }

    private string GetPresentationText(JJFormUpload formUpload)
    {
        var files = formUpload.GetFiles().FindAll(x => !x.Deleted);

        return files.Count switch
        {
            0 => string.Empty,
            1 => files[0].Content.FileName,
            _ => Translate.Key("{0} Selected Files", files.Count)
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
            CaretText = $"{files.Length}&nbsp;{Translate.Key("Files")}"
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
            url += JJDownloadFile.DirectDownloadParameter;
        else
            url += JJDownloadFile.DownloadParameter;

        url += "=";
        url += Cript.Cript64(filePath);

        return url;
    }

    private bool IsFormUploadRoute()
    {
        string pnlName = GetPanelName();
        string lookupRoute = CurrentContext.Request.QueryString(UploadFormParameterName + pnlName);
        return Name.Equals(lookupRoute);
    }

    public static bool IsFormUploadRoute(JJBaseView view)
    {
        string dataPanelName;
        if (view is JJFormView formView)
            dataPanelName = formView.DataPanel.Name;
        else if (view is JJDataPanel dataPanel)
            dataPanelName = dataPanel.Name;
        else
            dataPanelName = string.Empty;

        return view.CurrentContext.Request.QueryString(UploadFormParameterName + dataPanelName) != null;
    }

    public static HtmlBuilder ResponseRoute(JJDataPanel view)
    {
        string uploadFormRoute = view.CurrentContext.Request.QueryString(UploadFormParameterName + view.Name);

        if (uploadFormRoute == null) return null;

        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(uploadFormRoute));

        if (field == null) return null;

        var upload = view.FieldManager.GetField(field, view.PageState, null, view.Values);
        return upload.GetHtmlBuilder();

    }

    private class OpenFormParms
    {
        public PageState PageState { get; set; }

        public bool Enable { get; set; }

        public string PkValues { get; set; }

    }
}
