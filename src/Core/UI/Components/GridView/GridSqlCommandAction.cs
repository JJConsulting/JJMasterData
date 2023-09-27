using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.UI.Components.GridView;

internal class GridSqlCommandAction
{
    private readonly JJGridView _gridView;

    public GridSqlCommandAction(JJGridView gridView)
    {
        _gridView = gridView;
    }

    public async Task<JJMessageBox> ExecuteSqlCommand(ActionMap map, SqlCommandAction sqlCommandAction)
    {
        var messageFactory = _gridView.ComponentFactory.Html.MessageBox;
        try
        {
            if (IsApplyOnList(map, sqlCommandAction))
            {
                var selectedRows = _gridView.GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = _gridView.StringLocalizer["No lines selected."];
                    return  messageFactory.Create(msg, MessageIcon.Warning);
                }

                await ExecuteOnList(sqlCommandAction, selectedRows);
                _gridView.ClearSelectedGridValues();
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
               && _gridView.EnableMultiSelect
               && sqlCommandAction.ApplyOnSelected;
    }

    private async Task ExecuteOnList(SqlCommandAction sqlCommandAction, List<IDictionary<string, object>> selectedRows)
    {
        var commandList = new List<DataAccessCommand>();
        foreach (var row in selectedRows)
        {
            var formData = new FormStateData(row, _gridView.UserValues, PageState.List);
            var sql = _gridView.ExpressionsService.ParseExpression(sqlCommandAction.CommandSql, formData, false);
            commandList.Add(new DataAccessCommand(sql!));
        }

        await _gridView.EntityRepository.SetCommandListAsync(commandList);
    }

    private async Task ExecuteOnRecord(ActionMap map, SqlCommandAction sqlCommandAction)
    {
        var formElement = _gridView.FormElement;
        IDictionary<string, object> formValues;
        if (map.PkFieldValues.Any())
        {
            formValues = await _gridView.EntityRepository.GetFieldsAsync(formElement, map.PkFieldValues);
        }
        else
        {
            formValues = await _gridView.FieldsService.GetDefaultValuesAsync(formElement, null, PageState.List);
        }

        var formStateData = new FormStateData(formValues, _gridView.UserValues, PageState.List);
        var sqlCommand = _gridView.ExpressionsService.ParseExpression(sqlCommandAction.CommandSql, formStateData);
        
        await _gridView.EntityRepository.SetCommandAsync(new DataAccessCommand(sqlCommand!));
    }

}