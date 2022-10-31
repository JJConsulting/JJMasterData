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
    private List<FormElementField> _formFields = new List<FormElementField>();
    
    [DataMember(Name = "title")]
    public string Title { get; set; }
    
    [DataMember(Name = "subTitle")]
    public string SubTitle { get; set; }
    
    [DataMember(Name = "fields")]
    public new FormElementList Fields { get; private set; }

    [DataMember(Name = "panels")]
    public List<FormElementPanel> Panels { get; set; }


    public FormElement()
    {
        Fields = new FormElementList(base.Fields, _formFields);
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
        Fields = new FormElementList(base.Fields, _formFields);
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
        Fields = new FormElementList(base.Fields, _formFields);
        foreach (DataColumn col in schema.Columns)
        {
            var f = new ElementField();
            f.Name = col.Caption;
            f.Label = col.Caption.Replace("::ASC", "").Replace("::DESC", "");
            f.Size = col.MaxLength;
            f.IsRequired = !col.AllowDBNull;
            f.IsPk = col.Unique;

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
                f.DataType = FieldType.Int;
            }
            else if (type == typeof(decimal) ||
                     type == typeof(double))
            {
                f.DataType = FieldType.Float;
            }
            else if (type == typeof(DateTime))
            {
                f.DataType = FieldType.Date;
            }
            else if (type == typeof(TimeSpan))
            {
                f.DataType = FieldType.DateTime;
            }
            else
            {
                f.DataType = FieldType.NVarchar;
            }

            AddField(f);
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
    
    [Obsolete("Please use DeepCopy extension method.")]
    public Element DeepCopyElement()
    {
        var element = new Element();
        element.Name = Name;
        element.TableName = TableName;
        element.Info = Info;
        element.Indexes = Indexes;
        element.Relations = Relations;
        element.CustomProcNameGet = CustomProcNameGet;
        element.CustomProcNameSet = CustomProcNameSet;
        element.SyncMode = SyncMode;
        element.Sync = Sync;

        element.Fields = new ElementList();
        foreach (var f in Fields)
            element.Fields.Add(f.DeepCopyField());

        return element;
    }

}