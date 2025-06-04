using System.Collections.Generic;

namespace JJMasterData.Brasil.Models;


public class CnaeResult
{
    public required string Code { get; set; }
    public required string Text { get; set; }

    public Dictionary<string,object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { nameof(Code), Code },
            { nameof(Text), Text }
        };
    }
}