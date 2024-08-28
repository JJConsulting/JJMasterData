using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Logging;

namespace JJMasterData.Core.UI.Components;

internal sealed class GridSqlCommandAction(JJGridView gridView)
{
    public async Task<ComponentResult> ExecuteSqlCommand(ActionMap map, SqlCommandAction sqlCommandAction)
    {
        var messageFactory = gridView.ComponentFactory.Html.MessageBox;
        try
        {
            if (IsApplyOnList(map, sqlCommandAction))
            {
                var selectedRows = gridView.GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = gridView.StringLocalizer["No lines selected."];
                    return new RenderedComponentResult(messageFactory.Create(msg, MessageIcon.Warning).GetHtmlBuilder());
                }

                await ExecuteOnList(sqlCommandAction, selectedRows);
                gridView.ClearSelectedGridValues();
            }
            else
            {
                await ExecuteOnRecord(map, sqlCommandAction);
            }
        }
        catch (Exception ex)
        {
            gridView.Logger.LogSqlActionException(ex, sqlCommandAction.SqlCommand);
            string msg = gridView.StringLocalizer[ExceptionManager.GetMessage(ex)];
            return new RenderedComponentResult(messageFactory.Create(msg, MessageIcon.Error).GetHtmlBuilder());
        }

        if (!string.IsNullOrEmpty(sqlCommandAction.RedirectUrl))
            return new RedirectComponentResult(sqlCommandAction.RedirectUrl);
        
        return EmptyComponentResult.Value;
    }

    private bool IsApplyOnList(ActionMap map, SqlCommandAction sqlCommandAction)
    {
        return map.ActionSource == ActionSource.GridToolbar
               && gridView.EnableMultiSelect
               && sqlCommandAction.ApplyOnSelected;
    }

    private async Task ExecuteOnList(SqlCommandAction sqlCommandAction, List<Dictionary<string, object>> selectedRows)
    {
        var commandList = new List<DataAccessCommand>();
        foreach (var row in selectedRows)
        {
            var formData = new FormStateData(row, gridView.UserValues, PageState.List);
            var sql = sqlCommandAction.SqlCommand;
            var parsedValues = gridView.ExpressionsService.ParseExpression(sql, formData);
            var command = ExpressionDataAccessCommandFactory.Create(sql, parsedValues);
            commandList.Add(command);
        }

        await gridView.EntityRepository.SetCommandListAsync(commandList, gridView.FormElement.ConnectionId);
    }

    private async Task ExecuteOnRecord(ActionMap map, SqlCommandAction sqlCommandAction)
    {
        var formElement = gridView.FormElement;
        Dictionary<string, object> formValues;
        if (map.PkFieldValues.Count > 0)
        {
            formValues = await gridView.EntityRepository.GetFieldsAsync(formElement, map.PkFieldValues);
        }
        else
        {
            formValues = await gridView.FieldValuesService.GetDefaultValuesAsync(formElement, new FormStateData(new Dictionary<string, object>(), PageState.List));
        }

        var formStateData = new FormStateData(formValues, gridView.UserValues, PageState.List);
        var sql = sqlCommandAction.SqlCommand;
        var parsedValues = gridView.ExpressionsService.ParseExpression(sql, formStateData);
        var sqlCommand = ExpressionDataAccessCommandFactory.Create(sql, parsedValues);
        
        await gridView.EntityRepository.SetCommandAsync(sqlCommand, gridView.FormElement.ConnectionId);
    }

}