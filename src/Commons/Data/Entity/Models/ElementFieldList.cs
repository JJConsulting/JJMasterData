using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Represents a collection of <see cref="ElementField"/> objects with additional utility methods for field management.
/// </summary>
/// <remarks>Originally created by JJTeam on 2017-03-22.</remarks>
public class ElementFieldList : IList<ElementField>
{
    private readonly List<ElementField> _list = [];

    public ElementFieldList()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementFieldList"/> class with an existing list of fields.
    /// </summary>
    /// <param name="fields">A list of <see cref="ElementField"/> objects to initialize the collection.</param>
    public ElementFieldList(List<ElementField> fields)
    {
        _list = fields;
    }

    #region Implementation of IEnumerable

    public IEnumerator<ElementField> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Determines whether a field with the specified name exists in the list.
    /// </summary>
    /// <param name="name">The name of the field to locate.</param>
    /// <returns><c>true</c> if a field with the specified name exists; otherwise, <c>false</c>.</returns>
    public bool ContainsKey(string name)
    {
        foreach (var field in _list)
        {
            if (field.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    #endregion

    #region Implementation of ICollection<T>

    /// <summary>
    /// Adds a new field to the collection.
    /// </summary>
    /// <param name="item">The <see cref="ElementField"/> object to add.</param>
    /// <exception cref="ArgumentException">Thrown if the item is null or already exists in the collection.</exception>
    public void Add(ElementField item)
    {
        if (item == null)
            throw new ArgumentException("ElementField can not be null");

        if (_list.Any(x => x.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase)))
            throw new JJMasterDataException($"Field [{item.Name}] already exists in Element.Fields");

        _list.Add(item);
    }


    /// <summary>
    /// Adds a new field with the specified properties.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="label">The label or description.</param>
    /// <param name="dataType">The data type of the field.</param>
    /// <param name="size">The size of the field.</param>
    /// <param name="required">Indicates whether the field is required.</param>
    /// <param name="filterMode">The filter mode of the field.</param>
    public void Add(string name, string label, FieldType dataType, int size, bool required, FilterMode filterMode)
    {
        var field = new ElementField
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

        _list.Add(field);
    }


    /// <summary>
    /// Adds a new field with the specified properties.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="label">The label or description.</param>
    /// <param name="dataType">The data type of the field.</param>
    /// <param name="size">The size of the field.</param>
    /// <param name="required">Indicates whether the field is required.</param>
    /// <param name="filterMode">The filter mode of the field.</param>
    /// <param name="dataBehavior">Specifies the behavior of the field.</param>
    public void Add(string name, string label, FieldType dataType, int size, bool required, FilterMode filterMode,
        FieldBehavior dataBehavior)
    {
        var field = new ElementField
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

        _list.Add(field);
    }


    /// <summary>
    /// Adds a new primary key field.
    /// </summary>
    /// <param name="name">The primary key field name.</param>
    /// <param name="label">The label or description.</param>
    /// <param name="dataType">The data type of the field.</param>
    /// <param name="size">The size of the field.</param>
    /// <param name="autoNum">Indicates whether the field is auto-incremented.</param>
    /// <param name="filterMode">The filter mode of the field.</param>
    public void AddPk(string name, string label, FieldType dataType, int size, bool autoNum, FilterMode filterMode)
    {
        var field = new ElementField
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

        _list.Add(field);
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

    public bool IsReadOnly => false;

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
            foreach (ElementField val in _list)
            {
                if (val.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                    return val;
            }

            throw new ArgumentException($"Value {fieldName} not found");
        }
        set
        {
            bool isOk = false;
            for (int i = 0; i < _list.Count; i++)
            {
                ElementField e = _list[i];
                if (e.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    _list[i] = value;
                    isOk = true;
                    break;
                }
            }

            if (!isOk)
                throw new ArgumentException($"Value {fieldName} not found");
        }
    }

    #endregion

    public List<ElementField> GetAsList()
    {
        return _list;
    }

    public List<ElementField> FindAll(Predicate<ElementField> predicate)
    {
        return _list.FindAll(predicate);
    }

    public void AddRange(IEnumerable<ElementField> fields)
    {
        _list.AddRange(fields);
    }

    public ElementFieldList DeepCopy()
    {
        return new(_list.ConvertAll(f => f.DeepCopy()));
    }
}