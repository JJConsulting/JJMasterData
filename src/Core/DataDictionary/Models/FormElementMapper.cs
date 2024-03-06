using Riok.Mapperly.Abstractions;

namespace JJMasterData.Core.DataDictionary.Models;

[Mapper(
    UseDeepCloning = true,
    IgnoreObsoleteMembersStrategy = IgnoreObsoleteMembersStrategy.Target
    )]
public static partial class FormElementMapper
{
    public static partial FormElement Clone(FormElement formElement);
}