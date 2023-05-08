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
    private IList<ElementRelationship> baseRelationships;

    public FormElementRelationshipList()
    {
        baseRelationships = new List<ElementRelationship>();
        formRelationships = new List<FormElementRelationship>();
        formRelationships.Add(new FormElementRelationship(true));
    }

    public FormElementRelationshipList(IList<ElementRelationship> baseFields)
    {
        baseRelationships = baseFields;
        formRelationships = new List<FormElementRelationship>();
        formRelationships.Add(new FormElementRelationship(true));
        foreach (var relation in baseFields)
        {
            formRelationships.Add(new FormElementRelationship(relation));
        }
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
        if (item?.ElementRelation != null)
            baseRelationships.Add(item.ElementRelation);

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

    public void CopyTo(FormElementRelationship[] array, int arrayIndex)
    {
        baseRelationships.CopyTo(array.Select(x => x.ElementRelation).ToArray(), arrayIndex);
        formRelationships.CopyTo(array, arrayIndex);
    }

    public bool Remove(FormElementRelationship item)
    {
        if (item?.ElementRelation != null)
            baseRelationships.Remove(item.ElementRelation);

        return formRelationships.Remove(item);
    }

    public int Count
    {
        get { return formRelationships.Count; }
    }

    public bool IsReadOnly
    {
        get { return formRelationships.IsReadOnly; }
    }

    #endregion

    public int IndexOf(FormElementRelationship item)
    {
        return formRelationships.IndexOf(item);
    }

    public void Insert(int index, FormElementRelationship item)
    {
         formRelationships.Insert(index, item);
         if (item?.ElementRelation != null)
            baseRelationships.Insert(index, item.ElementRelation);
    }

    public void RemoveAt(int index)
    {
        var item = formRelationships[index];

        formRelationships.Remove(item);
        if (item?.ElementRelation != null)
            baseRelationships.Remove(item.ElementRelation);
    }

    public FormElementRelationship this[int index]
    {
        get
        {
            return formRelationships[index];
        }
        set
        {
            formRelationships[index] = value;
            baseRelationships[index] = value.ElementRelation;
        }
    }

   

   
 
}