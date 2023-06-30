using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.Web.Components;

internal class DataPanelScript
{   
    private JJDataPanel DataPanel { get; set; }

    private FormElement FormElement => DataPanel.FormElement;

    public DataPanelScript(JJDataPanel dataPanel)
    {
        DataPanel = dataPanel;
    }

    public string GetHtmlFormScript()
    {
        var script = new StringBuilder();
        var listFieldsExp = FormElement.Fields.ToList().FindAll(x => x.EnableExpression.StartsWith("exp:"));

        foreach (var f in listFieldsExp)
        {
            string exp = f.EnableExpression.Replace("exp:", "");
            exp = exp.Replace("{pagestate}", $"'{DataPanel.PageState.ToString()}'");
            exp = exp.Replace("{PAGESTATE}", $"'{DataPanel.PageState.ToString()}'");
            exp = exp
                .Replace(" and ", " && ")
                .Replace(" or ", " || ")
                .Replace(" AND ", " && ")
                .Replace(" OR ", " || ")
                .Replace("=", " == ")
                .Replace("<>", " != ");

            List<string> list = StringManager.FindValuesByInterval(exp, '{', '}');
            if (list.Count == 0)
                continue;

            exp = ParseExpression(exp, list);

            string selector = string.Join(",", list.Select(x => $"'#{x}'"));
            script.Append('\t');
            script.AppendLine($"$({selector}).change(function () {{");
            script.Append('\t', 2);
            script.AppendLine($"var exp = \"{exp}\";");

            for (int i = 0; i < list.Count; i++)
            {
                script.Append('\t', 2);
                script.Append("exp = exp.replace(\"");
                script.Append("{");
                script.Append(list[i]);
                script.Append("}\", \"'\" + $(\"#");
                script.Append(list[i]);
                script.AppendLine("\").val() + \"'\"); ");
            }

            script.Append('\t', 2);
            script.AppendLine("var enable = eval(exp);");
            script.Append('\t', 2);
            script.AppendLine("if (enable)");
            script.Append('\t', 3);
            script.Append("$(\"#");
            script.Append(f.Name);
            script.AppendLine("\").removeAttr(\"readonly\").removeAttr(\"disabled\");");
            script.Append('\t', 2);
            script.AppendLine("else");
            script.Append('\t', 3);
            script.Append("$(\"#");
            script.Append(f.Name);

            //Se alterar para disabled o valor não voltará no post e vai zuar a rotina GetFormValues() qd exisir exp EnabledExpression
            script.AppendLine("\").attr(\"readonly\",\"readonly\").val(\"\");");
            script.Append('\t');
            script.AppendLine("});");
        }

        return script.ToString();
    }

    private string ParseExpression(string exp, List<string> list)
    {
        foreach (string fieldName in list)
        {
            string val = null;
            var field = FormElement.Fields.ToList().Find(x => x.Name.Equals(fieldName));
            if (field != null && field.AutoPostBack)
                continue;

            if (DataPanel.UserValues.TryGetValue(fieldName, out var value))
            {
                //Valor customizado pelo usuário
                val = $"'{value}'";
            }
            else if (DataPanel.CurrentContext.Session[fieldName] != null)
            {
                //Valor da Sessão
                val = $"'{DataPanel.CurrentContext.Session[fieldName]}'";
            }
            else if (DataPanel.Values.TryGetValue(fieldName, out var panelValue))
            {
                //Campos ocultos
                if (field != null)
                {
                    bool visible = DataPanel.FieldManager.IsVisible(field, DataPanel.PageState, DataPanel.Values);
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
