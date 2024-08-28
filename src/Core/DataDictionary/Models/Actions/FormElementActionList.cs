#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public abstract class FormElementActionList : IList<BasicAction>
{
    protected List<BasicAction> List { get; init; }
    protected FormElementActionList()
    {
        List = [];
    }

    [JsonConstructor]
    protected FormElementActionList(List<BasicAction> list)
    {
        List = list.ToList();
    }

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
            action.IsDefaultOption = action.Name.Equals(actionName);
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
    public bool IsReadOnly => (List as IList).IsReadOnly;

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
    
    protected T EnsureActionExists<T>() where T : BasicAction, new()
    {
        var action = List.OfType<T>().FirstOrDefault();
        
        if (action != null) 
            return action;
        
        action = new T();
        List.Add(action);
        
        return action;
    }
}