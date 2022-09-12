using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class DataElementMap
{
    [DataMember(Name = "elementName")]
    public string ElementName { get; set; }

    [DataMember(Name = "fieldKey")]
    public string FieldKey { get; set; }

    [DataMember(Name = "fieldDescription")]
    public string FieldDescription { get; set; }

    [DataMember(Name = "popUpSize")]
    public PopupSize PopUpSize { get; set; }

    public Hashtable Filters 
    {
        get
        {
            var _filters = new Hashtable();
            foreach (var item in MapFilters)
                _filters.Add(item.FieldName, item.ExpressionValue);
                
            return _filters;
        }
        set
        {
            MapFilters.Clear();
            foreach (DictionaryEntry s in value)
            {
                var mapFilter = new DataElementMapFilter
                {
                    FieldName = s.Key.ToString(),
                    ExpressionValue = s.ToString()
                };
                MapFilters.Add(mapFilter);
            }
        }
    }

    [DataMember(Name = "mapFilters")]
    public List<DataElementMapFilter> MapFilters { get; set; }
        

    [DataMember(Name = "enableElementActions")]
    public bool EnableElementActions { get; set; }

    public DataElementMap()
    {
        MapFilters = new List<DataElementMapFilter>();
    }
}