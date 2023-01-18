using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace JJMasterData.Swagger.AspNetCore;

internal static class DataDictionarySchema
{
    internal static OpenApiSchema GetDictionarySchema(FormElement formElement, MetadataApiOptions metadataApiOptions, string modelName, bool ignoreIdentity = false)
    {
        var modelSchema = new OpenApiSchema
        {
            Type = "array",
            Title = modelName + "List",

            Items = new OpenApiSchema()
        };

        var example = new OpenApiObject();

        foreach (var field in formElement.Fields)
        {

            example[field.Name] = GetFieldExample(field);

            if (ignoreIdentity && field is { IsPk: true, AutoNum: true })
                continue;

            string fieldName = metadataApiOptions.GetFieldNameParsed(field.Name);
            var itemSchema = GetFieldSchema(field);

            modelSchema.Properties.Add(fieldName, itemSchema);

            if (field.IsRequired | field.IsPk)
                modelSchema.Required.Add(fieldName);
        }

        if (modelSchema.Required.Count == 0)
            modelSchema.Required = null;

        modelSchema.Example = example;

        return modelSchema;
    }

    private static IOpenApiAny GetFieldExample(ElementField field)
    {
        return field.DataType switch
        {
            FieldType.Int => new OpenApiInteger(0),
            FieldType.Float => new OpenApiFloat(0),
            FieldType.Date => new OpenApiDate(DateTime.Now),
            FieldType.DateTime => new OpenApiDateTime(DateTime.Now),
            _ => new OpenApiString("string"),
        };
    }

    internal static OpenApiSchema GetFieldSchema(FormElementField item)
    {
        OpenApiSchema itemSchema;

        switch (item.DataType)
        {
            case FieldType.Int:
                itemSchema = new OpenApiSchema
                {
                    Type = "integer",
                    Format = "int32"
                };
                break;
            case FieldType.Float:
                itemSchema = new OpenApiSchema
                {
                    Type = "number",
                    Format = "float"
                };
                break;
            case FieldType.Date:
                itemSchema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "date"
                };
                break;
            case FieldType.DateTime:
                itemSchema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "datetime"
                };
                break;
            default:
                itemSchema = new OpenApiSchema
                {
                    Type = "string"
                };
                if (item.Size > 0)
                    itemSchema.MaxLength = item.Size;
                break;
        }


        if (item is { Component: FormComponent.ComboBox, DataItem.Items.Count: > 0 })
        {
            foreach (var dataItem in item.DataItem.Items)
            {
                itemSchema.Description += "<br>" + dataItem.Id + " = " + dataItem.Description;
            }
        }

        itemSchema.Description = item.Label;
        if (item.IsPk)
            itemSchema.Description += " (<span class='propType'>PK<span>)";

        if (!string.IsNullOrEmpty(item.HelpDescription))
            itemSchema.Description += "<br> " + item.HelpDescription;

        itemSchema.ReadOnly = item.DataBehavior == FieldBehavior.ViewOnly;

        return itemSchema;
    }

    internal static OpenApiSchema GetResponseSchema(string modelName)
    {
        return new OpenApiSchema
        {
            Title = modelName + "Status",
            Type = "array",
            Items = GetValidationLetterSchema(true),
            Description = "List with status and validations"
        };
    }

    internal static OpenApiSchema GetValidationLetterSchema(bool enableDataField = false)
    {
        var modelSchema = new OpenApiSchema
        {
            Title = "validationLetter",
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>()
        };
        modelSchema.Properties.Add("status", new OpenApiSchema
        {
            Description = "Http Response Code",
            Type = "integer",
            Format = "int32"
        });

        modelSchema.Properties.Add("message", new OpenApiSchema
        {
            Description = "Error Message",
            Type = "string"
        });

        modelSchema.Properties.Add("validationList", new OpenApiSchema
        {
            Description = "Detailed error list",
            Type = "array",
            Items = new OpenApiSchema
            {
                Type = "object",
                Description = "Field, Value"
            },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                {
                    "errorList", 
                    new OpenApiSchema
                    {
                        Type = "object",
                        Description = "Field, Value"
                    }
                }
            }
        });

        if (enableDataField)
        {
            modelSchema.Properties.Add("data", new OpenApiSchema
            {
                Description = "Return of fields, identity for example",
                Type = "array",
                Items = new OpenApiSchema
                {
                    Description = "Field, Value",
                    Type = "object"
                }
            });
        }

        modelSchema.Required.Add("status");
        modelSchema.Required.Add("message");

        return modelSchema;
    }
}
