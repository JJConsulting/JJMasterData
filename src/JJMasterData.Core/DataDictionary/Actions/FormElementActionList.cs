#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary.Action;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public abstract class FormElementActionList : IList<BasicAction>
{
    protected IList<BasicAction> List { get; } = new List<BasicAction>();

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

    public List<BasicAction> GetAll()
    {
        return List.OrderBy(x => x.Order).ToList();
    }

    public void Add(BasicAction item)
    {
        if (!item.IsUserCreated)
            throw new NotSupportedException("You cannot add non-user created actions.");
        
        List.Add(item);
    }

    public void Set(BasicAction item)
    {
        if (!item.IsUserCreated)
            throw new NotSupportedException("You cannot set non-user created actions.");
        
        var existingAction = List.FirstOrDefault(a => a.Name == item.Name);
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
        throw new NotSupportedException("Clearing all actions is not supported.");
    }

    public bool Contains(BasicAction item)
    {
        return List.Contains(item);
    }

    public void CopyTo(BasicAction[] array, int arrayIndex)
    {
        List.CopyTo(array,arrayIndex);
    }

    public bool Remove(BasicAction item)
    {
        if (item.IsUserCreated)
            List.Remove(item);
        
        throw new NotSupportedException("You cannot remove non user created actions");
    }

    public int Count => List.Count;
    public bool IsReadOnly => List.IsReadOnly;

    public int IndexOf(BasicAction item)
    {
        return List.IndexOf(item);
    }

    public void Insert(int index, BasicAction item)
    {
        List.Insert(index,item);
    }

    public void RemoveAt(int index)
    {
        List.RemoveAt(index);
    }

    public BasicAction this[int index]
    {
        get => List[index];
        set => List[index] = value;
    }
}