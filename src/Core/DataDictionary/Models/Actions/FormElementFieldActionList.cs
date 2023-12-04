using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class FormElementFieldActionList : FormElementActionList
{
    public FormElementFieldActionList() : base()
    {
  
    }
    
    [JsonConstructor]
    private FormElementFieldActionList(List<BasicAction> list)
    {
        List = list;
    }
}