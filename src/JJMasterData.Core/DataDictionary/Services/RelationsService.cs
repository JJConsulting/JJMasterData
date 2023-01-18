using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class RelationsService : BaseService
{
    public RelationsService(IValidationDictionary validationDictionary, IDataDictionaryRepository dataDictionaryRepository)
        : base(validationDictionary, dataDictionaryRepository)
    {
    }

    public void Save(ElementRelation elementRelation, string sIndex, string dictionaryName)
    {
        var dictionary = DataDictionaryRepository.GetMetadata(dictionaryName);
        var relations = dictionary.Table.Relations;

        if (!string.IsNullOrEmpty(sIndex))
        {
            int index = int.Parse(sIndex);
            relations[index] = elementRelation;
        }
        else
        {
            relations.Add(elementRelation);
        }

        DataDictionaryRepository.InsertOrReplace(dictionary);
    }

    public bool ValidateRelation(string childElement, string pkColumnName, string fkColumnName)
    {
        if (string.IsNullOrEmpty(childElement))
        {
            AddError("PkTableName", Translate.Key("Required PkTableName Field"));
            return IsValid;
        }
        
        if (!DataDictionaryRepository.Exists(childElement))
        {
            AddError("Entity", Translate.Key("Entity {0} not found", childElement));
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

        var dictionary = DataDictionaryRepository.GetMetadata(childElement);
        var element = dictionary.Table;
        if (!element.Fields.ContainsKey(fkColumnName))
        {
            AddError("", Translate.Key("Column {0} not found in {1}.", fkColumnName, childElement));
            return IsValid;
        }

        if (!element.Fields.ContainsKey(pkColumnName))
        {
            AddError("", Translate.Key("Column {0} not found.", pkColumnName));
            return IsValid;
        }

        ElementField fkColumn = element.Fields[fkColumnName];
        ElementField pkColumn = element.Fields[pkColumnName];

        if (fkColumn.Filter.Type == FilterMode.None && !fkColumn.IsPk)
        {
            AddError("", Translate.Key("Column {0} has no filter or is not a primary key.", fkColumnName));
            return IsValid;
        }

        if (pkColumn.DataType != fkColumn.DataType)
        {
            AddError("", Translate.Key("Column {0} has incompatible types with column {1}", pkColumnName, fkColumnName));
            return IsValid;
        }

        return IsValid;
    }

    public bool ValidateFields(ElementRelation elementRelation, string dictionaryName, string sIndex)
    {
        if (string.IsNullOrWhiteSpace(elementRelation.ChildElement))
            AddError("", Translate.Key("Mandatory <b>PKTable </b> field"));

        if (IsValid)
        {
            if (elementRelation.Columns.Count == 0)
                AddError("", Translate.Key("No relationship registered."));
        }

        if (IsValid)
        {
            foreach (var r in elementRelation.Columns)
            {
                ValidateRelation(elementRelation.ChildElement, r.PkColumn, r.FkColumn);
            }
        }

        if (IsValid && string.IsNullOrEmpty(sIndex))
        {
            List<ElementRelation> listRelation = GetFormElement(dictionaryName).Relations.FindAll(x => x.ChildElement.Equals(elementRelation.ChildElement));
            if (listRelation.Count > 0)
                AddError("", Translate.Key("There is already a relationship registered for ") + elementRelation.ChildElement);
        }

        return IsValid;
    }

    public bool ValidateFinallyAddRelation(ElementRelation elementRelation, string pkColumnName, string fkColumnName)
    {
        if (ValidateRelation(elementRelation.ChildElement, pkColumnName, fkColumnName))
        {
            var list = elementRelation.Columns.FindAll(x =>
                x.PkColumn.Equals(pkColumnName) &&
                x.FkColumn.Equals(fkColumnName));

            if (list.Count > 0)
                AddError("", Translate.Key("Relationship already registered"));

        }

        return IsValid;
    }


    public void Delete(string dictionaryName, string index)
    {
        var dictionary = DataDictionaryRepository.GetMetadata(dictionaryName);
        ElementRelation elementRelation = dictionary.Table.Relations[int.Parse(index)];
        dictionary.Table.Relations.Remove(elementRelation);
        DataDictionaryRepository.InsertOrReplace(dictionary);
    }


    public void MoveDown(string dictionaryName, string index)
    {
        var dictionary = DataDictionaryRepository.GetMetadata(dictionaryName);
        var relations = dictionary.Table.Relations;
        int indexToMoveDown = int.Parse(index);
        if (indexToMoveDown >= 0 && indexToMoveDown < relations.Count - 1)
        {
            ElementRelation elementRelation = relations[indexToMoveDown + 1];
            relations[indexToMoveDown + 1] = relations[indexToMoveDown];
            relations[indexToMoveDown] = elementRelation;

            DataDictionaryRepository.InsertOrReplace(dictionary);
        }
    }


    public void MoveUp(string dictionaryName, string index)
    {
        var dictionary = DataDictionaryRepository.GetMetadata(dictionaryName);
        var relations = dictionary.Table.Relations;
        int indexToMoveUp = int.Parse(index);
        if (indexToMoveUp > 0)
        {
            ElementRelation elementRelation = relations[indexToMoveUp - 1];
            relations[indexToMoveUp - 1] = relations[indexToMoveUp];
            relations[indexToMoveUp] = elementRelation;
            DataDictionaryRepository.InsertOrReplace(dictionary);
        }
    }

}