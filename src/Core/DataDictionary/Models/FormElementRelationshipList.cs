using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.DataDictionary;


public class FormElementRelationshipList : IList<FormElementRelationship>
{
    private readonly IList<FormElementRelationship> _formRelationships;
    private readonly List<ElementRelationship> _baseRelationships;
    
    [JsonConstructor]
    private FormElementRelationshipList(IList<FormElementRelationship> formRelationships)
    {
        _baseRelationships = formRelationships.Where(r => r.ElementRelationship != null)
            .Select(r => r.ElementRelationship).ToList();
        _formRelationships = formRelationships;
    }
    
    public FormElementRelationshipList(List<ElementRelationship> baseFields)
    {
        _baseRelationships = baseFields;
        _formRelationships = new List<FormElementRelationship>();
        if (baseFields.Count > 0)
        {
            _formRelationships.Add(new FormElementRelationship(true));
            foreach (var relation in baseFields)
            {
                _formRelationships.Add(new FormElementRelationship(relation));
            }
        }
    }

    public List<ElementRelationship> GetElementRelationships()
    {
        return _baseRelationships;
    }

    #region Implementation of IEnumerable

    public IEnumerator<FormElementRelationship> GetEnumerator()
    {
        return _formRelationships.GetEnumerator();
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
            _baseRelationships.Add(item.ElementRelationship);

        SetId(item);

        _formRelationships.Add(item);
    }

    private void SetId(FormElementRelationship item)
    {
        var highestId = _formRelationships.Any() ? _formRelationships.Max(x => x.Id) : 1;
        item.Id = highestId + 1;
    }

    public void Clear()
    {
        _baseRelationships.Clear();
        _formRelationships.Clear();
    }

    public bool Contains(FormElementRelationship item)
    {
        return _formRelationships.Contains(item);
    }

    public void CopyTo(FormElementRelationship[] array, int index)
    {
        _formRelationships.CopyTo(array, index);
    }

    public bool Remove(FormElementRelationship item)
    {
        if (item?.ElementRelationship != null)
            _baseRelationships.Remove(item.ElementRelationship);

        return _formRelationships.Remove(item);
    }

    public int Count => _formRelationships.Count;

    public bool IsReadOnly => _formRelationships.IsReadOnly;

    #endregion

    public int IndexOf(FormElementRelationship item)
    {
        return _formRelationships.IndexOf(item);
    }

    public void Insert(int index, FormElementRelationship item)
    {
        if (item?.ElementRelationship != null)
            _baseRelationships.Add(item.ElementRelationship);
        
        SetId(item);
        _formRelationships.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        var item = _formRelationships[index];

        _formRelationships.Remove(item);
        if (item?.ElementRelationship != null)
            _baseRelationships.Remove(item.ElementRelationship);
    }

    public FormElementRelationship this[int index]
    {
        get => _formRelationships[index];
        set
        {
            _formRelationships[index] = value;

            if (!value.IsParent)
            {
                var element = _baseRelationships.First(r => r.ChildElement == value.ElementRelationship!.ChildElement);
                var i = _baseRelationships.IndexOf(element);
                _baseRelationships[i] = value.ElementRelationship;
            }
        }
    }

    public FormElementRelationship GetById(int id)
    {
        return _formRelationships.First(r => r.Id == id);
    }
    
    public int GetIndexById(int id)
    {
        var relationship = GetById(id);
        return _formRelationships.IndexOf(relationship);
    }
}