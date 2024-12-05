using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Fluid;
using Fluid.Values;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.Html;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using static JJMasterData.Core.Html.HtmlTemplateHelper;

namespace JJMasterData.Core.DataManager.Services;

public class HtmlTemplateService(
    HtmlTemplateRenderer<MasterDataResources> htmlTemplateRenderer,
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
            iframe.WithAttribute("srcdoc", HttpUtility.HtmlAttributeEncode(renderedTemplate));
        });

        return html;
    }

    public ValueTask<string> RenderTemplate(string templateString, Dictionary<string, object> values)
    {
        return htmlTemplateRenderer.RenderTemplate(templateString, values);
    }
}