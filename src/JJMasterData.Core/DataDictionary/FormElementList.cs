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
public class FormElementList : IEnumerable<FormElementField>, ICollection<FormElementField>
{
    private IList<FormElementField> _FormFields;
    private ElementList _BaseFields;

    public FormElementList()
    {
        _BaseFields = new ElementList();
        _FormFields = new List<FormElementField>();
    }

    public FormElementList(ElementList baseFields, List<FormElementField> formFields)
    {
        _BaseFields = baseFields;
        _FormFields = formFields;
    }

    #region Implementation of IEnumerable

    public IEnumerator<FormElementField> GetEnumerator()
    {
        return _FormFields.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion


    #region Implementation of ICollection

    public void Add(FormElementField item)
    {
        _BaseFields.Add(item);
        _FormFields.Add(item);
    }

    public void Clear()
    {
        _BaseFields.Clear();
        _FormFields.Clear();
    }

    public bool Contains(string fieldName)
    {
        foreach (FormElementField val in _FormFields)
        {
            if (val.Name.Equals(fieldName))
                return true;
        }
        return false;
    }

    public bool Contains(FormElementField item)
    {
        return _FormFields.Contains(item);
    }

    public void CopyTo(FormElementField[] array, int arrayIndex)
    {
        _BaseFields.CopyTo(array.ToArray<ElementField>(), arrayIndex);
        _FormFields.CopyTo(array, arrayIndex);
    }

    public bool Remove(FormElementField item)
    {
        _BaseFields.Remove(item);
        return _FormFields.Remove(item);
    }

    public int Count
    {
        get { return _FormFields.Count; }
    }

    public bool IsReadOnly
    {
        get { return _FormFields.IsReadOnly; }
    }

    #endregion

    public FormElementField this[int index]
    {
        get
        {
            return _FormFields[index];
        }
        set
        {
            _FormFields[index] = value;
            _BaseFields[index] = value;
        }
    }

    public FormElementField this[string fieldName]
    {
        get
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            foreach (FormElementField val in _FormFields)
            {
                if (val.Name.ToLower().Equals(fieldName.ToLower()))
                    return val;
            }
            throw new ArgumentException(Translate.Key("value {0} not found", fieldName));
        }
        set
        {
            bool isOk = false;
            for (int i = 0; i < _FormFields.Count; i++)
            {
                FormElementField e = _FormFields[i];
                if (e.Name.ToLower().Equals(fieldName.ToLower()))
                {
                    _FormFields[i] = value;
                    _BaseFields[i] = value;
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
        for (int i = 0; i < _FormFields.Count; i++)
        {
            if (_FormFields[i].Name.Equals(fieldName))
            {
                index = i;
                break;
            }
        }
        return index;
    }

}