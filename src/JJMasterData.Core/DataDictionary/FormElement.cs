#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Form data
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class FormElement : Element
{
    public string? Title { get; set; }

    public string? SubTitle { get; set; }
    
    [Required]
    [DataMember(Name = "fields")]
    public new FormElementList Fields { get; private set; }
    
    [Required]
    [DataMember(Name = "panels")]
    public List<FormElementPanel> Panels { get; set; }
    
    [Required]
    [DataMember(Name="relations")]
    public new FormElementRelationshipList Relationships { get; set; }
    
    [Required]
    [DataMember(Name="options")]
    public FormElementOptions Options { get; set; }
    
    [Required]
    [DataMember(Name="apiOptions")]
    public FormElementApiOptions ApiOptions { get; set; }
    
    public FormElement()
    {
        Fields = new FormElementList(base.Fields);
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
        Sync = element.Sync;
        SyncMode = element.SyncMode;
        Title = element.Name;
        SubTitle = element.Info;
        
        Fields = new FormElementList(base.Fields);
        Panels = new List<FormElementPanel>();
        ApiOptions = new FormElementApiOptions();
        Options = new FormElementOptions();
        
        foreach (var f in element.Fields)
        {
            AddField(f);
        }
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

            AddField(field);
        }
    }

    [JsonConstructor]
    private FormElement(
        FormElementList fields,
        List<FormElementPanel> panels,
        FormElementRelationshipList relationships, 
        FormElementOptions options,
        FormElementApiOptions apiOptions)
    {
        Fields = fields;
        Relationships = relationships;
        Options = options;
        ApiOptions = apiOptions;
        Panels = panels;
    }

    protected void SetFieldType(ElementField field, Type type)
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

    protected void AddField(ElementField field)
    {
        Fields.Add(new FormElementField(field));
    }

    public FormElementPanel GetPanelById(int id)
    {
        return Panels.First(x => x.PanelId == id);
    }
}