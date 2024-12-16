using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataPanelExpressionScripts(JJDataPanel dataPanel)
{
    private readonly FormElement _formElement = dataPanel.FormElement;

    public string GetHtmlFormScript()
    {
        var script = new StringBuilder();
        var fieldsWithExpression = _formElement.Fields.Where(x => x.EnableExpression.StartsWith("exp:"));
        var pageState = dataPanel.PageState;
        foreach (var field in fieldsWithExpression)
        {
            var expressionBuilder = new StringBuilder(field.EnableExpression);
            expressionBuilder.Replace("exp:", "");
            expressionBuilder.Replace("'{PageState}'", $"'{pageState.GetPageStateName()}'");
            expressionBuilder.Replace("'{PageState}'", $"'{pageState.GetPageStateName()}'");
            expressionBuilder
                .Replace(" and ", " && ")
                .Replace(" or ", " || ")
                .Replace(" AND ", " && ")
                .Replace(" OR ", " || ")
                .Replace("=", " == ")
                .Replace("<>", " != ");

            var list = StringManager.FindValuesByInterval(expressionBuilder.ToString(), '{', '}').ToList();
            if (list.Count == 0)
                continue;

            var expression = ExecuteExpression(expressionBuilder.ToString(), list);

            var selector = string.Join(",", list.Select(x => $"'#{x}'"));
            script.Append('\t');
            script.AppendLine($"$({selector}).change(function () {{");
            script.Append('\t', 2);
            script.AppendLine($"let exp = \"{expression}\";");

            foreach (var t in list)
            {
                script.Append('\t', 2);
                script.Append("exp = exp.replace(\"");
                script.Append('{');
                script.Append(t);
                script.Append("}\", \"'\" + $(\"#");
                script.Append(t);
                script.AppendLine("\").val() + \"'\"); ");
            }

            script.Append('\t', 2);
            script.AppendLine("let enable = eval(exp);");
            script.Append('\t', 2);
            script.AppendLine("if (enable)");
            script.Append('\t', 3);
            script.Append("$(\"#");
            script.Append(field.Name);
            script.AppendLine("\").removeAttr(\"readonly\").removeAttr(\"disabled\");");
            script.Append('\t', 2);
            script.AppendLine("else");
            script.Append('\t', 3);
            script.Append("$(\"#");
            script.Append(field.Name);

            //If disabled, will break GetFormValuesAsync at POST and not receive the values when the expression is exp:.
            script.AppendLine("\").attr(\"readonly\",\"readonly\").val(\"\");");
            script.Append('\t');
            script.AppendLine("});");
        }

        return script.ToString();
    }

    private string ExecuteExpression(string exp, List<string> list)
    {
        var formData = new FormStateData(dataPanel.Values, dataPanel.UserValues, dataPanel.PageState);
        foreach (var fieldName in list)
        {
            string val = null;
            var field = _formElement.Fields.FirstOrDefault(x => x.Name.Equals(fieldName));
            if (field is { AutoPostBack: true })
                continue;

            if (dataPanel.UserValues.TryGetValue(fieldName, out var value))
            {
                val = $"'{value}'";
            }
            else if (dataPanel.CurrentContext.Session[fieldName] != null)
            {
                val = $"'{dataPanel.CurrentContext.Session[fieldName]}'";
            }
            //Hidden fields
            else if (dataPanel.Values.TryGetValue(fieldName, out var panelValue))
            {
                if (field != null)
                {
                    var visible = dataPanel.ExpressionsService.GetBoolValue(field.VisibleExpression, formData);
                    if (!visible)
                    {
                        val = $"'{panelValue}'";
                    }
                }
            }

            if (val != null)
            {
                // Note: Use "{{" to denote a single "{" 
                exp = exp.Replace($"{{{fieldName}}}", val);
            }
        }

        return exp;
    }
}
