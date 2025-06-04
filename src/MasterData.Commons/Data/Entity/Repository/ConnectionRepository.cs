using System;
using System.Collections.Generic;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Data.Entity.Repository;

internal sealed class ConnectionRepository(IOptionsSnapshot<MasterDataCommonsOptions> optionsSnapshot) : IConnectionRepository
{
    public List<ConnectionString> GetAll()
    {
        return optionsSnapshot.Value.AdditionalConnectionStrings;
    }

    public ConnectionString Get(Guid? connectionId)
    {
        return optionsSnapshot.Value.GetConnectionString(connectionId);
    }
}