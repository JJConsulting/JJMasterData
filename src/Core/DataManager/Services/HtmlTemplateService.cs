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
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Services;

public class HtmlTemplateService(
    IEntityRepository entityRepository,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    FluidParser fluidParser)
{
    public async Task<HtmlBuilder> RenderTemplate(HtmlTemplateAction action, Dictionary<string,object> pkValues)
    {
        var command = ExpressionDataAccessCommandFactory.Create(action.SqlCommand, pkValues);
        var dataSource = await entityRepository.GetDataSetAsync(command);

        string renderedTemplate;

        if (!fluidParser.TryParse(action.HtmlTemplate, out var template, out var error))
        {
            renderedTemplate = error;
        }
        else
        {
            var context = new TemplateContext(new { DataSource = EnumerableHelper.ConvertDataSetToArray(dataSource) });

            var localize = new FunctionValue((args, _) =>
            {
                var firstArg = args.At(0).ToStringValue();
                var localizedString = stringLocalizer[firstArg, args.Values.Select(v => v.ToStringValue())];
                return new ValueTask<FluidValue>(new StringValue(localizedString));
            });

            context.SetValue("localize", localize);

            renderedTemplate = await template.RenderAsync(context);
        }

        var html = new HtmlBuilder();
        html.AppendDiv(div =>
        {
            div.WithCssClass("text-end").AppendComponent(new JJLinkButton(stringLocalizer)
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
            iframe.WithAttribute("srcdoc", HttpUtility.HtmlAttributeEncode(renderedTemplate) ?? string.Empty);
        });

        return html;
    }
}