using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.Html;
using JJMasterData.Core.Html.Templates;
using JJMasterData.Core.UI.Components;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;


namespace JJMasterData.Core.DataManager.Services;

public class HtmlTemplateService(
    HtmlTemplateHelper htmlTemplateHelper,
    IEntityRepository entityRepository,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILogger<HtmlTemplateService> logger)
{
    public async Task<HtmlBuilder> RenderTemplate(
        HtmlTemplateAction action,
        Guid? connectionId,
        Dictionary<string, object> pkValues)
    {
        string renderedTemplate;
        try
        {
            var command = ExpressionDataAccessCommandFactory.Create(action.SqlCommand, pkValues);
            var dataSource = await entityRepository.GetDataSetAsync(command, connectionId);

            renderedTemplate = await RenderTemplate(action.HtmlTemplate, new Dictionary<string, object>
            {
                { "DataSource", EnumerableHelper.ConvertDataSetToArray(dataSource) }
            });
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error rendering template {action}.", action.Name);

            renderedTemplate = "Error rendering template.";
        }
        var html = new HtmlBuilder();
        html.AppendDiv(div =>
        {
            div.WithCssClass("text-end").AppendComponent(new JJLinkButton
            {
                Icon = IconType.Print,
                ShowAsButton = true,
                Tooltip = stringLocalizer["Print"],
                OnClientClick = "printTemplateIframe()"
            });
        });
        html.Append(HtmlTag.Iframe, iframe =>
        {
            iframe.WithCssClass("modal-iframe");
            iframe.WithId("jjmasterdata-template-iframe");
            iframe.WithAttribute("srcdoc", renderedTemplate);
        });

        return html;
    }

    public ValueTask<string> RenderTemplate(string templateString, Dictionary<string, object> values)
    {
        return htmlTemplateHelper.RenderTemplate(templateString, values);
    }
}