#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

internal class FormViewRelationshipLayout
{
    private JJFormView ParentFormView { get; }

    public FormViewRelationshipLayout(JJFormView parentFormView)
    {
        ParentFormView = parentFormView;
    }

    public async Task<ComponentResult> GetRelationshipsResult(List<FormElementRelationship> relationships)
    {
        var relationshipsDiv = new HtmlBuilder(HtmlTag.Div);

        if (relationships.Any(r => r.Panel.Layout is PanelLayout.Tab))
        {
            var tabNavResult = await GetTabRelationshipsResult(relationships);

            if (tabNavResult is RenderedComponentResult renderedComponentResult)
            {
                relationshipsDiv.Append(renderedComponentResult.HtmlBuilder);
            }
            else
            {
                return tabNavResult;
            }
        }

        foreach (var relationship in relationships.Where(r => r.Panel.Layout is not PanelLayout.Tab))
        {
            var relationshipResult = await GetRelationshipResult(relationship);

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


    private async Task<ComponentResult> GetTabRelationshipsResult(List<FormElementRelationship> relationships)
    {
        var tabNav = new JJTabNav(ParentFormView.CurrentContext.Request.Form)
        {
            Name = $"relationships-tab-nav-{ParentFormView.DataPanel.Name}"
        };

        foreach (var relationship in relationships.Where(r => r.Panel.Layout is PanelLayout.Tab))
        {
            var relationshipResult = await GetRelationshipResult(relationship);

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
                var collapse = new JJCollapsePanel(ParentFormView.CurrentContext.Request.Form)
                {
                    Name = $"{relationship.ElementRelationship?.ChildElement ?? ParentFormView.Name}-collapse-panel",
                    Title = relationship.Panel.Title,
                    HtmlBuilderContent = content,
                    Color = relationship.Panel.Color,
                    ExpandedByDefault = relationship.Panel.ExpandedByDefault,
                    CssClass = relationship.Panel.CssClass
                };

                return collapse.GetHtmlBuilder();
            case PanelLayout.Panel or PanelLayout.Well:
                var panel = new JJCard
                {
                    Name = $"{relationship.Id}-card",
                    Title = relationship.Panel.Title,
                    Color = relationship.Panel.Color,
                    Layout = relationship.Panel.Layout,
                    HtmlBuilderContent = content,
                    CssClass = relationship.Panel.CssClass
                };

                return panel.GetHtmlBuilder();
            case PanelLayout.NoDecoration:
                var htmlTitle = new JJTitle
                {
                    Title = relationship.Panel.Title,
                    Size = HeadingSize.H3
                };

                var div = new HtmlBuilder(HtmlTag.Div);
                div.WithCssClass(relationship.Panel.CssClass);
                div.AppendComponent(htmlTitle);
                div.Append(content);

                return div;
            default:
                return null;
        }
    }

    private async Task<ComponentResult> GetRelationshipResult(FormElementRelationship relationship)
    {
        var parentPanel = ParentFormView.DataPanel;

        var formContext = new FormContext(parentPanel.Values, parentPanel.Errors, parentPanel.PageState);
        
        if (relationship.IsParent)
        {
            return new RenderedComponentResult(await ParentFormView.GetRelationshipParentPanelHtml(parentPanel));
        }

        var childElement =
            await ParentFormView.DataDictionaryRepository.GetMetadataAsync(relationship.ElementRelationship!
                .ChildElement);
        childElement.ParentName = ParentFormView.FormElement.ParentName ?? ParentFormView.FormElement.Name;

        var filter = new Dictionary<string, object?>();
        foreach (var col in relationship.ElementRelationship.Columns.Where(col =>
                     formContext.Values.ContainsKey(col.PkColumn)))
        {
            var value = formContext.Values[col.PkColumn];
            filter[col.FkColumn] = value;
            ParentFormView.UserValues[col.FkColumn] = value;
        }

        var mappedForeignKeys = DataHelper.GetRelationValues(ParentFormView.FormElement, filter);
        switch (relationship.ViewType)
        {
            case RelationshipViewType.Update:
            case RelationshipViewType.View:
            {
                var childValues =
                    await ParentFormView.EntityRepository.GetFieldsAsync(childElement,
                        (IDictionary<string, object>)filter);
                var childDataPanel = ParentFormView.ComponentFactory.DataPanel.Create(childElement);
                childDataPanel.FieldNamePrefix = $"{childDataPanel.Name}_";
                childDataPanel.PageState = relationship.ViewType is RelationshipViewType.View ? PageState.View : PageState.Update;
                childDataPanel.UserValues = ParentFormView.UserValues;
                childDataPanel.Values = childValues;
                childDataPanel.RenderPanelGroup = false;
                childDataPanel.FormUI = childElement.Options.Form;

                return await childDataPanel.GetResultAsync();
            }
            case RelationshipViewType.List:
            {
                var childFormView = ParentFormView.ComponentFactory.FormView.Create(childElement);
                childFormView.DataPanel.FieldNamePrefix = $"{childFormView.DataPanel.Name}_";
                childFormView.UserValues = ParentFormView.UserValues;
                childFormView.SetRelationshipPageState(relationship.ViewType);
                childFormView.RelationValues = mappedForeignKeys;
                await childFormView.GridView.Filter.ApplyCurrentFilter(filter);
                childFormView.ShowTitle = false;

                if (ParentFormView.PageState is PageState.View)
                {
                    childFormView.DisableActionsAtViewMode();
                }
                
                var result = await childFormView.GetFormResultAsync();
                
                return result;
            }
            default:
                return new EmptyComponentResult();
        }
    }
}