using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using System;
using System.Collections;
using System.IO;

namespace JJMasterData.Core.DataManager;

public class FormFilePathBuilder
{
    private FormElement FormElement { get; set; }

    public FormFilePathBuilder(FormElement formElement)
    {
        FormElement = formElement;
    }

    public string GetFolderPath(FormElementField field, Hashtable formValues)
    {
        if (field.DataFile == null)
            throw new ArgumentException($"{nameof(FormElementField.DataFile)} not defined.", field.Name);

        //Pks concat with  underline
        string pkval = DataHelper.ParsePkValues(FormElement, formValues, '_');
        if (!Validate.ValidFileName(pkval))
            throw new Exception(Translate.Key("Error rendering upload! Primary key value {0} contains invalid characters.",
                pkval));

        //Path configured in the dictionary
        string path = field.DataFile.FolderPath;

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException($"{nameof(FormElementField.DataFile.FolderPath)} cannot be empty.", field.Name);

        path = path.Replace("{app.path}", FileIO.GetApplicationPath());
        path = Path.Combine(path, pkval);

        string separator = Path.DirectorySeparatorChar.ToString();
        if (!path.EndsWith(separator))
            path += separator;

        return path;
    }

   

}
