#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;

public abstract class FormElementActionList : IList<BasicAction>
{
    protected IList<BasicAction> List { get; set; }
    protected FormElementActionList()
    {
        List = new List<BasicAction>();
    }
    
    [JsonConstructor]
    protected FormElementActionList(IList<BasicAction> list)
    {
        List = list;
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

    public List<BasicAction> GetAll()
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
        ValidateAction(item);
        List.Add(item);
    }
    

    public void Set(BasicAction item)
    {
        if (!item.IsUserCreated)
            throw new NotSupportedException("You cannot set non-user created actions.");
        
        ValidateAction(item);
        
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
            return List.Remove(item);
       
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
        ValidateAction(item);
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
    
    private static void ValidateAction(BasicAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrEmpty(action.Name))
            throw new ArgumentException("Property name action is not valid");
    }
}