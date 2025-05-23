﻿#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElement : Element
{
    private FormElementFieldList? _fields;
    private FormElementRelationshipList? _relationships;

    [JsonIgnore]
    internal string? ParentName { get; set; }

    [JsonPropertyName("title")]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    [JsonPropertyName("titleSize")]
    [Display(Name = "Title Size")]
    public HeadingSize TitleSize { get; set; } = HeadingSize.H3;

    [JsonPropertyName("subTitle")]
    [Display(Name = "SubTitle")]
    public string? SubTitle { get; set; }
    
    [JsonPropertyName("icon")]
    public IconType? Icon { get; set; }

    [JsonPropertyName("typeIdentifier")]
    public char TypeIdentifier { get; init; } = 'F';
    
    [Required]
    [JsonPropertyName("fields")]
    public new FormElementFieldList Fields
    {
        get => _fields ?? [];
        private set
        {
            base.Fields = new ElementFieldList(value.GetElementFields());
            _fields = value;
        }
    }

    [Required] [JsonPropertyName("panels")] public List<FormElementPanel> Panels { get; set; }

    [Required]
    [JsonPropertyName("relations")]
    public new FormElementRelationshipList Relationships
    {
        get => _relationships ?? [];
        private set
        {
            base.Relationships = value.GetElementRelationships();
            _relationships = value;
        }
    }

    [Required] [JsonPropertyName("options")] public FormElementOptions Options { get; set; }

    [Required]
    [JsonPropertyName("apiOptions")]
    public FormElementApiOptions ApiOptions { get; set; }

    public FormElement()
    {
        Fields = new FormElementFieldList(base.Fields);
        Panels = [];
        Options = new FormElementOptions();
        Relationships = new FormElementRelationshipList(base.Relationships);
        ApiOptions = new FormElementApiOptions();
    }

    [SetsRequiredMembers]
    public FormElement(Element element)
    {
        Name = element.Name;
        Schema = element.Schema;
        TableName = element.TableName;
        Info = element.Info;
        Indexes = element.Indexes;
        Relationships = new FormElementRelationshipList(base.Relationships);
        ReadProcedureName = element.ReadProcedureName;
        WriteProcedureName = element.WriteProcedureName;
        EnableSynchronism = element.EnableSynchronism;
        SynchronismMode = element.SynchronismMode;
        Title = element.Name;
        SubTitle = element.Info;
        ConnectionId = element.ConnectionId;
        Fields = new FormElementFieldList(element.Fields);
        Panels = [];
        ApiOptions = new FormElementApiOptions();
        Options = new FormElementOptions();
    }

    [SetsRequiredMembers]
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
        base.Relationships =
            [..relationships.Where(r => r.ElementRelationship != null).
                Select(r => r.ElementRelationship).ToList()!];
        Relationships = relationships;
        Options = options ?? new FormElementOptions();
        ApiOptions = apiOptions ?? new FormElementApiOptions();
        Panels = panels ?? [];
    }

    private static void SetFieldType(ElementField field, Type type)
    {
        if (type == typeof(int) ||
            type == typeof(short) ||
            type == typeof(long) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong))
        {
            field.DataType = FieldType.Int;
        }
        else if (type == typeof(double) || type == typeof(float))
        {
            field.DataType = FieldType.Float;
        }
        else if (type == typeof(decimal))
        {
            field.DataType = FieldType.Decimal;
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

    public FormElement DeepCopy()
    {
        var copy = (FormElement)MemberwiseClone();

        copy.Fields = Fields.DeepCopy();
        copy.Options = Options.DeepCopy();
        copy.Panels = Panels.ConvertAll(p => p.DeepCopy());
        copy.Relationships = Relationships.DeepCopy();
        copy.Indexes = Indexes.ConvertAll(i => i.DeepCopy());
        copy.ApiOptions = ApiOptions.DeepCopy();

        return copy;
    }
}