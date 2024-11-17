#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public abstract class FormElementActionList : IList<BasicAction>
{
    private List<BasicAction>? _list;
    [JsonIgnore]
    protected List<BasicAction> List => _list ??= InitializeList();
    
    
    [JsonPropertyName("sqlActions")]
    [JsonInclude] 
    protected internal List<SqlCommandAction> SqlActions { get; set; } = [];

    [JsonPropertyName("urlActions")]
    [JsonInclude] 
    protected internal List<UrlRedirectAction> UrlActions { get; set; } = [];

    [JsonPropertyName("htmlTemplateActions")]
    [JsonInclude] 
    protected internal List<HtmlTemplateAction> HtmlTemplateActions { get; set; } = [];

    [JsonPropertyName("jsActions")]
    [JsonInclude] 
    protected internal List<ScriptAction> JsActions { get; set; } = [];

    [JsonPropertyName("pluginActions")]
    [JsonInclude] 
    protected internal List<PluginAction> PluginActions { get; set; } = [];

    [JsonPropertyName("internalRedirectActions")]
    [JsonInclude] 
    protected internal List<InternalAction> InternalActions { get; set; } = [];
    

    private List<BasicAction> InitializeList()
    {
        var list = new List<BasicAction>();
        
        list.AddRange(SqlActions);
        list.AddRange(UrlActions);
        list.AddRange(InternalActions);
        list.AddRange(HtmlTemplateActions);
        list.AddRange(JsActions);
        list.AddRange(PluginActions);
        
        list.AddRange(GetActions());
        return list;
    }

    protected abstract IEnumerable<BasicAction> GetActions();

    public IEnumerator<BasicAction> GetEnumerator()
    {
        return List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public BasicAction Get(string actionName)
    {
        return List.First(a => a.Name == actionName);
    }

    public TAction? GetOrDefault<TAction>(string actionName) where TAction : BasicAction
    {
        return List.OfType<TAction>().FirstOrDefault(a => a.Name == actionName);
    }

    public List<BasicAction> GetAllSorted()
    {
        return List.OrderBy(x => x.Order).ToList();
    }

    public void SetDefaultOption(string actionName)
    {
        foreach (var action in List)
        {
            action.IsDefaultOption = action.Name.Equals(actionName, StringComparison.Ordinal);
        }
    }

    public void Add(BasicAction item)
    {
        List.Add(item);
    }

    public void AddRange(IEnumerable<BasicAction> items)
    {
        List.AddRange(items);
    }

    public void Set(BasicAction item)
    {
        var existingAction = List.Find(a => a.Name == item.Name);
        if (existingAction != null)
        {
            var index = List.IndexOf(existingAction);
            List[index] = item;
        }
        else
        {
            List.Add(item);
        }
    }

    public void Clear()
    {
        List.Clear();
    }

    public bool Contains(BasicAction item)
    {
        return List.Contains(item);
    }

    public void CopyTo(BasicAction[] array, int arrayIndex)
    {
        List.CopyTo(array, arrayIndex);
    }

    public bool Remove(BasicAction item)
    {
        return List.Remove(item);
    }

    public int Count => List.Count;
    public bool IsReadOnly => false;

    public int IndexOf(BasicAction item)
    {
        return List.IndexOf(item);
    }

    public void Insert(int index, BasicAction item)
    {
        List.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        List.RemoveAt(index);
    }

    public void RemoveAll(Predicate<BasicAction> match)
    {
        List.RemoveAll(match);
    }

    public List<BasicAction> FindAll(Predicate<BasicAction> match)
    {
        return List.FindAll(match);
    }

    public BasicAction this[int index]
    {
        get => List[index];
        set => List[index] = value;
    }
}