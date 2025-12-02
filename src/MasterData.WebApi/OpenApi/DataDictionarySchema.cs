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
            Type = JsonSchemaType.Array,
            Title = $"{modelName}List",

            Items = new OpenApiSchema()
        };

        var example = new JsonObject();

        foreach (var field in formElement.Fields)
        {
            example[field.Name] = JsonSerializer.SerializeToNode(GetFieldExample(field));

            if (ignoreIdentity && field is { IsPk: true, AutoNum: true })
                continue;

            var fieldName = apiOptions.GetJsonFieldName(field.Name);
            var itemSchema = GetFieldSchema(field);

            modelSchema.Properties?.Add(fieldName, itemSchema);

            if (field.IsRequired || field.IsPk)
                modelSchema.Required?.Add(fieldName);
        }

        if (modelSchema.Required?.Count == 0)
            modelSchema.Required.Clear();

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
        
        if (item is { Component: FormComponent.ComboBox, DataItem.Items.Count: > 0 })
        {
            foreach (var dataItem in item.DataItem.Items)
            {
                itemSchema.Description += $"<br>{dataItem.Id} = {dataItem.Description}";
            }
        }

        itemSchema.Description = item.Label;
        if (item.IsPk)
            itemSchema.Description += " (<span class='propType'>PK<span>)";

        if (!string.IsNullOrEmpty(item.HelpDescription))
            itemSchema.Description += $"<br> {item.HelpDescription}";

        itemSchema.ReadOnly = item.DataBehavior == FieldBehavior.ViewOnly;
        
        return itemSchema;
    }

    internal static OpenApiSchema GetResponseSchema(string modelName)
    {
        return new OpenApiSchema
        {
            Title = $"{modelName}Status",
            Type = JsonSchemaType.Array,
            Items = GetValidationLetterSchema(true),
            Description = "List with status and validations"
        };
    }

    internal static OpenApiSchema GetValidationLetterSchema(bool enableDataField = false)
    {
        var modelSchema = new OpenApiSchema
        {
            Title = "validationLetter",
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>()
        };
        modelSchema.Properties.Add("status", new OpenApiSchema
        {
            Description = "Http Response Code",
            Type = JsonSchemaType.Integer,
            Format = "int32"
        });

        modelSchema.Properties.Add("message", new OpenApiSchema
        {
            Description = "Error Message",
            Type = JsonSchemaType.String
        });

        modelSchema.Properties.Add("validationList", new OpenApiSchema
        {
            Description = "Detailed error list",
            Type = JsonSchemaType.Array,
            Items = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Description = "Field, Value"
            },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                {
                    "errorList", 
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
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
                Type = JsonSchemaType.Array,
                Items = new OpenApiSchema
                {
                    Description = "Field, Value",
                    Type = JsonSchemaType.Object
                }
            });
        }

        modelSchema.Required?.Add("status");
        modelSchema.Required?.Add("message");

        return modelSchema;
    }
}
