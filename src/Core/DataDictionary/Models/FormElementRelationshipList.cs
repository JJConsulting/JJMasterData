using System.Collections;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;


public class FormElementRelationshipList : IList<FormElementRelationship>
{
    private readonly IList<FormElementRelationship> formRelationships;
    private readonly System.Collections.Generic.List<ElementRelationship> baseRelationships;
    
    [JsonConstructor]
    private FormElementRelationshipList(IList<FormElementRelationship> formRelationships)
    {
        baseRelationships = formRelationships.Where(r => r.ElementRelationship != null)
            .Select(r => r.ElementRelationship).ToList();
        this.formRelationships = formRelationships;
    }
    
    public FormElementRelationshipList(System.Collections.Generic.List<ElementRelationship> baseFields)
    {
        baseRelationships = baseFields;
        formRelationships = new System.Collections.Generic.List<FormElementRelationship>();
        if (baseFields.Count > 0)
        {
            formRelationships.Add(new FormElementRelationship(true));
            foreach (var relation in baseFields)
            {
                formRelationships.Add(new FormElementRelationship(relation));
            }
        }
    }

    public System.Collections.Generic.List<ElementRelationship> GetElementRelationships()
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
        if (item!.ElementRelationship != null)
            baseRelationships.Add(item.ElementRelationship);

        SetId(item);

        formRelationships.Add(item);
    }

    private void SetId(FormElementRelationship item)
    {
        var highestId = formRelationships.Any() ? formRelationships.Max(x => x.Id) : 1;
        item.Id = highestId + 1;
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
        if (item?.ElementRelationship != null)
            baseRelationships.Add(item.ElementRelationship);
        
        SetId(item);
        formRelationships.Insert(index, item);
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

            if (!value.IsParent)
            {
                var element = baseRelationships.First(r => r.ChildElement == value.ElementRelationship!.ChildElement);
                var i = baseRelationships.IndexOf(element);
                baseRelationships[i] = value.ElementRelationship;
            }
        }
    }

    public FormElementRelationship GetById(int id)
    {
        return formRelationships.First(r => r.Id == id);
    }
    
    public int GetIndexById(int id)
    {
        var relationship = GetById(id);
        return formRelationships.IndexOf(relationship);
    }
}