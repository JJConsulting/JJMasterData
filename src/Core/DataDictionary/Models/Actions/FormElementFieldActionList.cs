using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class FormElementFieldActionList : FormElementActionList
{
    public FormElementFieldActionList() : base()
    {
  
    }
    
    [JsonConstructor]
    private FormElementFieldActionList(List<BasicAction> list)
    {
        List = list;
    }

    public FormElementFieldActionList DeepCopy()
    {
        return new FormElementFieldActionList(List.ConvertAll(a=>a.DeepCopy()));
    }
}