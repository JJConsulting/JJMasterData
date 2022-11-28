using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

public class JJTextFile : JJBaseControl
{
    private const string UploadFormParameterName = "jjuploadform_";

    private Hashtable _formValues;

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

    internal static JJTextFile GetInstance(FormElementField field,
                                  PageState pagestate,
                                  object value,
                                  Hashtable formValues,
                                  bool enable,
                                  string name)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        if (field.DataFile == null)
            throw new ArgumentException(Translate.Key("Upload config not defined"), field.Name);

        var text = new JJTextFile
        {
            ElementField = field,
            PageState = pagestate,
            Text = value != null ? value.ToString() : "",
            FormValues = formValues,
            Enable = enable,
            Name = name ?? field.Name,
            ReadOnly = field.DataBehavior == FieldBehavior.ViewOnly
        };
        text.SetAttr(field.Attributes);

        return text;
    }


    protected override string RenderHtml()
    {
        var html = new StringBuilder();
        if (IsFormUploadRoute())
        {
            //Ao abrir uma nova pagina no iframe o "jumi da india" não conseguiu fazer o iframe via post 
            //por esse motivo passamos os valores nessários do form anterior por parametro o:)
            LoadDirectValues();

            var formUpload = GetFormUpload();
            html.AppendLine(formUpload.GetHtml());
            html.AppendLine(GetRefreshScript());
        }
        else
        {
            html.AppendLine($"<div id=\"div_{Name}\">");
            html.AppendLine(RenderTextHtml());
            html.Append("</div>");
        }
        return html.ToString();
    }

    private string RenderTextHtml()
    {
        var formUpload = GetFormUpload();

        string cssClass = !string.IsNullOrEmpty(CssClass) ? CssClass : string.Empty;
        int tabIdent = 5;

        if (!Enable)
            formUpload.ClearMemoryFiles();

        var html = new StringBuilder();
        html.Append('\t', tabIdent);
        html.Append($"<div class=\"input-group jjform-upload {cssClass}\" >");

        html.Append('\t', tabIdent);
        html.Append("<input type=\"hidden\" ");
        html.Append($"id=\"{Name}\" ");
        html.Append($"name=\"{Name}\" ");
        html.AppendLine($"value =\"{GetFileName(formUpload)}\" >");

        html.Append('\t', tabIdent);

        if(BootstrapHelper.Version < 5)
            html.Append(GetInputHtml(cssClass, formUpload));

        html.Append('\t', tabIdent);
        html.AppendLine($"<span class=\"{BootstrapHelper.InputGroupBtn}\"> ");
        html.Append('\t', ++tabIdent);
        if (BootstrapHelper.Version >= 5)
            html.Append(GetInputHtml(cssClass, formUpload));
        html.Append("<button type=\"button\" ");
        html.Append($"onclick=\"{GetOpenUploadFormAction()}\" ");
        html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
        html.AppendFormat("title=\"{0}\" ", Translate.Key("Manage Files"));
        html.AppendLine($"class=\"{(BootstrapHelper.Version ==3 ? BootstrapHelper.DefaultButton : "input-group-text")}\">");
        html.Append('\t', ++tabIdent);
        html.AppendLine(new JJIcon(IconType.Paperclip).GetHtml());
        html.Append('\t', --tabIdent);
        html.AppendLine("</button>");
        html.Append('\t', --tabIdent);
        html.AppendLine("</span> ");
        html.Append('\t', --tabIdent);
        html.Append("</div>");

        return html.ToString();
    }

    private string GetInputHtml(string cssClass, JJFormUpload formUpload)
    {

        var html = new StringBuilder();

        html.Append("<input type=\"text\" readonly ");
        html.Append($"id=\"v_{Name}\" ");
        html.Append($"name=\"v_{Name}\" ");
        html.Append($"class=\"{cssClass} form-control\" ");
        html.Append($"value =\"{GetPresentationText(formUpload)}\" ");

        if (!string.IsNullOrEmpty(ToolTip))
        {
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" title=\"");
            html.Append(Translate.Key(ToolTip));
            html.Append("\" ");
        }

        foreach (DictionaryEntry attr in Attributes)
        {
            html.Append(" ");
            html.Append(attr.Key);
            if (attr.Value != null)
            {
                html.Append("=\"");
                html.Append(attr.Value);
                html.Append("\"");
            }
        }

        html.AppendLine("/>");

        return html.ToString();
    }

    private string GetRefreshScript()
    {
        var html = new StringBuilder();

        string fieldHtml = RenderTextHtml().Replace("\r\n", "").Replace("\t", "").Replace("\"", "\\\"");

        //Scripts
        html.Append('\t', 1);
        html.AppendLine("<script type=\"text/javascript\"> ");
        html.Append('\t', 2);
        html.AppendLine("$(document).ready(function () {");
        html.Append('\t', 3);
        html.AppendLine($"window.parent.$(\"#div_{Name}\").html(\"{fieldHtml}\");");
        html.AppendLine("\t\t});");
        html.AppendLine("\t</script> ");


        return html.ToString();
    }

    private string GetOpenUploadFormAction()
    {
        var parms = new OpenFormParms();
        parms.PageState = PageState;
        parms.Enable = Enable & !ReadOnly;

        if (PageState != PageState.Insert)
            parms.PkValues = GetPkValues('|');

        string json = JsonConvert.SerializeObject(parms);
        string value = Cript.Cript64(json);

        string title = ElementField.Label;
        if (title == null)
            title = "Manage Files";
        else
            title.Replace('\'', '`').Replace('\"', ' ');

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
            throw new Exception(Translate.Key("Invalid parameters when opening file upload"));

        PageState = parms.PageState;
        Enable = parms.Enable;

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

    public void SaveTempUserFiles()
    {
        string folderPath = GetFolderPath();
        JJFormUpload formUpload = GetFormUpload();
        formUpload.SaveTempUserFiles(folderPath);
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
        form.Name = ElementField.Name + "_formupload";
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

        if (!Enable)
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

            string value = FormValues[pkField.Name].ToString();
            if (!Validate.ValidFileName(value))
                return false;
        }

        return true;
    }

    public string GetFolderPath()
    {
        if (ElementField.DataFile == null)
            throw new ArgumentException($"{nameof(FormElementField.DataFile)} not defined.", ElementField.Name);

        //Pks separadas por underline
        string pkval = GetPkValues('_');

        //Caminho confugurado no dicionario
        string path = ElementField.DataFile.FolderPath;

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException($"{nameof(FormElementField.DataFile.FolderPath)} cannot be empty.", ElementField.Name);

        //Replace {app.path}
        
        string baseDirectory = FileIO.GetApplicationPath();
        
        path = path.Replace("{app.path}", baseDirectory);
        
        path = Path.Combine(path, pkval);

        string separator = Path.DirectorySeparatorChar.ToString();

        if (!path.EndsWith(separator))
            path += separator;
        
        return path;
    }

    private string GetPkValues(char separator)
    {
        string name = string.Empty;
        var pkFields = FormElement.Fields.ToList().FindAll(x => x.IsPk);
        if (pkFields.Count == 0)
            throw new Exception(Translate.Key("Error rendering upload! Primary key not defined in {0}",
                FormElement.Name));

        foreach (var pkField in pkFields)
        {
            if (name.Length > 0)
                name += separator.ToString();

            if (!FormValues.ContainsKey(pkField.Name))
                throw new Exception(Translate.Key("Error rendering upload! Primary key value {0} not found at {1}",
                    pkField.Name, FormElement.Name));

            string value = FormValues[pkField.Name].ToString();
            if (!Validate.ValidFileName(value))
                throw new Exception(Translate.Key("Error rendering upload! Primary key value {0} contains invalid characters.",
                    pkField.Name));

            name += value;
        }

        return name;
    }

    private string GetPanelName()
    {
        string pnlName = string.Empty;
        if (Attributes.ContainsKey("pnlname"))
            pnlName = Attributes["pnlname"].ToString();

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

            fileNames += file.FileName;
        }

        return fileNames;

    }

    private string GetPresentationText(JJFormUpload formUpload)
    {
        var files = formUpload.GetFiles().FindAll(x => !x.Deleted);
        
        return files.Count switch
        {
            0 => string.Empty,
            1 => files[0].FileName,
            _ => Translate.Key("{0} Selected Files", files.Count)
        };
    }

    internal string GetHtmlForGrid()
    {
        if (string.IsNullOrEmpty(Text))
            return string.Empty;

        var html = new StringBuilder();
        string[] files = Text.Split(',');
        if (files.Length == 1)
        {
            string filename = files[0];
            html.Append($"<a href=\"{GetDownloadLink(filename)}\">");
            html.Append(new JJIcon(IconType.CloudDownload).GetHtml());
            html.Append($"&nbsp;{filename}");
            html.AppendLine("</a>");
        }
        else
        {
            html.AppendLine("\t\t\t\t\t\t<div class=\"btn-group\">");
            html.AppendLine($"\t\t\t\t\t\t\t<button type=\"button\" class=\"btn-link dropdown-toggle\" {BootstrapHelper.DataToggle}=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">");
            html.Append(files.Length);
            html.Append("&nbsp;");
            html.Append(Translate.Key("Files"));
            html.Append(" &nbsp;");
            html.Append("<span class=\"caret\" ");
            html.Append($"{BootstrapHelper.DataToggle}=\"tooltip\" ");
            html.Append($"title=\"{Translate.Key("Download")}\">");
            html.AppendLine("</span>");
            html.AppendLine("\t\t\t\t\t\t\t</button>");
            html.AppendLine("\t\t\t\t\t\t\t<ul class=\"dropdown-menu dropdown-menu-right\">");
            foreach (var filename in files)
            {
                html.AppendLine("\t\t\t\t\t\t\t\t<li>");
                html.Append("\t\t\t\t\t\t\t\t\t");
                html.Append($"<a href=\"{GetDownloadLink(filename)}\">");
                html.Append(new JJIcon(IconType.CloudDownload).GetHtml());
                html.Append($"&nbsp;{filename}");
                html.AppendLine("</a>");
                html.AppendLine("\t\t\t\t\t\t\t\t</li>");

            }
            html.AppendLine("\t\t\t\t\t\t\t</ul>");
            html.AppendLine("\t\t\t\t\t\t</div>");
        }
        return html.ToString();
    }

    public string GetDownloadLink(string fileName, bool isExternalLink = false)
    {
        string filePath = GetFolderPath() + fileName;
        string url = CurrentContext.Request.AbsoluteUri;
        if (url.Contains("?"))
            url += "&";
        else
            url += "?";

        if (isExternalLink)
            url += JJDownloadFile.PARAM_DIRECTDOWNLOAD;
        else
            url += JJDownloadFile.PARAM_DOWNLOAD;

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
        string dataPanelName = view switch
        {
            JJFormView formView => formView.DataPanel.Name,
            JJDataPanel dataPanel => dataPanel.Name,
            _ => string.Empty
        };

        return view.CurrentContext.Request.QueryString(UploadFormParameterName + dataPanelName) != null;
    }

    public static string ResponseRoute(JJDataPanel view)
    {
        string uploadFormRoute = view.CurrentContext.Request.QueryString(UploadFormParameterName + view.Name);
        
        if (uploadFormRoute == null) return null;
        
        var field = view.FormElement.Fields.ToList().Find(x => x.Name.Equals(uploadFormRoute));

        if (field == null) return null;
        
        var upload = view.FieldManager.GetField(field, view.PageState, null, view.Values);
        return upload.GetHtml();

    }

    private class OpenFormParms
    {
        public PageState PageState { get; set; }

        public bool Enable { get; set; }

        public string PkValues { get; set; }

    }
}
