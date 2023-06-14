#nullable enable

using System.Collections.Generic;
using System.Reflection;

namespace JJMasterData.Core.FormEvents;

public class FormEventOptions
{
    /// <summary>
    /// Default value: null <br></br>
    /// </summary>
    public IEnumerable<Assembly>? Assemblies { get; set; }

}