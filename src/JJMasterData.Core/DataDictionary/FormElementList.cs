using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Lista de campos do formulário
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
public class FormElementList : ICollection<FormElementField>
{
    private IList<FormElementField> _formFields;
    private ElementList _baseFields;

    public FormElementList()
    {
        _baseFields = new ElementList();
        _formFields = new List<FormElementField>();
    }

    public FormElementList(ElementList baseFields, List<FormElementField> formFields)
    {
        _baseFields = baseFields;
        _formFields = formFields;
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

    public bool Contains(string fieldName)
    {
        foreach (FormElementField val in _formFields)
        {
            if (val.Name.Equals(fieldName))
                return true;
        }
        return false;
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

    public int Count
    {
        get { return _formFields.Count; }
    }

    public bool IsReadOnly
    {
        get { return _formFields.IsReadOnly; }
    }

    #endregion

    public FormElementField this[int index]
    {
        get
        {
            return _formFields[index];
        }
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

            foreach (FormElementField val in _formFields)
            {
                if (val.Name.ToLower().Equals(fieldName.ToLower()))
                    return val;
            }
            throw new ArgumentException(Translate.Key("value {0} not found", fieldName));
        }
        set
        {
            bool isOk = false;
            for (int i = 0; i < _formFields.Count; i++)
            {
                FormElementField e = _formFields[i];
                if (e.Name.ToLower().Equals(fieldName.ToLower()))
                {
                    _formFields[i] = value;
                    _baseFields[i] = value;
                    isOk = true;
                    break;
                }
            }
            if (!isOk)
                throw new ArgumentException(Translate.Key("value {0} not found", fieldName));
        }
    }

    public int IndexOf(string fieldName)
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
 
}