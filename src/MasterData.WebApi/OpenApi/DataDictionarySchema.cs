#nullable enable

using System.Text.Json;
using System.Text.Json.Nodes;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.OpenApi;


namespace JJMasterData.WebApi.OpenApi;

internal static class DataDictionarySchema
{
    internal static OpenApiSchema GetDictionarySchema(FormElement formElement, FormElementApiOptions apiOptions, string modelName, bool ignoreIdentity = false)
    {
        var modelSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Title = modelName,
            Properties = new Dictionary<string, IOpenApiSchema>(),
            Required = new HashSet<string>()
        };

        var example = new JsonObject();

        foreach (var field in formElement.Fields)
        {
            if (ignoreIdentity && field is { IsPk: true, AutoNum: true })
                continue;

            var fieldName = apiOptions.GetJsonFieldName(field.Name);
            var itemSchema = GetFieldSchema(field);

            example[fieldName] = JsonSerializer.SerializeToNode(GetFieldExample(field));

            modelSchema.Properties.Add(fieldName, itemSchema);

            if (field.IsRequired || field.IsPk)
                modelSchema.Required.Add(fieldName);
        }

        modelSchema.Example = example;

        return modelSchema;
    }

    private static JsonValue GetFieldExample(ElementField field)
    {
        return field.DataType switch
        {
            FieldType.Int => JsonValue.Create(0),
            FieldType.Float or FieldType.Decimal => JsonValue.Create(0d),
            FieldType.Date => JsonValue.Create(DateTime.Now.Date),
            FieldType.DateTime => JsonValue.Create(DateTime.Now),
            _ => JsonValue.Create("string"),
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
                    Type = JsonSchemaType.Integer,
                    Format = "int32"
                };
                break;
            case FieldType.Decimal:
            case FieldType.Float:
                itemSchema = new OpenApiSchema
                {
                    Type = JsonSchemaType.Number,
                    Format = "double"
                };
                break;
            case FieldType.Date:
                itemSchema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "date"
                };
                break;
            case FieldType.DateTime:
                itemSchema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "datetime"
                };
                break;
            default:
                itemSchema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String
                };
                if (item.Size > 0)
                    itemSchema.MaxLength = item.Size;
                break;
        }

        var description = item.LabelOrName;

        if (item is { Component: FormComponent.ComboBox, DataItem.Items.Count: > 0 })
        {
            foreach (var dataItem in item.DataItem.Items)
                description += $"<br>{dataItem.Id} = {dataItem.Description}";
        }

        if (item.IsPk)
            description += " (<span class='propType'>PK</span>)";

        if (!string.IsNullOrEmpty(item.HelpDescription))
            description += $"<br>{item.HelpDescription}";

        itemSchema.Description = description;
        itemSchema.ReadOnly = item.DataBehavior == FieldBehavior.ViewOnly;

        return itemSchema;
    }

    internal static OpenApiSchema GetValidationLetterSchema(bool enableDataField = false)
    {
        var modelSchema = new OpenApiSchema
        {
            Title = "ValidationLetter",
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                {
                    "status", new OpenApiSchema
                    {
                        Description = "Http Response Code",
                        Type = JsonSchemaType.Integer,
                        Format = "int32"
                    }
                },
                {
                    "message", new OpenApiSchema
                    {
                        Description = "Error Message",
                        Type = JsonSchemaType.String
                    }
                },
                {
                    "validationList", new OpenApiSchema
                    {
                        Description = "Detailed error list",
                        Type = JsonSchemaType.Object,
                        AdditionalProperties = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String
                        }
                    }
                }
            },
            Required = new HashSet<string> { "status", "message" }
        };

        if (enableDataField)
        {
            modelSchema.Properties.Add("data", new OpenApiSchema
            {
                Description = "Return of fields, identity for example",
                Type = JsonSchemaType.Object
            });
        }

        return modelSchema;
    }
}
