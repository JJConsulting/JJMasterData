using System.Data;
using JJMasterData.Commons.Data;
using JJMasterData.Core.DataManager;

namespace JJMasterData.RecursiveProcedureAction;

internal class RecursiveProcedureInputParameters
{
    private const string ParamPrimaryKey = "id";
    private const string ParamObs = "obs";
    private const string ParamUserId = "userid";
    private const string ParamExecutionSequence = "seqexec";

    public DataAccessParameter PrimaryKeyParameter { get; } = new()
    {
        Name = ParamPrimaryKey,
        Type = DbType.AnsiString,
    };

    public DataAccessParameter UserIdParameter { get; } = new()
    {
        Name = ParamUserId,
        Type = DbType.AnsiString,
        Size = null,
    };

    public DataAccessParameter ExecutionSequenceParameter { get; } = new()
    {
        Name = ParamExecutionSequence,
        Type = DbType.Int32
    };

    public DataAccessParameter ObsParameter { get; } = new()
    {

        Name = ParamObs,
        Size = -1
    };

    public IEnumerable<DataAccessParameter> GetAllParameters()
    {
        yield return PrimaryKeyParameter;
        yield return UserIdParameter;
        yield return ExecutionSequenceParameter;
        yield return ObsParameter;
    }
}