﻿using System;
using System.Collections;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.FormEvents.Args;

public class ActionEventArgs : EventArgs
{
    public BasicAction Action { get; internal set; }
    
    public JJLinkButton LinkButton { get; set; }
    
    public IDictionary<string,dynamic> FieldValues { get; internal set; }
    
    public string HtmlResult { get; set; }

    public ActionEventArgs(BasicAction action, JJLinkButton linkButton, IDictionary<string,dynamic> fieldValues)
    {
        Action = action ?? throw new ArgumentNullException(nameof(action));
        Action.Name = action.Name;
        LinkButton = linkButton;
        FieldValues = fieldValues;
        HtmlResult = null;
    }
}