#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using JJConsulting.FontAwesome;
using JJConsulting.Html.Bootstrap.Models;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElement : Element
{
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
    public FontAwesomeIcon? Icon { get; set; }

    [JsonPropertyName("typeIdentifier")]
    public char TypeIdentifier { get; init; } = 'F';

    [Required]
    [JsonPropertyName("fields")]
    public new FormElementFieldList Fields
    {
        get;
        set
        {
            base.Fields = new ElementFieldList(value.GetElementFields());
            field = value;
        }
    }

    [Required] 
    [JsonPropertyName("panels")]
    public List<FormElementPanel> Panels { get; set; }

    [Required]
    [JsonPropertyName("relations")]
    public new FormElementRelationshipList Relationships
    {
        get;
        set
        {
            base.Relationships = value.GetElementRelationships();
            field = value;
        }
    }

    [Required] 
    [JsonPropertyName("options")] 
    public FormElementOptions Options { get; set; }

    [Required]
    [JsonPropertyName("rules")]
    public List<FormElementRule> Rules { get; set; }

    [Required]
    [JsonPropertyName("apiOptions")]
    public FormElementApiOptions ApiOptions { get; set; }

    public FormElement()
    {
        Fields = new FormElementFieldList(base.Fields);
        Panels = [];
        Options = new FormElementOptions();
        Rules = [];
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
        Rules = [];
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
        List<FormElementRule>? rules,
        FormElementApiOptions? apiOptions)
    {
        base.Fields = new ElementFieldList(fields.Cast<ElementField>().ToList());
        Fields = fields;
        base.Relationships = relationships
            .Where(r => r.ElementRelationship != null)
            .Select(r => r.ElementRelationship)
            .ToList()!;
        Relationships = relationships;
        Options = options ?? new FormElementOptions();
        Rules = rules ?? [];
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

    public BasicAction? GetActionOrDefault(string? name, ActionSource source, string? fieldName = null)
    {
        var actions = source switch
        {
            ActionSource.GridTable => Options.GridTableActions,
            ActionSource.GridToolbar => Options.GridToolbarActions,
            ActionSource.FormToolbar => Options.FormToolbarActions,
            ActionSource.Field => GetFieldActions(fieldName),
            _ => null
        };

        if (actions == null)
            return null;

        var isValid = string.IsNullOrWhiteSpace(name);
        
        return isValid
            ? actions.FirstOrDefault()
            : actions.FirstOrDefault(a => a.Name == name);
    }

    private IEnumerable<BasicAction>? GetFieldActions(string? fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return Fields.SelectMany(f => f.Actions);

        return Fields.FirstOrDefault(f => f.Name == fieldName)?.Actions;
    }

    public FormElement DeepCopy()
    {
        var copy = (FormElement)MemberwiseClone();

        copy.Fields = Fields.DeepCopy();
        copy.Options = Options.DeepCopy();
        copy.Rules = Rules.ConvertAll(v => v.DeepCopy());
        copy.Panels = Panels.ConvertAll(p => p.DeepCopy());
        copy.Relationships = Relationships.DeepCopy();
        copy.Indexes = Indexes.ConvertAll(i => i.DeepCopy());
        copy.ApiOptions = ApiOptions.DeepCopy();

        return copy;
    }

    public FormElementRule GetRuleById(int id)
    {
        return Rules.First(v => v.Id == id);
    }
}
