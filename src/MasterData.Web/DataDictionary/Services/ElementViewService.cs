using System.Globalization;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Util;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Structure;
using JJMasterData.Web.Components;
using JJMasterData.Web.DataDictionary.Structure;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.DataDictionary.Services;

public sealed class ElementViewService(
    IFormElementComponentFactory<JJFormView> formViewFactory,
    IDataDictionaryRepository dataDictionaryRepository,
    DataDictionaryFormElementFactory dataDictionaryFormElementFactory,
    RelativeDateFormatter relativeDateFormatter,
    IUrlHelper urlHelper)
{
    public JJFormView GetFormView()
    {
        var formView = formViewFactory.Create(dataDictionaryFormElementFactory.GetFormElement());

        formView.GridView.SetCurrentFilter(DataDictionaryStructure.Type, "F");

        if (!formView.GridView.CurrentOrder.Any())
            formView.GridView.CurrentOrder.AddOrReplace(DataDictionaryStructure.Name, OrderByDirection.Asc);

        formView.ShowTitle = false;
        formView.GridView.ShowTitle = false;
        formView.GridView.EnableMultiSelect = true;
        formView.GridView.FilterAction.ExpandedByDefault = true;

        formView.GridView.OnDataLoadAsync += async (_, args) =>
        {
            var filter = DataDictionaryFilter.FromDictionary(args.Filters!);
            var result = await dataDictionaryRepository.GetFormElementInfoListAsync(
                filter, args.OrderBy, args.RecordsPerPage, args.CurrentPage);

            args.DataSource = result.Data.ConvertAll(info => info.ToDictionary());
            args.TotalOfRecords = result.TotalOfRecords;
        };

        formView.GridView.OnRenderCellAsync += (_, args) =>
        {
            if (args.Field.Name == DataDictionaryStructure.Name)
            {
                var info = args.DataRow[DataDictionaryStructure.Info]?.ToString();
                if (!string.IsNullOrWhiteSpace(info))
                {
                    args.HtmlResult!.AppendSpan(span =>
                    {
                        span.WithCssClass("fa fa-question-circle help-description");
                        span.WithToolTip(info);
                    });
                }
            }

            if (args.Field.Name == DataDictionaryStructure.TableName)
            {
                args.HtmlResult = new HtmlBuilder(HtmlTag.Span)
                    .WithCssClass("font-monospace")
                    .AppendText(args.DataRow[DataDictionaryStructure.TableName]?.ToString());
            }

            if (args.Field.Name == DataDictionaryStructure.LastModified)
            {
                var lastModified = (DateTime)args.DataRow[DataDictionaryStructure.LastModified]!;
                args.HtmlResult = new HtmlBuilder(HtmlTag.Span)
                    .WithToolTip(lastModified.ToString(CultureInfo.CurrentCulture))
                    .AppendText(relativeDateFormatter.ToRelativeString(lastModified));
            }

            return ValueTask.CompletedTask;
        };

        formView.GridView.OnRenderActionAsync += (_, args) =>
        {
            var elementName = args.FieldValues["name"]?.ToString();
            switch (args.ActionName)
            {
                case "render":
                    args.LinkButton.OnClientClick =
                        $"window.open('{urlHelper.Action("Render", "Form", new { Area = "MasterData", elementName })}', '_blank').focus();";
                    break;
                case "tools":
                    args.LinkButton.UrlAction = urlHelper.Action("Index", "Entity",
                        new { Area = "DataDictionary", elementName });
                    args.LinkButton.OnClientClick = "";
                    break;
                case "duplicate":
                    args.LinkButton.UrlAction = urlHelper.Action("Duplicate", "Element",
                        new { Area = "DataDictionary", elementName });
                    args.LinkButton.OnClientClick = "";
                    break;
            }

            return ValueTask.CompletedTask;
        };

        return formView;
    }
}
