using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        try
        {
            if (IsApplyOnList(map, cmdAction))
            {
                var selectedRows = _gridView.GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = _gridView.StringLocalizer["No lines selected."];
                    return new JJMessageBox(msg, MessageIcon.Warning);
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
            return new JJMessageBox(msg, MessageIcon.Error);
        }

        return null;
    }

    private bool IsApplyOnList(ActionMap map, SqlCommandAction cmdAction)
    {
        return map.ActionSource == ActionSource.GridToolbar
               && _gridView.EnableMultiSelect
               && cmdAction.ApplyOnSelected;
    }

    private async Task ExecuteOnList(SqlCommandAction cmdAction, List<IDictionary<string, dynamic>> selectedRows)
    {
        var listSql = new List<string>();
        foreach (var row in selectedRows)
        {
            string sql = _gridView.ExpressionsService.ParseExpression(cmdAction.CommandSql, PageState.List, false, row);
            listSql.Add(sql);
        }

        await _gridView.EntityRepository.SetCommandAsync(listSql);
    }

    private async Task ExecuteOnRecord(ActionMap map, SqlCommandAction cmdAction)
    {
        var formElement = _gridView.FormElement;
        IDictionary<string, dynamic> formValues;
        if (map.PkFieldValues != null && (map.PkFieldValues != null ||
                                          map.PkFieldValues.Count > 0))
        {
            formValues = await _gridView.EntityRepository.GetDictionaryAsync(formElement, map.PkFieldValues);
        }
        else
        {
            formValues = await _gridView.FieldsService.GetDefaultValuesAsync(formElement, null, PageState.List);
        }

        string sql = _gridView.ExpressionsService.ParseExpression(cmdAction.CommandSql, PageState.List, false, formValues);
        await _gridView.EntityRepository.SetCommandAsync(sql);
    }
    
}