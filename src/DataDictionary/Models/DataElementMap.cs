using System.Collections;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;


public class DataElementMap
{
    [JsonProperty("elementName")]
    public string ElementName { get; set; }

    [JsonProperty("fieldKey")]
    public string FieldKey { get; set; }

    [JsonProperty("fieldDescription")]
    public string FieldDescription { get; set; }

    [JsonProperty("popUpSize")]
    public PopupSize PopUpSize { get; set; }

    public Hashtable Filters 
    {
        get
        {
            var _filters = new Hashtable();
            
            if (MapFilters != null)
            {
                foreach (var item in MapFilters)
                    _filters.Add(item.FieldName, item.ExpressionValue);
            }

                
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

    [JsonProperty("mapFilters")]
    public List<DataElementMapFilter> MapFilters { get; set; }
        

    [JsonProperty("enableElementActions")]
    public bool EnableElementActions { get; set; }

    public DataElementMap()
    {
        MapFilters = new List<DataElementMapFilter>();
    }
}