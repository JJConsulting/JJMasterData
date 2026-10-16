using System.Data.Common;

namespace JJMasterData.Commons.Data;

public interface IDataAccessMapper<out T>
{
    T Map(DbDataReader reader);
}
