#nullable enable
using System;
using JJMasterData.Commons.Configuration.Options;

namespace JJMasterData.Core.Configuration.Options;

public class MasterDataCoreOptionsConfiguration
{
    public Action<MasterDataCoreOptions>? ConfigureCore { get; set; } 
    public Action<MasterDataCommonsOptions>? ConfigureCommons { get; set; }
}