using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.DictionaryDAL;

[DataContract]
public class DicFormParser
{
    [DataMember(Name = "formfields")]
    public List<DicFormFieldParser> FormFields { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }

    [DataMember(Name = "subtitle")]
    public string SubTitle { get; set; }

    [DataMember(Name = "panels")]
    public List<FormElementPanel> Panels { get; set; }


    public DicFormParser() 
    {
        Panels = new List<FormElementPanel>();
        FormFields = new List<DicFormFieldParser>();
    }

    public DicFormParser(FormElement e) : this()
    {
        Title = e.Title;
        SubTitle = e.SubTitle;
        Panels = e.Panels;
        foreach (var f in e.Fields)
        {
            FormFields.Add(new DicFormFieldParser(f));
        }
    }
}