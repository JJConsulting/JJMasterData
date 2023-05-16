#nullable enable
using System;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class RelationshipsService : BaseService
{
    private readonly PanelService _panelService;

    public RelationshipsService(
        IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        PanelService panelService)
        : base(validationDictionary, dataDictionaryRepository)
    {
        _panelService = panelService;
    }

    public void SaveElementRelationship(ElementRelationship elementRelationship, int? id, string dictionaryName)
    {
        var formElement = DataDictionaryRepository.GetMetadata(dictionaryName);
        var relationships = formElement.Relationships;

        var formElementRelationShip = new FormElementRelationship(elementRelationship);
        
        if (id != null)
        {
            var index = relationships.GetIndexById(id.Value);
            relationships[index] = formElementRelationShip;
        }
        else
        {
            relationships.Add(formElementRelationShip);
        }

        if (!relationships.Any(r => r.IsParent))
        {
            relationships.Add(new FormElementRelationship(true));
        }

        DataDictionaryRepository.InsertOrReplace(formElement);
    }

    public void SaveFormElementRelationship(FormElementPanel panel, RelationshipViewType viewType, int id,
        string dictionaryName)
    {
        var formElement = DataDictionaryRepository.GetMetadata(dictionaryName);

        var index = formElement.Relationships.GetIndexById(id);
        
        var relationship = formElement.Relationships[index];
        relationship.ViewType = viewType;
        relationship.Panel = panel;
        
        formElement.Relationships[index] = relationship;

        DataDictionaryRepository.InsertOrReplace(formElement);
    }

    public bool ValidateRelation(string dictionaryName, string childElementName, string pkColumnName,
        string fkColumnName)
    {
        if (string.IsNullOrEmpty(childElementName))
        {
            AddError("ChildElement", Translate.Key("Required ChildElement Field"));
            return IsValid;
        }

        if (!DataDictionaryRepository.Exists(childElementName))
        {
            AddError("Entity", Translate.Key("Entity {0} not found", childElementName));
            return IsValid;
        }

        if (string.IsNullOrEmpty(pkColumnName))
        {
            AddError("PkColumn", Translate.Key("Required PkColumn field"));
            return IsValid;
        }

        if (string.IsNullOrEmpty(fkColumnName))
        {
            AddError("FkColumn", Translate.Key("Required FkColumn field"));
            return IsValid;
        }

        var fkColumn = GetField(childElementName, fkColumnName);
        if (fkColumn == null)
        {
            AddError("", Translate.Key("Column {0} not found in {1}.", fkColumnName, childElementName));
            return IsValid;
        }

        var pkColumn = GetField(dictionaryName, pkColumnName);
        if (pkColumn == null)
        {
            AddError("", Translate.Key("Column {0} not found.", pkColumnName));
            return IsValid;
        }

        if (fkColumn.Filter.Type == FilterMode.None && !fkColumn.IsPk)
        {
            AddError("", Translate.Key("Column {0} has no filter or is not a primary key.", fkColumnName));
            return IsValid;
        }

        if (pkColumn.DataType != fkColumn.DataType)
        {
            AddError("",
                Translate.Key("Column {0} has incompatible types with column {1}", pkColumnName, fkColumnName));
            return IsValid;
        }

        return IsValid;
    }

    private ElementField? GetField(string dictionaryName, string fieldName)
    {
        var formElement = DataDictionaryRepository.GetMetadata(dictionaryName);
        return formElement.Fields.Contains(fieldName) ? formElement.Fields[fieldName] : null;
    }


    public bool ValidateElementRelationship(ElementRelationship elementRelationship, string dictionaryName,
        int? index)
    {
        if (string.IsNullOrWhiteSpace(elementRelationship.ChildElement))
            AddError("", Translate.Key("Mandatory <b>PKTable </b> field"));

        if (IsValid)
        {
            if (elementRelationship.Columns.Count == 0)
                AddError("", Translate.Key("No relationship registered."));
        }

        if (IsValid)
        {
            foreach (var r in elementRelationship.Columns)
            {
                ValidateRelation(dictionaryName, elementRelationship.ChildElement, r.PkColumn, r.FkColumn);
            }
        }

        if (IsValid && index == null)
        {
            var relationships = GetFormElement(dictionaryName).Relationships
                .GetElementRelationships().FindAll(x => x.ChildElement.Equals(elementRelationship.ChildElement));
            if (relationships.Count > 0)
                AddError("",
                    Translate.Key("There is already a relationship registered for ") +
                    elementRelationship.ChildElement);
        }

        return IsValid;
    }

    public bool ValidateFinallyAddRelation(string dictionaryName, ElementRelationship elementRelationship,
        string pkColumnName, string fkColumnName)
    {
        if (ValidateRelation(dictionaryName, elementRelationship.ChildElement, pkColumnName, fkColumnName))
        {
            var list = elementRelationship.Columns.FindAll(x =>
                x.PkColumn.Equals(pkColumnName) &&
                x.FkColumn.Equals(fkColumnName));

            if (list.Count > 0)
                AddError("", Translate.Key("Relationship already registered"));
        }

        return IsValid;
    }


    public void Delete(string dictionaryName, int id)
    {
        var formElement = DataDictionaryRepository.GetMetadata(dictionaryName);
        var relationship = formElement.Relationships.GetById(id);

        formElement.Relationships.Remove(relationship);

        if (formElement.Relationships.All(r => r.IsParent))
        {
            formElement.Relationships.Clear();
        }
        
        DataDictionaryRepository.InsertOrReplace(formElement);
    }
    

    public void Sort(string dictionaryName, IEnumerable<string> relationships)
    {
        var formElement = DataDictionaryRepository.GetMetadata(dictionaryName);

        FormElementRelationship GetRelationship(string name)
        {
            if (name == "parent")
            {
                return formElement.Relationships.First(r => r.IsParent);
            }

            return formElement.Relationships.First(r => r.ElementRelationship?.ChildElement == name);
        }
        
        var newList = relationships.Select(GetRelationship).ToList();

        for (int i = 0; i < formElement.Relationships.Count; i++)
        {
            formElement.Relationships[i] = newList[i];
        }
        
        DataDictionaryRepository.InsertOrReplace(formElement);
    }

    public bool ValidatePanel(FormElementPanel panel)
    {
        return _panelService.ValidatePanel(panel);
    }
}