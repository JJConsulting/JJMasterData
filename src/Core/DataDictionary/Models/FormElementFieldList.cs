#nullable enable
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Implementation of IList representing a list of data dictionary fields.
/// </summary>
public class FormElementFieldList : IList<FormElementField>
{
    private readonly IList<FormElementField> _formFields;
    private readonly ElementFieldList _baseFields;
    
    public FormElementFieldList()
    {
        _baseFields = new ElementFieldList();
        _formFields = new List<FormElementField>();
    }
    [JsonConstructor]
    private FormElementFieldList(IList<FormElementField> formFields)
    {
        _baseFields = new ElementFieldList(formFields.Cast<ElementField>().ToList());
        _formFields = formFields;
    }
    
    public FormElementFieldList(ElementFieldList baseFields)
    {
        _baseFields = baseFields;

        _formFields = new List<FormElementField>();

        foreach (var field in _baseFields)
        {
            _formFields.Add(new FormElementField(field));
        }
    }

    #region Implementation of IEnumerable

    public IEnumerator<FormElementField> GetEnumerator()
    {
        return _formFields.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion


    #region Implementation of ICollection

    public void Add(FormElementField item)
    {
        _baseFields.Add(item);
        _formFields.Add(item);
    }

    public void Clear()
    {
        _baseFields.Clear();
        _formFields.Clear();
    }

    public bool Contains(string? fieldName)
    {
        return _formFields.Any(val => val.Name.Equals(fieldName));
    }

    public bool Contains(FormElementField item)
    {
        return _formFields.Contains(item);
    }

    public void CopyTo(FormElementField[] array, int arrayIndex)
    {
        _baseFields.CopyTo(array.ToArray<ElementField>(), arrayIndex);
        _formFields.CopyTo(array, arrayIndex);
    }

    public bool Remove(FormElementField item)
    {
        _baseFields.Remove(item);
        return _formFields.Remove(item);
    }

    public int Count => _formFields.Count;

    public bool IsReadOnly => _formFields.IsReadOnly;

    #endregion

    public int IndexOf(FormElementField item) => _formFields.IndexOf(item);

    public void Insert(int index, FormElementField item)
    {
        _formFields.Insert(index,item);
        _baseFields.Insert(index,item);
    }

    public void RemoveAt(int index)
    {
        _formFields.RemoveAt(index);
        _baseFields.RemoveAt(index);
    }

    public FormElementField this[int index]
    {
        get => _formFields[index];
        set
        {
            _formFields[index] = value;
            _baseFields[index] = value;
        }
    }

    public FormElementField this[string fieldName]
    {
        get
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            foreach (var val in _formFields)
            {
                if (val.Name.ToLower().Equals(fieldName.ToLower()))
                    return val;
            }
            throw new KeyNotFoundException($"Field {fieldName} not found.");
        }
        set
        {
            var isOk = false;
            for (var i = 0; i < _formFields.Count; i++)
            {
                if (!_formFields[i].Name.ToLower().Equals(fieldName.ToLower()))
                    continue;
                
                _formFields[i] = value;
                _baseFields[i] = value;
                isOk = true;
                break;
            }
            if (!isOk)
                throw new KeyNotFoundException($"Field {fieldName} not found.");
        }
    }

    public int IndexOf(string? fieldName)
    {
        int index = -1;
        for (int i = 0; i < _formFields.Count; i++)
        {
            if (_formFields[i].Name.Equals(fieldName))
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public bool TryGetField(string fieldName, out FormElementField? formElementField)
    {
        if (Contains(fieldName))
        {
            formElementField = this[fieldName];
            return true;
        }

        formElementField = null;
        return false;
    }
 
}