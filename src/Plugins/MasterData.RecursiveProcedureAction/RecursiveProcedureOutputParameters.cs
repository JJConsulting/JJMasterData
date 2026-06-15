using System.Data;
using JJMasterData.Commons.Data;

namespace JJMasterData.RecursiveProcedureAction;

internal class RecursiveProcedureOutputParameters
{
    public const string ParamEndExec = "endexec";
    public const string ParamMessageContent = "msg";
    public const string ParamMessageType = "typemsg";
    public const string ParamMessageSize = "sizemsg";
    public const string ParamUrlPost = "urlpost";
    public const string ParamUrlPayment = "urlpayment";
    public const string ParamElementRedirect = "elementRedirect";

    public DataAccessParameter PaymentApiParameter { get; } = new()
    {
        Name = ParamUrlPayment,
        Direction = ParameterDirection.Output,
        Size = -1
    };

    public DataAccessParameter EndExecutionParameter { get; } = new()
    {
        Name = ParamEndExec,
        Type = DbType.Boolean,
        Size = 1,
        Direction = ParameterDirection.Output
    };

    public DataAccessParameter MessageContentParameter { get; } = new()
    {
        Name = ParamMessageContent,
        Size = -1,
        Direction = ParameterDirection.InputOutput
    };

    public DataAccessParameter MessageTypeParameter { get; } = new()
    {
        Name = ParamMessageType,
        Type = DbType.Int32,
        Direction = ParameterDirection.Output
    };

    public DataAccessParameter MessageSizeParameter { get; } = new()
    {
        Name = ParamMessageSize,
        Type = DbType.Int32,
        Direction = ParameterDirection.Output
    };

    public DataAccessParameter UrlPostParameter { get; } = new()
    {
        Name = ParamUrlPost,
        Size = -1,
        Direction = ParameterDirection.InputOutput
    };

    public DataAccessParameter ElementRedirectParameter { get; } = new()
    {
        Name = ParamElementRedirect,
        Size = -1,
        Direction = ParameterDirection.Output
    };
    
    public IEnumerable<DataAccessParameter> GetAllParameters()
    {
        yield return PaymentApiParameter;
        yield return EndExecutionParameter;
        yield return MessageContentParameter;
        yield return MessageTypeParameter;
        yield return MessageSizeParameter;
        yield return UrlPostParameter;
        yield return ElementRedirectParameter;
    }
    public void Deconstruct(
        out DataAccessParameter endExecutionParameter,
        out DataAccessParameter messageContentParameter,
        out DataAccessParameter messageTypeParameter,
        out DataAccessParameter messageSizeParameter,
        out DataAccessParameter urlPostParameter,
        out DataAccessParameter paymentApiParameter,
        out DataAccessParameter elementRedirectParameter
    )
    {
        paymentApiParameter = PaymentApiParameter;
        endExecutionParameter = EndExecutionParameter;
        messageContentParameter = MessageContentParameter;
        messageTypeParameter = MessageTypeParameter;
        messageSizeParameter = MessageSizeParameter;
        urlPostParameter = UrlPostParameter;
        elementRedirectParameter = ElementRedirectParameter;
    }
}