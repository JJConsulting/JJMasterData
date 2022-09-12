using System.Collections.Generic;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class RelationsService : BaseService
{
    public RelationsService(IValidationDictionary validationDictionary) : base(validationDictionary)
    {
    }

    public void Save(ElementRelation elementRelation, string sIndex, string dictionaryName)
    {
        FormElement formElement = DicDao.GetFormElement(dictionaryName);

        if (!string.IsNullOrEmpty(sIndex))
        {
            int index = int.Parse(sIndex);
            formElement.Relations[index] = elementRelation;
        }
        else
        {
            formElement.Relations.Add(elementRelation);
        }

        DicDao.SetFormElement(formElement);
    }

    public bool ValidateRelation(string childElement, string pkColumnName, string fkColumnName)
    {
        if (string.IsNullOrEmpty(childElement))
        {
            AddError("PkTableName", Translate.Key("Required PkTableName Field"));
            return IsValid;
        }

        FormElement formElement = DicDao.GetFormElement(childElement);
        Element childTable = DicDao.Factory.GetElement(childElement);
        if (childTable == null)
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

        if (!childTable.Fields.ContainsKey(fkColumnName))
        {
            AddError("", Translate.Key("Column {0} not found in {1}.", fkColumnName, childElement));
            return IsValid;
        }

        if (!formElement.Fields.Contains(pkColumnName))
        {
            AddError("", Translate.Key("Column {0} not found.", pkColumnName));
            return IsValid;
        }

        ElementField fkColumn = childTable.Fields[fkColumnName];
        ElementField pkColumn = formElement.Fields[pkColumnName];

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

}