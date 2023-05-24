#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Action.Form;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class FormToolbarActions : IList<BasicAction>
{
    private readonly IList<BasicAction> _list;
    public FormToolbarActions()
    {
        _list = new List<BasicAction>
        {
            new SaveAction(),
            new CancelAction()
        };
    }

    public IEnumerator<BasicAction> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(BasicAction item)
    {
        switch (item)
        {
            case SqlCommandAction:
            case InternalAction:
                _list.Add(item);
                break;
            default:
                throw new NotSupportedException("BaseAction not supported.");
        }
    }

    public void Clear()
    {
        throw new NotSupportedException("Clearing all actions is not supported.");
    }

    public bool Contains(BasicAction item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(BasicAction[] array, int arrayIndex)
    {
        _list.CopyTo(array,arrayIndex);
    }

    public bool Remove(BasicAction item)
    {
        if (item.IsUserCreated)
            _list.Remove(item);
        throw new NotSupportedException("You cannot remove non user created actions");
    }

    public int Count => _list.Count;
    public bool IsReadOnly => _list.IsReadOnly;

    public int IndexOf(BasicAction item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, BasicAction item)
    {
        _list.Insert(index,item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public BasicAction this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }
}