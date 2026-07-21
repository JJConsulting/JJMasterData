using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager.Exportation.Configuration;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Web.Components;

internal static class ExportOptionsFormLoader
{
    internal static ExportOptions Load(IHttpContextAccessor httpContextAccessor, string componentName)
    {
        var options = new ExportOptions();
        if (httpContextAccessor.HttpContext?.Request.HasFormContentType != true)
            return options;

        var form = httpContextAccessor.HttpContext.Request.Form;
        if (!form.TryGetValue(componentName + ExportOptions.FileName, out var fileName))
            return options;

        options.FileExtension = (ExportFileExtension)int.Parse(fileName.ToString());
        options.IsLandScape = StringManager.ParseBool(form[componentName + ExportOptions.TableOrientation]);
        options.ExportFirstLine = StringManager.ParseBool(form[componentName + ExportOptions.ExportTableFirstLine]);
        options.ExportAllFields = StringManager.ParseBool(form[componentName + ExportOptions.ExportAll]);
        options.Delimiter = form[componentName + ExportOptions.ExportDelimiter].ToString();
        return options;
    }
}
