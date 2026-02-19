#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using JJMasterData.Commons.Util;
using NCalc;
using NCalc.Exceptions;

namespace JJMasterData.Core.Configuration.Options;

public sealed class MasterDataCoreOptions
{
    [Display(Name = "Data Dictionary Table Schema")]
    public string? DataDictionaryTableSchema { get; set; }
    
    /// <summary>
    /// Default value: tb_masterdata
    /// </summary>
    [Display(Name = "Data Dictionary Table Name")]
    public string DataDictionaryTableName { get; set; } = "tb_masterdata";

    [Display(Name = "Audit Log Table Schema")]
    public string? AuditLogTableSchema { get; set; }
    
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

    public string UserIdClaimType { get; set; } = ClaimTypes.NameIdentifier;
    
    /// <summary>
    /// Context of expressions starting with "exp:". Declare here custom parameters and functions.
    /// </summary>
    public ExpressionContext ExpressionContext { get; set; } = new()
    {
        Options = ExpressionOptions.IgnoreCaseAtBuiltInFunctions
                  | ExpressionOptions.AllowNullParameter
                  | ExpressionOptions.OrdinalStringComparer
                  | ExpressionOptions.AllowNullOrEmptyExpressions
                  | ExpressionOptions.ArithmeticNullOrEmptyStringAsZero
                  | ExpressionOptions.CaseInsensitiveStringComparer,
        Functions = new Dictionary<string, ExpressionFunction>(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                "now", _ => DateTime.Now
            },
            {
                "utcNow", _ => DateTime.UtcNow
            },
            {
                "empty", _ => string.Empty
            },
            {
                "iif", args =>
                {
                    if (args.Count() != 3)
                        throw new NCalcEvaluationException("iif() takes exactly 3 arguments.");
                    var conditional = StringManager.ParseBool(args[0].Evaluate());
                    return conditional ? args[1].Evaluate() : args[2].Evaluate();
                }
            },
            {
                "len", args =>
                {
                    if (args.Count() != 1)
                    {
                        throw new NCalcEvaluationException("len() takes exactly 1 argument.");
                    }

                    return args[0].Evaluate()?.ToString()?.Length;
                }
            },
            {
                "trim", args =>
                {
                    if (args.Count() != 1)
                    {
                        throw new NCalcEvaluationException("trim() takes exactly 1 argument.");
                    }

                    return args[0].Evaluate()?.ToString()?.Trim();
                }
            }
        }
    };
}