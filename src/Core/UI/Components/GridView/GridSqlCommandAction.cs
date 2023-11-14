using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Models;

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
            string msg = ExceptionManager.GetMessage(ex);
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

    private async Task ExecuteOnList(SqlCommandAction sqlCommandAction, List<IDictionary<string, object>> selectedRows)
    {
        var commandList = new List<DataAccessCommand>();
        foreach (var row in selectedRows)
        {
            var formData = new FormStateData(row, gridView.UserValues, PageState.List);
            var sql = gridView.ExpressionsService.ReplaceExpressionWithParsedValues(sqlCommandAction.CommandSql, formData);
            commandList.Add(new DataAccessCommand(sql!));
        }

        await gridView.EntityRepository.SetCommandListAsync(commandList);
    }

    private async Task ExecuteOnRecord(ActionMap map, SqlCommandAction sqlCommandAction)
    {
        var formElement = gridView.FormElement;
        IDictionary<string, object> formValues;
        if (map.PkFieldValues.Any())
        {
            formValues = await gridView.EntityRepository.GetFieldsAsync(formElement, map.PkFieldValues);
        }
        else
        {
            formValues = await gridView.FieldsService.GetDefaultValuesAsync(formElement, null, PageState.List);
        }

        var formStateData = new FormStateData(formValues, gridView.UserValues, PageState.List);
        var sqlCommand = gridView.ExpressionsService.ReplaceExpressionWithParsedValues(sqlCommandAction.CommandSql, formStateData);
        
        await gridView.EntityRepository.SetCommandAsync(new DataAccessCommand(sqlCommand!));
    }

}