using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class RelationshipsService : BaseService
{
    private readonly PanelService _panelService;

    public RelationshipsService(
        IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        PanelService panelService)
        : base(validationDictionary, dataDictionaryRepository,stringLocalizer)
    {
        _panelService = panelService;
    }

    public async Task SaveElementRelationship(ElementRelationship elementRelationship, int? id, string dictionaryName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        var relationships = formElement.Relationships;

        if (!relationships.Any(r => r.IsParent))
        {
            var relation = new FormElementRelationship(true);
            relation.Panel.Title = formElement.Title;
            relationships.Add(relation);
        }
        
        if (id == null)
        {
            relationships.Add(new FormElementRelationship(elementRelationship));
        }
        else
        {
            var relation = relationships.GetById(id.Value);
            relation.ElementRelationship = elementRelationship;
        }
        
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }

    public async Task SaveFormElementRelationship(FormElementPanel panel, RelationshipViewType viewType, int id,
        string dictionaryName)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);

        var relationship = formElement.Relationships.First(r=>r.Id == id);
        relationship.ViewType = viewType;
        relationship.Panel = panel;

        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }

    public async Task<bool> ValidateRelation(string dictionaryName, string childElementName, string pkColumnName,
        string fkColumnName)
    {
        if (string.IsNullOrEmpty(childElementName))
        {
            AddError("ChildElement", StringLocalizer["Required ChildElement Field"]);
            return IsValid;
        }

        if (!(await DataDictionaryRepository.ExistsAsync(childElementName)))
        {
            AddError("Entity", StringLocalizer["Entity {0} not found", childElementName]);
            return IsValid;
        }

        if (string.IsNullOrEmpty(pkColumnName))
        {
            AddError("PkColumn", StringLocalizer["Required PkColumn field"]);
            return IsValid;
        }

        if (string.IsNullOrEmpty(fkColumnName))
        {
            AddError("FkColumn", StringLocalizer["Required FkColumn field"]);
            return IsValid;
        }

        var fkColumn = await GetFieldAsync(childElementName, fkColumnName);
        if (fkColumn == null)
        {
            AddError("", StringLocalizer["Column {0} not found in {1}.", fkColumnName, childElementName]);
            return IsValid;
        }

        var pkColumn = await GetFieldAsync(dictionaryName, pkColumnName);
        if (pkColumn == null)
        {
            AddError("", StringLocalizer["Column {0} not found.", pkColumnName]);
            return IsValid;
        }

        if (fkColumn.Filter.Type == FilterMode.None && !fkColumn.IsPk)
        {
            AddError("", StringLocalizer["Column {0} has no filter or is not a primary key.", fkColumnName]);
            return IsValid;
        }

        if (pkColumn.DataType != fkColumn.DataType)
        {
            AddError("",
                StringLocalizer["Column {0} has incompatible types with column {1}", pkColumnName, fkColumnName]);
            return IsValid;
        }

        return IsValid;
    }

    private async Task<ElementField?> GetFieldAsync(string dictionaryName, string fieldName)
    {
        var formElement = await _panelService.GetFormElementAsync(dictionaryName);
        return formElement.Fields.Contains(fieldName) ? formElement.Fields[fieldName] : null;
    }


    public async Task<bool> ValidateElementRelationship(ElementRelationship elementRelationship, string dictionaryName,
        int? index)
    {
        if (string.IsNullOrWhiteSpace(elementRelationship.ChildElement))
            AddError("", StringLocalizer["Mandatory <b>PKTable </b> field"]);

        if (IsValid)
        {
            if (elementRelationship.Columns.Count == 0)
                AddError("", StringLocalizer["No relationship registered."]);
        }

        if (IsValid)
        {
            foreach (var r in elementRelationship.Columns)
            {
                await ValidateRelation(dictionaryName, elementRelationship.ChildElement, r.PkColumn, r.FkColumn);
            }
        }

        if (IsValid && index == null)
        {
            var element = await GetFormElementAsync(dictionaryName);
            var relationships = element.Relationships
                .GetElementRelationships().FindAll(x => x.ChildElement.Equals(elementRelationship.ChildElement));
            if (relationships.Count > 0)
                AddError("",
                    StringLocalizer["There is already a relationship registered for "] +
                    elementRelationship.ChildElement);
        }

        return IsValid;
    }

    public async Task<bool> ValidateFinallyAddRelation(string dictionaryName, ElementRelationship elementRelationship,
        string pkColumnName, string fkColumnName)
    {
        if (await ValidateRelation(dictionaryName, elementRelationship.ChildElement, pkColumnName, fkColumnName))
        {
            var list = elementRelationship.Columns.FindAll(x =>
                x.PkColumn.Equals(pkColumnName) &&
                x.FkColumn.Equals(fkColumnName));

            if (list.Count > 0)
                AddError("", StringLocalizer["Relationship already registered"]);
        }

        return IsValid;
    }


    public async Task DeleteAsync(string dictionaryName, int id)
    {
        var formElement =  await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        var relationship = formElement.Relationships.GetById(id);

        formElement.Relationships.Remove(relationship);
        if (formElement.Relationships.All(r => r.IsParent))
        {
            formElement.Relationships.Clear();
        }
        
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }
    

    public async Task SortAsync(string dictionaryName, IEnumerable<string> relationships)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);

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
        
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }

    public bool ValidatePanel(FormElementPanel panel)
    {
        return _panelService.ValidatePanel(panel);
    }
}