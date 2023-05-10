using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
public class FormElementRelationshipList : IList<FormElementRelationship>
{
    private IList<FormElementRelationship> formRelationships;
    private List<ElementRelationship> baseRelationships;

    //public FormElementRelationshipList()
    //{
    //    baseRelationships = new List<ElementRelationship>();
    //    formRelationships = new List<FormElementRelationship>();
    //    formRelationships.Add(new FormElementRelationship(true));
    //}

    public FormElementRelationshipList(List<ElementRelationship> baseFields)
    {
        baseRelationships = baseFields;
        formRelationships = new List<FormElementRelationship>();
        if (baseFields.Count > 0)
        {
            formRelationships.Add(new FormElementRelationship(true));
            foreach (var relation in baseFields)
            {
                formRelationships.Add(new FormElementRelationship(relation));
            }
        }
    }

    public List<ElementRelationship> GetElementRelationships()
    {
        return baseRelationships;
    }

    #region Implementation of IEnumerable

    public IEnumerator<FormElementRelationship> GetEnumerator()
    {
        return formRelationships.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion


    #region Implementation of ICollection

    public void Add(FormElementRelationship item)
    {
        if (item?.ElementRelationship != null)
            baseRelationships.Add(item.ElementRelationship);

        formRelationships.Add(item);
    }

    public void Clear()
    {
        baseRelationships.Clear();
        formRelationships.Clear();
    }

    public bool Contains(FormElementRelationship item)
    {
        return formRelationships.Contains(item);
    }

    public void CopyTo(FormElementRelationship[] array, int index)
    {
        baseRelationships.CopyTo(array.Select(x => x.ElementRelationship).ToArray(), GetIndexRelativeToParent(index));
        formRelationships.CopyTo(array, index);
    }

    public bool Remove(FormElementRelationship item)
    {
        if (item?.ElementRelationship != null)
            baseRelationships.Remove(item.ElementRelationship);

        return formRelationships.Remove(item);
    }

    public int Count => formRelationships.Count;

    public bool IsReadOnly => formRelationships.IsReadOnly;

    #endregion

    public int IndexOf(FormElementRelationship item)
    {
        return formRelationships.IndexOf(item);
    }

    public void Insert(int index, FormElementRelationship item)
    {
        formRelationships.Insert(index, item);
        if (item?.ElementRelationship != null)
            baseRelationships.Insert(GetIndexRelativeToParent(index), item.ElementRelationship);
    }

    public void RemoveAt(int index)
    {
        var item = formRelationships[index];

        formRelationships.Remove(item);
        if (item?.ElementRelationship != null)
            baseRelationships.Remove(item.ElementRelationship);
    }

    public FormElementRelationship this[int index]
    {
        get => formRelationships[index];
        set
        {
            formRelationships[index] = value;
            
            if (index != GetParentIndex())
            {
                baseRelationships[GetIndexRelativeToParent(index)] = value.ElementRelationship;
            }
        }
    }

    public FormElementRelationship GetParent()
    {
        return formRelationships.First(r => r.IsParent);
    }
    
    private int GetParentIndex()
    {
        return formRelationships.IndexOf(GetParent());
    }
    
    private int GetIndexRelativeToParent(int index)
    {
        var parentIndex = GetParentIndex();

        return index < parentIndex ? index : parentIndex;
    }
}