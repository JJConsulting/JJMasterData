#nullable enable

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using JJMasterData.Commons.Util;
using NCalc;
using NCalc.Exceptions;

namespace JJMasterData.Core.Configuration.Options;

public sealed class MasterDataCoreOptions
{
    /// <summary>
    /// Default value: tb_masterdata
    /// </summary>
    [Display(Name = "Data Dictionary Table Name")]
    public string DataDictionaryTableName { get; set; } = "tb_masterdata";

    /// <summary>
    /// Default value: tb_masterdata_auditlog
    /// </summary>
    [Display(Name = "Audit Log Table Name")]
    public string AuditLogTableName { get; set; } = "tb_masterdata_auditlog";

#if !NET
    /// <summary>
    /// Default value: null
    /// </summary>
    public string? MasterDataUrl { get; set; }

    public bool EnableCultureProviderAtUrl { get; set; } = true;
#endif

    [Display(Name = "Enable Data Dictionary Caching")]
    public bool EnableDataDictionaryCaching { get; set; } = true;

    /// <summary>
    /// Default value: {ApplicationPath}/JJExportationFiles
    /// </summary>
    [Display(Name = "Exportation Folder Path")]
    public string ExportationFolderPath { get; set; } = Path.Combine(FileIO.GetApplicationPath(), "JJExportationFiles");

    /// <summary>
    /// Context of expressions starting with "exp:". Declare here custom parameters and functions.
    /// </summary>
    public ExpressionContext ExpressionContext { get; set; } = new()
    {
        Options = ExpressionOptions.IgnoreCaseAtBuiltInFunctions
                  | ExpressionOptions.AllowNullParameter
                  | ExpressionOptions.OrdinalStringComparer
                  | ExpressionOptions.AllowNullOrEmptyExpressions
                  | ExpressionOptions.CaseInsensitiveStringComparer,
        Functions = new Dictionary<string, ExpressionFunction>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "now", _ => DateTime.Now },
            {
                "iif", args =>
                {
                    if (args.Count() != 3)
                        throw new NCalcEvaluationException("iif() takes exactly 3 arguments.");
                    var conditional = StringManager.ParseBool(args[0].Evaluate());
                    return conditional ? args[1].Evaluate() : args[2].Evaluate();
                }
            }
        }.ToFrozenDictionary()
    };
}