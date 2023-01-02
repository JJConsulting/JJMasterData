using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JJMasterData.Core.WebComponents;

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
            exp = exp.Replace("{pagestate}", $"'{DataPanel.PageState.ToString().ToLower()}'");
            exp = exp
                .Replace(" and ", " && ")
                .Replace(" or ", " || ")
                .Replace(" AND ", " && ")
                .Replace(" OR ", " || ")
                .Replace("=", " == ")
                .Replace("<>", " != ");

            var list = StringManager.FindValuesByInterval(exp, '{', '}').ToList();
            if (list.Count == 0)
                continue;

            exp = ParseExpression(exp, list);

            string selector = string.Join(",", list.Select(x => $"'#{x}'"));
            script.Append('\t');
            script.AppendLine($"$({selector}).change(function () {{");
            script.Append('\t', 2);
            script.AppendLine($"var exp = \"{exp}\";");

            foreach (var value in list)
            {
                script.Append('\t', 2);
                script.Append("exp = exp.replace(\"");
                script.Append('{');
                script.Append(value);
                script.Append("}\", \"'\" + $(\"#");
                script.Append(value);
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

            /*If you change it to disabled, the value will not return in the post and the GetFormValues() routine will
            fail when there is a EnabledExpression*/
            script.AppendLine("\").attr(\"readonly\",\"readonly\").val(\"\");");
            script.Append('\t');
            script.AppendLine("});");
        }

        return script.ToString();
    }

    private string ParseExpression(string exp, IEnumerable<string> list)
    {
        foreach (string fieldName in list)
        {
            string val = null;
            var field = FormElement.Fields.ToList().Find(x => x.Name.Equals(fieldName));
            if (field is { AutoPostBack: true })
                continue;

            if (DataPanel.UserValues.Contains(fieldName))
            {
                //Valor customizado pelo usuário
                val = $"'{DataPanel.UserValues[fieldName]}'";
            }
            else if (DataPanel.HttpContext.Session[fieldName] != null)
            {
                //Valor da Sessão
                val = $"'{DataPanel.HttpContext.Session[fieldName]}'";
            }
            else if (DataPanel.Values.Contains(fieldName))
            {
                //Campos ocultos
                if (field != null)
                {
                    bool visible = DataPanel.FieldManager.IsVisible(field, DataPanel.PageState, DataPanel.Values);
                    if (!visible)
                    {
                        val = $"'{DataPanel.Values[fieldName]}'";
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
