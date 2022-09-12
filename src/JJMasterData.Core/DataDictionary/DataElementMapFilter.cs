using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class DataElementMapFilter
{
    [DataMember(Name = "fieldName")]
    public string FieldName { get; set; }

    [DataMember(Name = "expressionValue")]
    public string ExpressionValue { get; set; }

}