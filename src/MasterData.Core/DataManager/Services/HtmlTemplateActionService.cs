using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.FontAwesome;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Extensions;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.Html.Templates;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;


namespace JJMasterData.Core.DataManager.Services;

public class HtmlTemplateActionService(
    HtmlTemplateRenderer htmlTemplateRenderer,
    IEntityRepository entityRepository,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILogger<HtmlTemplateActionService> logger)
{
    public async Task<HtmlBuilder> RenderTemplate(
        HtmlTemplateAction action,
        Guid? connectionId,
        Dictionary<string, object> pkValues)
    {
        string renderedTemplate;
        try
        {
            var parsedValues = pkValues.ToDictionary(x => x.Key, x => (object?)x.Value, StringComparer.InvariantCultureIgnoreCase);
            var command = ExpressionDataAccessCommandFactory.Create(action.SqlCommand, parsedValues);
            var dataSource = await entityRepository.GetDataSetAsync(command, connectionId);

            var values = new Dictionary<string, object>
            {
                { "DataSource", EnumerableHelper.ConvertDataSetToArray(dataSource) }
            };
            renderedTemplate = await htmlTemplateRenderer.RenderTemplate(action.HtmlTemplate, values);
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
                Icon = FontAwesomeIcon.Print,
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
}
