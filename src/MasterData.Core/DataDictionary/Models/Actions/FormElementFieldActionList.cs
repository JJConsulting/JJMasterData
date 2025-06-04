using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class FormElementFieldActionList : FormElementActionList
{
    public FormElementFieldActionList() 
    {
        
    }
    
    private FormElementFieldActionList(List<BasicAction> actions) : base(actions)
    {
     
    }

    internal List<PluginFieldAction> PluginFieldActions
    {
        get => List.OfType<PluginFieldAction>().ToList();
        init
        {
            List.RemoveAll(a => a is PluginFieldAction);
            List.AddRange(value);
        }
    }

    public FormElementFieldActionList DeepCopy()
    {
        return new FormElementFieldActionList(List.ConvertAll(a => a.DeepCopy()));
    }
}