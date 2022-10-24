using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Html;
using Newtonsoft.Json;


namespace JJMasterData.Core.WebComponents;

internal class DataPanelScript
{
    public PageState PageState { get; set; }

    public FormElement FormElement { get; set; }

    private string GetHtmlFormScript()
    {
        var sHtml = new StringBuilder();
        //var listFieldsPost = FormElement.Fields.ToList().FindAll(x => x.AutoPostBack);
        var listFieldsExp = FormElement.Fields.ToList().FindAll(x => x.EnableExpression.StartsWith("exp:"));

        //string functionname = string.Format("do_reload_{0}", Name);

     

        foreach (var f in listFieldsExp)
        {
            string exp = f.EnableExpression.Replace("exp:", "");
            exp = exp.Replace("{pagestate}", string.Format("'{0}'", PageState.ToString()));
            exp = exp.Replace("{PAGESTATE}", string.Format("'{0}'", PageState.ToString()));
            exp = exp
                .Replace(" and ", " && ")
                .Replace(" or ", " || ")
                .Replace(" AND ", " && ")
                .Replace(" OR ", " || ")
                .Replace("=", " == ")
                .Replace("<>", " != ");

            List<string> list = StringManager.FindValuesByInterval(exp, '{', '}');
            if (list.Count > 0)
            {
                foreach (string field in list)
                {
                    string val = null;
                    if (UserValues.Contains(field))
                    {
                        //Valor customizado pelo usuário
                        val = string.Format("'{0}'", UserValues[field]);
                    }
                    else if (CurrentContext.Session[field] != null)
                    {
                        //Valor da Sessão
                        val = string.Format("'{0}'", CurrentContext.Session[field]);
                    }
                    else
                    {
                        //Campos ocultos
                        if (Values.Contains(field))
                        {
                            var fTemp = FormElement.Fields.ToList().Find(x => x.Name.Equals(field));
                            if (fTemp != null)
                            {
                                bool visible = FieldManager.IsVisible(fTemp, PageState, Values);
                                if (!visible)
                                {
                                    val = string.Format("'{0}'", Values[field]);
                                }
                            }
                        }
                    }

                    if (val != null)
                    {
                        // Note: Use "{{" to denote a single "{" 
                        exp = exp.Replace(string.Format("{{{0}}}", field), val);
                    }
                }

                sHtml.Append("\t\t$(\"");
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                        sHtml.Append(",");

                    sHtml.Append("#");
                    sHtml.Append(list[i]);
                }

                sHtml.AppendLine("\").change(function () {");
                sHtml.Append("\t\t\tvar exp = \"");
                sHtml.Append(exp);
                sHtml.AppendLine("\";");

                for (int i = 0; i < list.Count; i++)
                {
                    sHtml.Append("\t\t\texp = exp.replace(\"");
                    sHtml.Append("{");
                    sHtml.Append(list[i]);
                    sHtml.Append("}\", \"'\" + $(\"#");
                    sHtml.Append(list[i]);
                    sHtml.AppendLine("\").val() + \"'\"); ");
                }

                sHtml.AppendLine("\t\t\tvar enable = eval(exp);");
                sHtml.AppendLine("\t\t\tif (enable)");
                sHtml.Append("\t\t\t\t$(\"#");
                sHtml.Append(f.Name);
                sHtml.AppendLine("\").removeAttr(\"readonly\").removeAttr(\"disabled\");");
                sHtml.AppendLine("\t\t\telse");
                sHtml.Append("\t\t\t\t$(\"#");
                sHtml.Append(f.Name);

                //Se alterar para disabled o valor não voltará no post e vai zuar a rotina GetFormValues() qd exisir exp EnabledExpression
                sHtml.AppendLine("\").attr(\"readonly\",\"readonly\").val(\"\");");
                sHtml.AppendLine("\t\t});");
            }
        }



        return sHtml.ToString();
    }


}
