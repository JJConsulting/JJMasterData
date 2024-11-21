#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public abstract class FormElementActionList : IList<BasicAction>
{
    [JsonIgnore] protected List<BasicAction> List { get; }

    [JsonPropertyName("sqlActions")]
    [JsonInclude]
    protected internal List<SqlCommandAction> SqlActions => List.OfType<SqlCommandAction>().ToList();

    [JsonPropertyName("urlActions")]
    [JsonInclude]
    protected internal List<UrlRedirectAction> UrlActions => List.OfType<UrlRedirectAction>().ToList();

    [JsonPropertyName("htmlTemplateActions")]
    [JsonInclude]
    protected internal List<HtmlTemplateAction> HtmlTemplateActions => List.OfType<HtmlTemplateAction>().ToList();

    [JsonPropertyName("jsActions")]
    [JsonInclude]
    protected internal List<ScriptAction> JsActions => List.OfType<ScriptAction>().ToList();

    [JsonPropertyName("pluginActions")]
    [JsonInclude]
    protected internal List<PluginAction> PluginActions => List.OfType<PluginAction>().ToList();

    [JsonPropertyName("internalRedirectActions")]
    [JsonInclude]
    protected internal List<InternalAction> InternalActions => List.OfType<InternalAction>().ToList();

    protected FormElementActionList()
    {
        List = [];
    }

    protected FormElementActionList(List<BasicAction> list)
    {
        List = list;
    }

    public IEnumerator<BasicAction> GetEnumerator() => List.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public BasicAction Get(string actionName) => List.First(a => a.Name == actionName);

    public TAction? GetOrDefault<TAction>(string actionName) where TAction : BasicAction =>
        List.OfType<TAction>().FirstOrDefault(a => a.Name == actionName);

    public List<BasicAction> GetAllSorted() => List.OrderBy(x => x.Order).ToList();

    public void SetDefaultOption(string actionName)
    {
        foreach (var action in List)
            action.IsDefaultOption = action.Name.Equals(actionName, StringComparison.Ordinal);
    }

    public void Add(BasicAction item) => List.Add(item);
    public void AddRange(IEnumerable<BasicAction> items) => List.AddRange(items);

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

    public void Clear() => List.Clear();
    public bool Contains(BasicAction item) => List.Contains(item);
    public void CopyTo(BasicAction[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);
    public bool Remove(BasicAction item) => List.Remove(item);
    public int Count => List.Count;
    public bool IsReadOnly => false;
    public int IndexOf(BasicAction item) => List.IndexOf(item);
    public void Insert(int index, BasicAction item) => List.Insert(index, item);
    public void RemoveAt(int index) => List.RemoveAt(index);
    public void RemoveAll(Predicate<BasicAction> match) => List.RemoveAll(match);
    public List<BasicAction> FindAll(Predicate<BasicAction> match) => List.FindAll(match);

    public BasicAction this[int index]
    {
        get => List[index];
        set => List[index] = value;
    }
    
    protected T GetOrAdd<T>() where T : BasicAction, new()
    {
        var existingItem = List.OfType<T>().FirstOrDefault();
        
        if (existingItem != null) 
            return existingItem;
        
        var item = new T();
        
        List.Add(item);
        
        return item;
    }
}