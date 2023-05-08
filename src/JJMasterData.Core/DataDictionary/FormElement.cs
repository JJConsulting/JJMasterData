using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Form data
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class FormElement : Element
{
    private List<FormElementField> _formFields = new();
    
    [DataMember(Name = "title")]
    public string Title { get; set; }
    
    [DataMember(Name = "subTitle")]
    public string SubTitle { get; set; }
    
    [DataMember(Name = "fields")]
    public new FormElementList Fields { get; private set; }

    [DataMember(Name = "panels")]
    public List<FormElementPanel> Panels { get; set; }

    [DataMember(Name = "relationshipsLayout")]
    public FormElementRelationshipList Relationships2 { get; set; }

    public FormElement()
    {
        Fields = new FormElementList(base.Fields, _formFields);
        Panels = new List<FormElementPanel>();
        Relationships2 = new FormElementRelationshipList(base.Relationships);
    }

    public FormElement(Element element) 
    {
        Name = element.Name;
        TableName = element.TableName;
        Info = element.Info;
        Indexes = element.Indexes;
        base.Relationships = element.Relationships;
        Relationships2 = new FormElementRelationshipList(base.Relationships);
        CustomProcNameGet = element.CustomProcNameGet;
        CustomProcNameSet = element.CustomProcNameSet;
        Sync = element.Sync;
        SyncMode = element.SyncMode;
        Title = element.Name;
        SubTitle = element.Info;
        Fields = new FormElementList(base.Fields, _formFields);
        Panels = new List<FormElementPanel>();
        foreach (var f in element.Fields)
        {
            AddField(f);
        }
    }

    public FormElement(DataTable schema) : this()
    {
        if (schema == null)
            throw new ArgumentNullException(nameof(schema), "DataTable schema cannot be null");

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
        base.Fields.Add(field);
        _formFields.Add(new FormElementField(field));
    }

    public FormElementPanel GetPanelById(int id)
    {
        return Panels.Find(x => x.PanelId == id);
    }

}