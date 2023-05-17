using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

internal class FormElementRelationshipLayout
{
    private JJFormView ParentFormView { get; }

    public FormElementRelationshipLayout(JJFormView parentFormView)
    {
        ParentFormView = parentFormView;
    }
    
    public IEnumerable<HtmlBuilder> GetRelationshipsHtml(JJDataPanel parentPanel, List<FormElementRelationship> relationships)
    {
        if (relationships.Any(r => r.Panel.Layout is PanelLayout.Tab))
        {
            var tabNav = GetTabRelationshipsHtml(parentPanel, relationships);

            yield return tabNav;
        }

        foreach (var relationship in relationships.Where(r => r.Panel.Layout is not PanelLayout.Tab))
        {
            var relationshipHtml = GetRelationshipHtml(parentPanel, relationship);
            var panel = GetNonTabRelationshipPanelHtml(relationship, relationshipHtml);
            yield return panel;
        }
    }

    private HtmlBuilder GetTabRelationshipsHtml(JJDataPanel parentPanel, List<FormElementRelationship> relationships)
    {
        var tabNav = new JJTabNav
        {
            Name = $"nav_relationships_{parentPanel.Name}"
        };

        foreach (var relationship in relationships.Where(r => r.Panel.Layout is PanelLayout.Tab))
        {
            var relationshipHtml = GetRelationshipHtml(parentPanel, relationship);
            tabNav.ListTab.Add(new NavContent
            {
                Title = relationship.Panel.Title,
                HtmlContent = relationshipHtml
            });
        }

        return tabNav.GetHtmlBuilder();
    }

    private static HtmlBuilder GetNonTabRelationshipPanelHtml(FormElementRelationship relationship, HtmlBuilder html)
    {
        switch (relationship.Panel.Layout)
        {
            case PanelLayout.Collapse:
            {
                var collapse = new JJCollapsePanel
                {
                    Name = "collapse_" + relationship.Id,
                    Title = relationship.Panel.Title,
                    HtmlBuilderContent = html
                };

                return collapse.GetHtmlBuilder();
            }
            case PanelLayout.Panel or PanelLayout.Well:
            {
                var panel = new JJCard
                {
                    Name = "card_" + relationship.Id,
                    Title = relationship.Panel.Title,
                    ShowAsWell = relationship.Panel.Layout == PanelLayout.Well,
                    HtmlBuilderContent = html
                };

                return panel.GetHtmlBuilder();
            }
            case PanelLayout.NoDecoration:
                return html;
            default:
                return null;
        }
    }

    private HtmlBuilder GetRelationshipHtml(JJDataPanel parentPanel,
        FormElementRelationship relationship)
    {
        var formContext = new FormContext(parentPanel.Values, parentPanel.Errors, parentPanel.PageState);
        if (relationship.IsParent)
        {
            return ParentFormView.GetParentPanelHtml( parentPanel);
        }

        var childElement = ParentFormView.DataDictionaryRepository.GetMetadata(relationship.ElementRelationship!.ChildElement);

        var filter = new Dictionary<string,dynamic>();
        foreach (var col in relationship.ElementRelationship.Columns.Where(col => formContext.Values.Contains(col.PkColumn)))
        {
            filter.Add(col.FkColumn, formContext.Values[col.PkColumn]);
        }

        switch (relationship.ViewType)
        {
            case RelationshipViewType.View:
            {
                var childValues = ParentFormView.EntityRepository.GetFields(childElement, filter);
                var childDataPanel = new JJDataPanel(childElement)
                {
                    EntityRepository = ParentFormView.EntityRepository,
                    PageState = PageState.View,
                    UserValues = ParentFormView.UserValues,
                    Values = childValues,
                    RenderPanelGroup = false,
                    FormUI = childElement.Options.Form
                };

                return childDataPanel.GetHtmlBuilder();
            }
            case RelationshipViewType.List:
            {
                var childGrid = new JJFormView(childElement)
                {
                    EntityRepository = ParentFormView.EntityRepository,
                    UserValues = ParentFormView.UserValues,
                    FilterAction =
                    {
                        ShowAsCollapse = false
                    },
                    Name = "jjgridview_" + childElement.Name
                };
                childGrid.Filter.ApplyCurrentFilter(filter);

                childGrid.SetOptions(childElement.Options);

                childGrid.ShowTitle = false;
                return childGrid.GetHtmlBuilder();
            }
            default:
                return null;
        }
    }
    

}