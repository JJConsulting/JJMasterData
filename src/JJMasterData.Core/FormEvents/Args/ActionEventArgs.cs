using System;
using System.Collections;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.FormEvents.Args;

public class ActionEventArgs : EventArgs
{
    /// <summary>
    /// Informações basicas da Ação
    /// </summary>
    public BasicAction Action { get; internal set; }

    /// <summary>
    /// Objeto LinkButton renderizado a partir da ação
    /// </summary>
    public JJLinkButton LinkButton { get; set; }

    /// <summary>
    /// Campos do formulário, composto por um hash com o nome e valor do campo
    /// </summary>
    public Hashtable FieldValues { get; internal set; }

    /// <summary>
    /// (opcional) Retorno esperado com o conteudo HTML renderizado
    /// </summary>
    public string ResultHtml { get; set; }

    public ActionEventArgs(BasicAction action, JJLinkButton linkButton, Hashtable fieldValues)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Action.Name = action.Name;
        LinkButton = linkButton;
        FieldValues = fieldValues;
        ResultHtml = null;
    }
}
