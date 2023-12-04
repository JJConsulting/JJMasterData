using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Models;

namespace JJMasterData.Commons.Localization;

public static class MasterDataStringLocalizerElement
{
    public static Element GetElement(MasterDataCommonsOptions options)
    {
        var element = new Element
        {
            Name = options.LocalizationTableName,
            TableName = options.LocalizationTableName,
            ReadProcedureName = options.GetReadProcedureName(options.LocalizationTableName),
            WriteProcedureName = options.GetWriteProcedureName(options.LocalizationTableName),
            Info = "Resources"
        };

        var culture = new ElementField
        {
            Name = "cultureCode",
            Label = "Culture Code",
            DataType = FieldType.Varchar,
            Filter =
            {
                Type = FilterMode.Equal
            },
            Size = 10,
            IsPk = true
        };
        element.Fields.Add(culture);

        var key = new ElementField
        {
            Name = "resourceKey",
            Label = "Key",
            DataType = FieldType.Varchar,
            Size = 255,
            Filter =
            {
                Type = FilterMode.Contain
            },
            IsPk = true
        };
        element.Fields.Add(key);

        var value = new ElementField
        {
            Name = "resourceValue",
            Label = "Value",
            DataType = FieldType.Varchar,
            Size = 500,
            Filter =
            {
                Type = FilterMode.Contain
            },
            IsRequired = true
        };
        element.Fields.Add(value);

        var origin = new ElementField
        {
            Name = "resourceOrigin",
            Label = "Origin",
            DataType = FieldType.Varchar,
            Size = 50,
            Filter =
            {
                Type = FilterMode.Equal
            }
        };
        element.Fields.Add(origin);

        return element;
    }
}