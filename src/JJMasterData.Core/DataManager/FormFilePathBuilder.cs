using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace JJMasterData.Core.DataManager;

internal class FormFilePathBuilder
{
    private FormElement FormElement { get; set; }

    public FormFilePathBuilder(FormElement formElement)
    {
        FormElement = formElement;
    }

    public string GetFolderPath(FormElementField ElementField, Hashtable formValues)
    {
        if (ElementField.DataFile == null)
            throw new ArgumentException($"{nameof(FormElementField.DataFile)} not defined.", ElementField.Name);

        //Pks concat with  underline
        string pkval = GetPkValues(formValues, '_');

        //Path configured in the dictionary
        string path = ElementField.DataFile.FolderPath;

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException($"{nameof(FormElementField.DataFile.FolderPath)} cannot be empty.", ElementField.Name);

        path = path.Replace("{app.path}", FileIO.GetApplicationPath());
        path = Path.Combine(path, pkval);

        string separator = Path.DirectorySeparatorChar.ToString();
        if (!path.EndsWith(separator))
            path += separator;

        return path;
    }

    public string GetPkValues(Hashtable formValues, char separator)
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

            if (!formValues.ContainsKey(pkField.Name))
                throw new Exception(Translate.Key("Error rendering upload! Primary key value {0} not found at {1}",
                    pkField.Name, FormElement.Name));

            string value = formValues[pkField.Name].ToString();
            if (!Validate.ValidFileName(value))
                throw new Exception(Translate.Key("Error rendering upload! Primary key value {0} contains invalid characters.",
                    pkField.Name));

            name += value;
        }

        return name;
    }

}
