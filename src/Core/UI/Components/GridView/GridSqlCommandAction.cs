using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Logging;

namespace JJMasterData.Core.UI.Components;

internal class GridSqlCommandAction(JJGridView gridView)
{
    public async Task<JJMessageBox> ExecuteSqlCommand(ActionMap map, SqlCommandAction sqlCommandAction)
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
                    return  messageFactory.Create(msg, MessageIcon.Warning);
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
            return messageFactory.Create(msg, MessageIcon.Error);
        }

        return null;
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
            var command = SqlExpressionProvider.GetParsedDataAccessCommand(sql, parsedValues);
            commandList.Add(command);
        }

        await gridView.EntityRepository.SetCommandListAsync(commandList, gridView.FormElement.ConnectionId);
    }

    private async Task ExecuteOnRecord(ActionMap map, SqlCommandAction sqlCommandAction)
    {
        var formElement = gridView.FormElement;
        Dictionary<string, object> formValues;
        if (map.PkFieldValues.Any())
        {
            formValues = await gridView.EntityRepository.GetFieldsAsync(formElement, map.PkFieldValues);
        }
        else
        {
            formValues = await gridView.FieldsService.GetDefaultValuesAsync(formElement, new FormStateData(new Dictionary<string, object>(), PageState.List));
        }

        var formStateData = new FormStateData(formValues, gridView.UserValues, PageState.List);
        var sql = sqlCommandAction.SqlCommand;
        var parsedValues = gridView.ExpressionsService.ParseExpression(sql, formStateData);
        var sqlCommand = SqlExpressionProvider.GetParsedDataAccessCommand(sql, parsedValues);
        
        await gridView.EntityRepository.SetCommandAsync(sqlCommand, gridView.FormElement.ConnectionId);
    }

}