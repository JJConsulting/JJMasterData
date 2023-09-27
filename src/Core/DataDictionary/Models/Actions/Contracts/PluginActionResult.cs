#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginActionResult
{
    [JsonProperty("jsCallback")] 
    public string? JsCallback { get; set; }
}