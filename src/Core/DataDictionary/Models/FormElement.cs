#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class FormElement : Element
{
    [JsonIgnore]
    internal string? ParentName { get; set; }
    
    [JsonProperty]
    public string? Title { get; set; }

    [JsonProperty]
    public string? SubTitle { get; set; }
    
    [Required]
    [JsonProperty("fields")]
    public new FormElementFieldList Fields { get; private set; }
    
    [Required]
    [JsonProperty("panels")]
    public List<FormElementPanel> Panels { get; set; }
    
    [Required]
    [JsonProperty("relations")]
    public new FormElementRelationshipList Relationships { get; set; }
    
    [Required]
    [JsonProperty("options")]
    public FormElementOptions Options { get; set; }
    
    [Required]
    [JsonProperty("apiOptions")]
    public FormElementApiOptions ApiOptions { get; set; }

    public FormElement()
    {
        Fields = new FormElementFieldList(base.Fields);
        Panels = new List<FormElementPanel>();
        Options = new FormElementOptions();
        Relationships = new FormElementRelationshipList(base.Relationships);
        ApiOptions = new FormElementApiOptions();
    }

    public FormElement(Element element) 
    {
        Name = element.Name;
        TableName = element.TableName;
        Info = element.Info;
        Indexes = element.Indexes;
        base.Relationships = element.Relationships;
        Relationships = new FormElementRelationshipList(base.Relationships);
        CustomProcNameGet = element.CustomProcNameGet;
        CustomProcNameSet = element.CustomProcNameSet;
        EnableApi = element.EnableApi;
        SyncMode = element.SyncMode;
        Title = element.Name;
        SubTitle = element.Info;

        base.Fields = element.Fields;
        Fields = new FormElementFieldList(element.Fields);
        Panels = new List<FormElementPanel>();
        ApiOptions = new FormElementApiOptions();
        Options = new FormElementOptions();
    }

    public FormElement(DataTable schema) : this()
    {
        if (schema == null)
            throw new ArgumentNullException(nameof(schema), @"DataTable schema cannot be null");

        Name = schema.TableName;
        TableName = schema.TableName;
        Title = schema.TableName;

        foreach (DataColumn col in schema.Columns)
        {
            var field = new ElementField
            {
                Name = col.ColumnName,
                Label = col.Caption.Replace("::ASC", "").Replace("::DESC", ""),
                Size = col.MaxLength,
                IsRequired = !col.AllowDBNull,
                IsPk = col.Unique
            };

            var type = col.DataType;

            SetFieldType(field, type);
            
            Fields.Add(new FormElementField(field));
        }
    }

    [JsonConstructor]
    private FormElement(
        FormElementFieldList fields,
        List<FormElementPanel>? panels,
        FormElementRelationshipList relationships, 
        FormElementOptions? options,
        FormElementApiOptions? apiOptions)
    {
        base.Fields = new ElementFieldList(fields.Cast<ElementField>().ToList());
        Fields = fields;
        base.Relationships = new List<ElementRelationship>(relationships.Where(r=>r.ElementRelationship != null).Select(r=>r.ElementRelationship).ToList()!);
        Relationships = relationships;
        Options = options ?? new FormElementOptions();
        ApiOptions = apiOptions ?? new FormElementApiOptions();
        Panels = panels ?? new List<FormElementPanel>();
    }

    private static void SetFieldType(ElementField field, Type type)
    {
        if (type == typeof(int) ||
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong) ||
            type == typeof(float))
        {
            field.DataType = FieldType.Int;
        }
        else if (type == typeof(decimal) ||
                 type == typeof(double))
        {
            field.DataType = FieldType.Float;
        }
        else if (type == typeof(DateTime))
        {
            field.DataType = FieldType.Date;
        }
        else if (type == typeof(TimeSpan))
        {
            field.DataType = FieldType.DateTime;
        }
        else
        {
            field.DataType = FieldType.NVarchar;
        }
    }

    public FormElementPanel GetPanelById(int id)
    {
        return Panels.First(x => x.PanelId == id);
    }
}