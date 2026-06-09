#nullable enable

using System.IO;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataManager.Exportation;

public static class DataExportationHelper
{
    /// <summary>
    /// Path where the files are generated.
    /// </summary>
    public static string GetExportationFolderPath(FormElement formElement, string path, string userId)
    {
        var processOptions = formElement.Options.GridToolbarActions.ExportAction.ProcessOptions;
        var folderPath = Path.Combine(path, formElement.Name);

        if (processOptions.Scope == ProcessScope.User)
        {
            folderPath = Path.Combine(folderPath, userId);
        }
        return folderPath;
    }

    public static string GetExportationFolderPath(JJDataExportation dataExportation)
    {
        var path = dataExportation.MasterDataOptions.ExportationFolderPath;
        return GetExportationFolderPath(dataExportation.FormElement , path, dataExportation.UserId);    
    }
}
