using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;

namespace JJMasterData.Commons.Dao.Entity;

/// <summary>
/// Table Field List
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
public class ElementList : IList<ElementField>
{
    private readonly IList<ElementField> _list = new List<ElementField>();

    #region Implementation of IEnumerable

    public IEnumerator<ElementField> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool ContainsKey(string name)
    {
        return _list.Any(field => field.Name.ToLower().Equals(name.ToLower()));
    }

    #endregion

    #region Implementation of ICollection<T>


    /// <summary>
    /// Add Field
    /// </summary>
    /// <param name="item">Object with field information</param>
    public void Add(ElementField item)
    {
        if (item == null)
            throw new ArgumentException(Translate.Key("ElementField can not be null"));

        int qtd = _list.Count(x => x.Name.Equals(item.Name));
        if (qtd > 0)
            throw new JJMasterDataException(Translate.Key("Field [{0}] already exists in Element.Fields", item.Name));

        _list.Add(item);
    }


    /// <summary>
    /// Add Field
    /// </summary>
    /// <param name="name">Column Name</param>
    /// <param name="label">Description on the form</param>
    /// <param name="dataType">Data Type</param>
    /// <param name="size">Field Size</param>
    /// <param name="required">Required Field</param>
    /// <param name="filterMode">Filter Type</param>
    public void Add(string name, string label, FieldType dataType, int size, bool required, FilterMode filterMode)
    {
        var e = new ElementField
        {
            Name = name,
            Label = label,
            DataType = dataType,
            Size = size,
            IsPk = false,
            IsRequired = required,
            AutoNum = false,
            Filter = new ElementFilter(filterMode),
            DataBehavior = FieldBehavior.Real
        };

        _list.Add(e);
    }


    /// <summary>
    /// Add Field
    /// </summary>
    /// <param name="name">Column Name</param>
    /// <param name="label">Description on the form</param>
    /// <param name="dataType">Data Type</param>
    /// <param name="size">Field Size</param>
    /// <param name="required">Required field</param>
    /// <param name="filterMode">Filter type</param>
    /// <param name="dataBehavior">Specifies the behavior of the field.</param>
    public void Add(string name, string label, FieldType dataType, int size, bool required, FilterMode filterMode, FieldBehavior dataBehavior)
    {
        var elementField = new ElementField
        {
            Name = name,
            Label = label,
            DataType = dataType,
            Size = size,
            IsPk = false,
            IsRequired = required,
            AutoNum = false,
            Filter = new ElementFilter(filterMode),
            DataBehavior = dataBehavior
        };

        _list.Add(elementField);
    }

    /// <summary>
    /// Add primary key
    /// </summary>
    /// <param name="name">Column Name</param>
    /// <param name="label">Description on the form</param>
    /// <param name="dataType">Data Type</param>
    /// <param name="size">Field Size</param>
    /// <param name="autoNum">Auto Numerical (Identity)</param>
    /// <param name="filterMode">Filter type</param>
    public void AddPK(string name, string label, FieldType dataType, int size, bool autoNum, FilterMode filterMode)
    {
        var elementField = new ElementField
        {
            Name = name,
            Label = label,
            DataType = dataType,
            Size = size,
            IsPk = true,
            IsRequired = true,
            AutoNum = autoNum,
            Filter = new ElementFilter(filterMode),
            DataBehavior = FieldBehavior.Real
        };

        _list.Add(elementField);
    }

    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(ElementField item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(ElementField[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public bool Remove(ElementField item)
    {
        return _list.Remove(item);
    }

    public int Count => _list.Count;

    public bool IsReadOnly => _list.IsReadOnly;

    #endregion

    #region Implementation of IList<T>

    public int IndexOf(ElementField item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, ElementField item)
    {
        _list.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public ElementField this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }

    public ElementField this[string fieldName]
    {
        get
        {
            foreach (var val in _list)
            {
                if (val.Name.ToLower().Equals(fieldName.ToLower()))
                    return val;
            }

            throw new ArgumentException(Translate.Key("Value {0} not found", fieldName));
        }
        set
        {
            bool isOk = false;
            for (int i = 0; i < _list.Count; i++)
            {
                var field = _list[i];
                if (field.Name.ToLower().Equals(fieldName.ToLower()))
                {
                    _list[i] = value;
                    isOk = true;
                    break;
                }
            }
            if (!isOk)
                throw new ArgumentException(Translate.Key("Value {0} not found", fieldName));
        }
    }


    #endregion
}