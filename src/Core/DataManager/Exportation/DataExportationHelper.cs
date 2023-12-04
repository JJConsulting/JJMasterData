#nullable enable

using System.IO;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataManager.Exportation;

internal static class DataExportationHelper
{
    /// <summary>
    /// Path where the files are generated.
    /// </summary>
    public static string GetFolderPath(FormElement formElement, string path, string userId)
    {
        var processOptions = formElement.Options.GridToolbarActions.ExportAction.ProcessOptions;
        string folderPath = Path.Combine(path, (string)formElement.Name);

        if (processOptions.Scope == ProcessScope.User)
        {
            folderPath = Path.Combine(folderPath, userId);
        }
        return folderPath;
    }

    public static string GetFolderPath(JJDataExportation dataExportation)
    {
        var path = dataExportation.MasterDataOptions.ExportationFolderPath;
        return DataExportationHelper.GetFolderPath(dataExportation.FormElement , path, dataExportation.UserId);    
    }
    
}