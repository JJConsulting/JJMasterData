using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;

namespace JJMasterData.Core.UI.Components.GridView;

internal class GridSqlCommandAction
{
    private readonly JJGridView _gridView;

    public GridSqlCommandAction(JJGridView gridView)
    {
        _gridView = gridView;
    }

    public async Task<JJMessageBox> ExecuteSqlCommand(ActionMap map, SqlCommandAction cmdAction)
    {
        var messageFactory = _gridView.ComponentFactory.Html.MessageBox;
        try
        {
            if (IsApplyOnList(map, cmdAction))
            {
                var selectedRows = _gridView.GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = _gridView.StringLocalizer["No lines selected."];
                    return  messageFactory.Create(msg, MessageIcon.Warning);
                }

                await ExecuteOnList(cmdAction, selectedRows);
                _gridView.ClearSelectedGridValues();
            }
            else
            {
                await ExecuteOnRecord(map, cmdAction);
            }
        }
        catch (Exception ex)
        {
            string msg = ExceptionManager.GetMessage(ex);
            return messageFactory.Create(msg, MessageIcon.Error);
        }

        return null;
    }

    private bool IsApplyOnList(ActionMap map, SqlCommandAction cmdAction)
    {
        return map.ActionSource == ActionSource.GridToolbar
               && _gridView.EnableMultiSelect
               && cmdAction.ApplyOnSelected;
    }

    private async Task ExecuteOnList(SqlCommandAction cmdAction, List<IDictionary<string, object>> selectedRows)
    {
        var commandList = new List<DataAccessCommand>();
        foreach (var row in selectedRows)
        {
            var formData = new FormStateData(row, _gridView.UserValues, PageState.List);
            string sql = _gridView.ExpressionsService.ParseExpression(cmdAction.CommandSql, formData, false);
            commandList.Add(new DataAccessCommand(sql!));
        }

        await _gridView.EntityRepository.SetCommandListAsync(commandList);
    }

    private async Task ExecuteOnRecord(ActionMap map, SqlCommandAction cmdAction)
    {
        var formElement = _gridView.FormElement;
        IDictionary<string, object> formValues;
        if (map.PkFieldValues != null && (map.PkFieldValues != null ||
                                          map.PkFieldValues.Count > 0))
        {
            formValues = await _gridView.EntityRepository.GetFieldsAsync(formElement, map.PkFieldValues);
        }
        else
        {
            formValues = await _gridView.FieldsService.GetDefaultValuesAsync(formElement, null, PageState.List);
        }

        var formData = new FormStateData(formValues, _gridView.UserValues, PageState.List);
        string sql = _gridView.ExpressionsService.ParseExpression(cmdAction.CommandSql, formData, false);
        await _gridView.EntityRepository.SetCommandAsync(new DataAccessCommand(sql!));
    }

}