using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DI;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

internal class FormElementRelationshipLayout
{
    public JJDataPanel ParentPanel { get; }
    public List<FormElementRelationship> Relationships { get; }
    
    public FormElementRelationshipLayout(JJDataPanel parentPanel, List<FormElementRelationship> relationships)
    {
        ParentPanel = parentPanel;
        Relationships = relationships;
    }
    
    // private HtmlBuilder GetNonTabRelationshipPanel(FormElementRelationship relationship, HtmlBuilder html)
    // {
    //     switch (relationship.Panel.Layout)
    //     {
    //         case PanelLayout.Collapse:
    //         {
    //             var collapse = new JJCollapsePanel
    //             {
    //                 Name = "collapse_" + relationship.Id,
    //                 Title = relationship.Panel.Title,
    //                 HtmlBuilderContent = html
    //             };
    //
    //             return collapse.GetHtmlBuilder();
    //         }
    //         case PanelLayout.Panel or PanelLayout.Well:
    //         {
    //             var panel = new JJCard
    //             {
    //                 Name = "card_" + relationship.Id,
    //                 Title = relationship.Panel.Title,
    //                 ShowAsWell = relationship.Panel.Layout == PanelLayout.Well,
    //                 HtmlBuilderContent = html
    //             };
    //
    //             return panel.GetHtmlBuilder();
    //         }
    //         case PanelLayout.NoDecoration:
    //             return html;
    //         default:
    //             return null;
    //     }
    // }
    //
    // private HtmlBuilder GetRelationshipBaseView(FormContext formContext,
    //     FormElementRelationship relationship, JJDataPanel parentPanel)
    // {
    //     if (relationship.IsParent)
    //     {
    //         return parentPanel.GetParentPanelHtml(parentPanel);
    //     }
    //
    //     var childElement = JJServiceCore.DataDictionaryRepository.GetMetadata(relationship.ElementRelationship!.ChildElement);
    //
    //     var filter = new Dictionary<string,dynamic>();
    //     foreach (var col in relationship.ElementRelationship.Columns.Where(col => formContext.Values.Contains(col.PkColumn)))
    //     {
    //         filter.Add(col.FkColumn, formContext.Values[col.PkColumn]);
    //     }
    //
    //     switch (relationship.ViewType)
    //     {
    //         case RelationshipViewType.View:
    //         {
    //             var childValues = EntityRepository.GetFields(childElement, filter);
    //             var childDataPanel = new JJDataPanel(childElement)
    //             {
    //                 EntityRepository = EntityRepository,
    //                 PageState = PageState.View,
    //                 UserValues = parentPanel.UserValues,
    //                 Values = childValues,
    //                 RenderPanelGroup = false,
    //                 FormUI = childElement.Options.Form
    //             };
    //
    //             return childDataPanel.GetHtmlBuilder();
    //         }
    //         case RelationshipViewType.List:
    //         {
    //             var childGrid = new JJFormView(childElement)
    //             {
    //                 EntityRepository = EntityRepository,
    //                 UserValues = parentPanel.UserValues,
    //                 FilterAction =
    //                 {
    //                     ShowAsCollapse = false
    //                 },
    //                 Name = "jjgridview_" + childElement.Name
    //             };
    //             childGrid.Filter.ApplyCurrentFilter(filter);
    //
    //             childGrid.SetOptions(childElement.Options);
    //
    //             childGrid.ShowTitle = false;
    //             return childGrid.GetHtmlBuilder();
    //         }
    //         default:
    //             return null;
    //     }
    // }

}