using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Dados do formuário, herda de Element
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
    // BsonSerializationException:
    // The property 'Name' of type 'JJMasterData.Core.DataDictionary.FormElement'
    // cannot use element name 'Name' because it is already being used by property 'Name' of type
    // 'JJMasterData.Commons.Dao.Entity.Element'.
    public FormElementList FormFields { get; private set; }

    [DataMember(Name = "panels")]
    public List<FormElementPanel> Panels { get; set; }


    public FormElement()
    {
        FormFields = new FormElementList(base.Fields, _formFields);
        Panels = new List<FormElementPanel>();
    }

    public FormElement(Element element)
    {
        Name = element.Name;
        TableName = element.TableName;
        Info = element.Info;
        Indexes = element.Indexes;
        Relations = element.Relations;
        CustomProcNameGet = element.CustomProcNameGet;
        CustomProcNameSet = element.CustomProcNameSet;
        Sync = element.Sync;
        SyncMode = element.SyncMode;
        Title = element.Name;
        SubTitle = element.Info;
        FormFields = new FormElementList(base.Fields, _formFields);
        Panels = new List<FormElementPanel>();
        foreach (var f in element.Fields)
        {
            AddField(f);
        }
    }

    public FormElement(DataTable schema)
    {
        if (schema == null)
            throw new ArgumentNullException(nameof(schema), Translate.Key("DataTable schema is null"));

        Name = schema.TableName;
        TableName = schema.TableName;
        Title = schema.TableName;
        Panels = new List<FormElementPanel>();
        FormFields = new FormElementList(base.Fields, _formFields);
        foreach (DataColumn col in schema.Columns)
        {
            var field = new ElementField
            {
                Name = col.Caption,
                Label = col.Caption.Replace("::ASC", "").Replace("::DESC", ""),
                Size = col.MaxLength,
                IsRequired = !col.AllowDBNull,
                IsPk = col.Unique
            };

            var type = col.DataType;
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

            AddField(field);
        }
    }

    private void AddField(ElementField field)
    {
        base.Fields.Add(field);
        _formFields.Add(new FormElementField(field));
    }

    public FormElementPanel GetPanelById(int id)
    {
        return Panels.Find(x => x.PanelId == id);
    }

}