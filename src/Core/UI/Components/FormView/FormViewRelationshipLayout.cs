#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;
using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

internal class FormViewRelationshipLayout
{
    private JJFormView ParentFormView { get; }

    public FormViewRelationshipLayout(JJFormView parentFormView)
    {
        ParentFormView = parentFormView;
    }

    public async Task<ComponentResult> GetRelationshipsResult(JJDataPanel parentPanel, List<FormElementRelationship> relationships)
    {
        var visibleRelationships = await GetVisibleRelationships(relationships).ToListAsync();

        var relationshipsDiv = new HtmlBuilder(HtmlTag.Div);
        
        if (visibleRelationships.Any(r => r.Panel.Layout is PanelLayout.Tab))
        {
            var tabNavResult = await GetTabRelationshipsResult(parentPanel, relationships);

            if (tabNavResult is RenderedComponentResult renderedComponentResult)
            {
                relationshipsDiv.Append(renderedComponentResult.HtmlBuilder);
            }
            else
            {
                return tabNavResult;
            }
        }

        foreach (var relationship in visibleRelationships.Where(r => r.Panel.Layout is not PanelLayout.Tab))
        {
            var relationshipResult = await GetRelationshipResult(parentPanel, relationship);

            if (relationshipResult is RenderedComponentResult renderedComponentResult)
            {
                var panel = GetNonTabRelationshipPanelHtml(relationship, renderedComponentResult.HtmlBuilder);
                relationshipsDiv.Append(panel);
            }
            else
            {
                return relationshipResult;
            }
          
        }

        return new RenderedComponentResult(relationshipsDiv);
    }
    
    
    private async IAsyncEnumerable<FormElementRelationship> GetVisibleRelationships(IEnumerable<FormElementRelationship> relationships)
    {
        var formStateData = await ParentFormView.GetFormStateDataAsync();
        foreach (var relationship in relationships)
        {
            var isVisible = await ParentFormView.ExpressionsService.GetBoolValueAsync(relationship.Panel.VisibleExpression, formStateData);

            if (isVisible)
                yield return relationship;
        }
    }

    private async Task<ComponentResult> GetTabRelationshipsResult(JJDataPanel parentPanel, List<FormElementRelationship> relationships)
    {
        var tabNav = new JJTabNav(ParentFormView.CurrentContext)
        {
            Name = $"nav_relationships_{parentPanel.Name}"
        };

        foreach (var relationship in relationships.Where(r => r.Panel.Layout is PanelLayout.Tab))
        {
            var relationshipResult = await GetRelationshipResult(parentPanel, relationship);

            if (relationshipResult is RenderedComponentResult renderedComponentResult)
            {
                tabNav.ListTab.Add(new NavContent
                {
                    Title = relationship.Panel.Title,
                    HtmlContent = renderedComponentResult.HtmlBuilder
                });
            }
            else
            {
                return relationshipResult;
            }
        }

        return new RenderedComponentResult(tabNav.GetHtmlBuilder());
    }

    private HtmlBuilder? GetNonTabRelationshipPanelHtml(FormElementRelationship relationship, HtmlBuilder? content)
    {
        switch (relationship.Panel.Layout)
        {
            case PanelLayout.Collapse or PanelLayout.Panel:
                {
                    var collapse = new JJCollapsePanel(ParentFormView.CurrentContext)
                    {
                        Name = "collapse_" + relationship.Id,
                        Title = relationship.Panel.Title,
                        HtmlBuilderContent = content,
                        Color = relationship.Panel.Color,
                        ExpandedByDefault = relationship.Panel.ExpandedByDefault,
                        CssClass = relationship.Panel.CssClass
                    };

                    return collapse.GetHtmlBuilder();
                }
            case PanelLayout.Panel or PanelLayout.Well:
                {
                    var panel = new JJCard
                    {
                        Name = "card_" + relationship.Id,
                        Title = relationship.Panel.Title,
                        Color = relationship.Panel.Color,
                        Layout = relationship.Panel.Layout,
                        HtmlBuilderContent = content,
                        CssClass = relationship.Panel.CssClass
                    };

                    return panel.GetHtmlBuilder();
                }
            case PanelLayout.NoDecoration:
                var div = new HtmlBuilder(HtmlTag.Div);
                div.WithCssClass(relationship.Panel.CssClass);
                div.AppendComponent(new JJTitle(relationship.Panel.Title, string.Empty)
                {
                    Size = HeadingSize.H3
                });
                div.Append(content);
                return div;
            default:
                return null;
        }
    }

    private async Task<ComponentResult> GetRelationshipResult(JJDataPanel parentPanel, FormElementRelationship relationship)
    {
        var formContext = new FormContext(parentPanel.Values, parentPanel.Errors, parentPanel.PageState);
        if (relationship.IsParent)
        {
            return new RenderedComponentResult(await ParentFormView.GetHtmlFromPanel(parentPanel));
        }

        var childElement = await ParentFormView.DataDictionaryRepository.GetMetadataAsync(relationship.ElementRelationship!.ChildElement);

        var filter = new Dictionary<string, object?>();
        foreach (var col in relationship.ElementRelationship.Columns.Where(col => formContext.Values.ContainsKey(col.PkColumn)))
        {
            var value = formContext.Values[col.PkColumn];
            filter[col.FkColumn] = value;
            ParentFormView.UserValues[col.FkColumn] = value;
        }

        var mappedForeignKeys = GetMappedForeignKeys(ParentFormView.FormElement,filter);
        switch (relationship.ViewType)
        {
            case RelationshipViewType.View or RelationshipViewType.Update:
                {
                    var childValues = await ParentFormView.EntityRepository.GetFieldsAsync((Element)childElement, (IDictionary<string, object>)filter);

                    var childDataPanel = ParentFormView.ComponentFactory.DataPanel.Create(childElement);
                    childDataPanel.FieldNamePrefix = childElement.Name + "_";
                    childDataPanel.PageState = relationship.ViewType is RelationshipViewType.View ? PageState.View : PageState.Update;
                    childDataPanel.UserValues = ParentFormView.UserValues;
                    childDataPanel.Values = childValues;
                    childDataPanel.IsExternalRoute = true;
                    childDataPanel.RenderPanelGroup = false;
                    childDataPanel.FormUI = childElement.Options.Form;

                    return await childDataPanel.GetResultAsync();
                }
            case RelationshipViewType.List:
            {
                    var childFormView = ParentFormView.ComponentFactory.FormView.Create(childElement);
                    childFormView.DataPanel.FieldNamePrefix = childElement.Name + "_";
                    childFormView.UserValues = ParentFormView.UserValues;
                    childFormView.IsExternalRoute = true;
                    childFormView.RelationValues = mappedForeignKeys;
                    await childFormView.GridView.Filter.ApplyCurrentFilter(filter);
                    childFormView.SetOptions(childElement.Options);

                    childFormView.GridView.ShowTitle = false;

                    var result = await childFormView.GetResultAsync();

                    if (result is RenderedComponentResult renderedComponentResult)
                    {
                 
                        var filters = ParentFormView.EncryptionService.EncryptStringWithUrlEscape(JsonConvert.SerializeObject(filter));
                        renderedComponentResult.HtmlBuilder.AppendHiddenInput($"jjgridview-{childElement.Name}_filters", filters);
                    
                        return renderedComponentResult;
                    }

                    return result;
            }
            default:
                return new EmptyComponentResult();
        }
    }

    private static IDictionary<string, object?> GetMappedForeignKeys(FormElement formElement, Dictionary<string, object?> filters)
    {
        var foreignKeys = new Dictionary<string, object?>();
        var relationships = formElement.Relationships.GetElementRelationships();

        foreach (var entry in filters)
        {
            var matchingRelationship = relationships.FirstOrDefault(r => r.Columns.Any(c => c.FkColumn == entry.Key));

            if (matchingRelationship != null)
            {
                var matchingColumn = matchingRelationship.Columns.First(c => c.FkColumn == entry.Key);
                foreignKeys[matchingColumn.FkColumn] = entry.Value;
            }
        }

        return foreignKeys;
    }
}