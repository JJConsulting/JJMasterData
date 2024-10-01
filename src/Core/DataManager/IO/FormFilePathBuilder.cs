using System;
using System.Collections.Generic;
using System.IO;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.IO;

public class FormFilePathBuilder(FormElement formElement)
{
    public string GetFolderPath(FormElementField field, Dictionary<string, object> formValues)
    {
        if (field.DataFile == null)
            throw new ArgumentException($"{nameof(FormElementField.DataFile)} not defined.", field.Name);

        //Pks concat with  underline
        string pkval = DataHelper.ParsePkValues(formElement, formValues, '_');

        //Path configured in the dictionary
        string path = field.DataFile.FolderPath;

        if (string.IsNullOrEmpty(path))
            throw new ArgumentException($"{nameof(FormElementField.DataFile.FolderPath)} cannot be empty.", field.Name);

        char separator = Path.DirectorySeparatorChar;

        string appPath = FileIO.GetApplicationPath().TrimEnd(separator);

        path = path.Replace("{app.path}", appPath);
        path = Path.Combine(path, pkval);

        if (!path.EndsWith(separator.ToString()))
            path += separator;

        return path;
    }

   

}
