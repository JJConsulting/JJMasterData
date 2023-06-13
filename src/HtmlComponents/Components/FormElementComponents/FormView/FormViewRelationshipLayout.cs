using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

internal class FormViewRelationshipLayout
{
    private JJFormView ParentFormView { get; }

    public FormViewRelationshipLayout(JJFormView parentFormView)
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

    private HtmlBuilder GetNonTabRelationshipPanelHtml(FormElementRelationship relationship, HtmlBuilder content)
    {
        switch (relationship.Panel.Layout)
        {
            case PanelLayout.Collapse or PanelLayout.Panel:
            {
                var collapse = new JJCollapsePanel
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
                div.AppendElement(new JJTitle(relationship.Panel.Title, string.Empty)
                {
                    Size = HeadingSize.H3
                });
                div.AppendElement(content);
                return div;
            default:
                return null;
        }
    }

    private HtmlBuilder GetRelationshipHtml(JJDataPanel parentPanel, FormElementRelationship relationship)
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
            var value = formContext.Values[col.PkColumn];
            filter[col.FkColumn] = value;
            ParentFormView.UserValues[col.FkColumn] = value;
        }

        switch (relationship.ViewType)
        {
            case RelationshipViewType.View or RelationshipViewType.Update:
            {
                var childValues = ParentFormView.EntityRepository.GetFields(childElement, filter);
                var childDataPanel = new JJDataPanel(childElement)
                {
                    EntityRepository = ParentFormView.EntityRepository,
                    PageState = relationship.ViewType is RelationshipViewType.View ? PageState.View : PageState.Update,
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