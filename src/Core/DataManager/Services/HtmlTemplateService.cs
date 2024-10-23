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
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Services;

public class HtmlTemplateService(
    IEntityRepository entityRepository,
    IStringLocalizer<MasterDataResources> stringLocalizer,
    ILogger<HtmlTemplateService> logger,
    FluidParser fluidParser)
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
            iframe.WithAttribute("srcdoc", HttpUtility.HtmlAttributeEncode(renderedTemplate));
        });

        return html;
    }

    public async ValueTask<string> RenderTemplate(string templateString, Dictionary<string, object> values)
    {
        if (!fluidParser.TryParse(templateString, out var template, out var error))
        {
            return error;
        }

        var context = new TemplateContext(values);

        var localize = new FunctionValue((args, _) =>
        {
            var firstArg = args.At(0).ToStringValue();
            var localizerArgs = args.Values.Skip(1).Select(v => v.ToStringValue()).ToArray();

            var localizedString = stringLocalizer[firstArg, localizerArgs.ToArray()];

            return new ValueTask<FluidValue>(new StringValue(localizedString));
        });

        context.SetValue("isNullOrWhiteSpace", new FunctionValue(IsNullOrWhiteSpace));
        context.SetValue("isNullOrEmpty", new FunctionValue(IsNullOrEmpty));
        context.SetValue("substring", new FunctionValue(Substring));
        context.SetValue("localize", localize);

        return await template.RenderAsync(context);
    }

    private static BooleanValue IsNullOrEmpty(FunctionArguments args, TemplateContext _)
    {
        var str = args.At(0).ToStringValue();

        return BooleanValue.Create(string.IsNullOrEmpty(str));
    }
    
    private static BooleanValue IsNullOrWhiteSpace(FunctionArguments args, TemplateContext _)
    {
        var str = args.At(0).ToStringValue();

        return BooleanValue.Create(string.IsNullOrWhiteSpace(str));
    }
    
    private static StringValue Substring(FunctionArguments args, TemplateContext _)
    {
        if (args.Count < 2)
        {
            return new StringValue("Error: Not enough arguments");
        }

        var str = args.At(0).ToObjectValue().ToString()!;
        if (!int.TryParse(args.At(1).ToStringValue(), out var startIndex))
        {
            return new StringValue("Error: Invalid start index");
        }

        int length = 0;

        if (args.Count > 2 && !int.TryParse(args.At(2).ToStringValue(), out length))
        {
            return new StringValue("Error: Invalid length");
        }

        // If length is not provided, use the length of the remaining string from the start index
        var substring = args.Count > 2
            ? str.Substring(startIndex, length)
            : str[startIndex..];

        return new StringValue(substring);
    }
}