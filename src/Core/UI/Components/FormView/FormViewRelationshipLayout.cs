#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Extensions;
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

    public async IAsyncEnumerable<HtmlBuilder?> GetRelationshipsHtml(JJDataPanel parentPanel, List<FormElementRelationship> relationships)
    {
        if (relationships.Any(r => r.Panel.Layout is PanelLayout.Tab))
        {
            var tabNav = await GetTabRelationshipsHtml(parentPanel, relationships);

            yield return tabNav;
        }

        foreach (var relationship in relationships.Where(r => r.Panel.Layout is not PanelLayout.Tab))
        {
            var relationshipHtml = await GetRelationshipHtml(parentPanel, relationship);
            var panel = GetNonTabRelationshipPanelHtml(relationship, relationshipHtml);
            yield return panel;
        }
    }

    private async Task<HtmlBuilder> GetTabRelationshipsHtml(JJDataPanel parentPanel, List<FormElementRelationship> relationships)
    {
        var tabNav = new JJTabNav(ParentFormView.CurrentContext)
        {
            Name = $"nav_relationships_{parentPanel.Name}"
        };

        foreach (var relationship in relationships.Where(r => r.Panel.Layout is PanelLayout.Tab))
        {
            var relationshipHtml = await GetRelationshipHtml(parentPanel, relationship);
            tabNav.ListTab.Add(new NavContent
            {
                Title = relationship.Panel.Title,
                HtmlContent = relationshipHtml
            });
        }

        return tabNav.GetHtmlBuilder();
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
                        ShowAsWell = relationship.Panel.Layout == PanelLayout.Well,
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

    private async Task<HtmlBuilder?> GetRelationshipHtml(JJDataPanel parentPanel, FormElementRelationship relationship)
    {
        var formContext = new FormContext(parentPanel.Values, parentPanel.Errors, parentPanel.PageState);
        if (relationship.IsParent)
        {
            return await ParentFormView.GetParentPanelHtml(parentPanel);
        }

        var childElement = ParentFormView.DataDictionaryRepository.GetMetadata(relationship.ElementRelationship!.ChildElement);

        var filter = new Dictionary<string, dynamic>();
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
                    var childValues = ParentFormView.EntityRepository.GetDictionaryAsync(childElement, filter).GetAwaiter().GetResult();

                    var childDataPanel = ParentFormView.ComponentFactory.DataPanel.Create(childElement);
                    childDataPanel.PageState = relationship.ViewType is RelationshipViewType.View ? PageState.View : PageState.Update;
                    childDataPanel.UserValues = ParentFormView.UserValues;
                    childDataPanel.Values = childValues;
                    childDataPanel.RenderPanelGroup = false;
                    childDataPanel.FormUI = childElement.Options.Form;

                    return childDataPanel.GetHtmlBuilder();
                }
            case RelationshipViewType.List:
            {
                    var childGrid = ParentFormView.ComponentFactory.FormView.Create(childElement);
                    childGrid.UserValues = ParentFormView.UserValues;
                    childGrid.IsExternalRoute = true;
                    childGrid.RelationValues = mappedForeignKeys;
                    childGrid.GridView.Filter.ApplyCurrentFilter(filter);
                    childGrid.SetOptions(childElement.Options);

                    childGrid.GridView.ShowTitle = false;

                    var htmlBuilder = childGrid.RenderHtml();
                    if (htmlBuilder != null)
                    {
                        var filters = ParentFormView.EncryptionService.EncryptStringWithUrlEncode(JsonConvert.SerializeObject(filter));
                        htmlBuilder.AppendHiddenInput($"jjgridview_{childElement.Name}_filters", filters);
                    }
                    
                    return htmlBuilder;
                }
            default:
                return null;
        }
    }

    private IDictionary<string, dynamic?> GetMappedForeignKeys(FormElement formElement, Dictionary<string, dynamic> filters)
    {
        var foreignKeys = new Dictionary<string, dynamic?>();
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