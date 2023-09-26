#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Services;

public class ActionExecutionService
{
    private IEntityRepository EntityRepository { get; }
    private IEnumerable<IActionPlugin> ActionPlugins { get; }
    private ExpressionsService ExpressionsService { get; }

    public ActionExecutionService(
        IEntityRepository entityRepository,
        IEnumerable<IActionPlugin> actionPlugins,
        ExpressionsService expressionsService)
    {
        EntityRepository = entityRepository;
        ActionPlugins = actionPlugins;
        ExpressionsService = expressionsService;
    }
    public UrlRedirectModel GetUrlRedirect(UrlRedirectAction urlRedirectAction, FormStateData formStateData)
    {
        var parsedUrl = ExpressionsService.ParseExpression(urlRedirectAction.UrlRedirect, formStateData);

        var model = new UrlRedirectModel
        {
            IsIframe = urlRedirectAction.IsIframe,
            UrlRedirect = parsedUrl!,
            ModalTitle = urlRedirectAction.ModalTitle,
            UrlAsModal = urlRedirectAction.IsModal
        };
        
        return model;
    }

    public async Task ExecuteSqlCommandAction(SqlCommandAction sqlCommandAction, FormStateData formStateData)
    {
        var sqlCommand = ExpressionsService.ParseExpression(sqlCommandAction.CommandSql, formStateData);
        
        await EntityRepository.SetCommandAsync(new DataAccessCommand(sqlCommand!));
    }
    
    public async Task<PluginActionResult> ExecutePluginAction(PluginAction pluginAction, FormStateData formStateData)
    {
        var actionPlugin = ActionPlugins.First(p => p.Id == pluginAction.PluginId);

        var values = formStateData.FormValues;

        DataHelper.CopyIntoDictionary(values, formStateData.UserValues, false);
        
        return await actionPlugin.ExecuteActionAsync(values);
    }
}