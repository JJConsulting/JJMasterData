using System;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.UI.Components;

internal interface IDataItemControl
{
    public Guid? ConnectionId { get; set; }
    
    public FormElementDataItem DataItem { get; set; }
    
    internal FormStateData FormStateData { get; set; }
}