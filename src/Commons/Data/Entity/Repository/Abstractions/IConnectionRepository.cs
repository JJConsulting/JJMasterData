using System;
using System.Collections.Generic;
using JJMasterData.Commons.Configuration.Options;

namespace JJMasterData.Commons.Data.Entity.Repository.Abstractions;

public interface IConnectionRepository
{
    List<ConnectionString> GetAll();
    ConnectionString Get(Guid? connectionId);
}