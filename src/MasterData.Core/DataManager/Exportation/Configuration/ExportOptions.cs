#nullable disable warnings
using System;
using System.Collections.Generic;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.DataManager.Exportation.Configuration;

/// <summary>
/// Classe responsável por configurar a exportação dos dados da JJGridView
/// </summary>
public class ExportOptions
{
    internal const string FileName = "_export_table_file";
    internal const string FormatOptionPrefix = "_export_option_";
    internal const string ExportTableFirstLine = "_export_table_firstline";
    internal const string ExportAll = "_export_table_all";

    public string FormatId { get; set; } = "xlsx";
    public bool ExportAllFields { get; set; } = true;
    public Dictionary<string, string?> FormatOptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    internal static ExportOptions LoadFromForm(IHttpContextAccessor httpContextAccessor, string componentName)
    {
        var expConfig = new ExportOptions();

        if (!httpContextAccessor.HttpContext!.Request.HasFormContentType)
            return expConfig;
        
        var form = httpContextAccessor.HttpContext!.Request.Form;
        if (form.TryGetValue(componentName + FileName, out var fileName))
        {
            expConfig.FormatId = fileName.ToString();
            expConfig.ExportAllFields = StringManager.ParseBool(form[componentName + ExportAll]);
            var prefix = componentName + FormatOptionPrefix + expConfig.FormatId + "_";
            foreach (var value in form)
            {
                if (value.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    expConfig.FormatOptions[value.Key[prefix.Length..]] = value.Value.ToString();
            }
        }

        return expConfig;
    }


}
