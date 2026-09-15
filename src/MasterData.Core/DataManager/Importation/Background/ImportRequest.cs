using System.Collections.Generic;
using System.Globalization;
using JJMasterData.Commons;
using JJMasterData.Commons.Background;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.Events.Args;

namespace JJMasterData.Core.DataManager.Importation.Background;

public sealed class ImportRequest : BackgroundJobRequest
{
    public string CultureName { get; init; } = CultureInfo.InvariantCulture.Name;
    public string UICultureName { get; init; } = CultureInfo.InvariantCulture.Name;
    internal AsyncEventHandler<FormAfterActionEventArgs>? OnAfterDeleteAsync { get; init; }
    internal AsyncEventHandler<FormAfterActionEventArgs>? OnAfterInsertAsync { get; init; }
    internal AsyncEventHandler<FormAfterActionEventArgs>? OnAfterUpdateAsync { get; init; }
    internal AsyncEventHandler<FormBeforeActionEventArgs>? OnBeforeImportAsync { get; init; }
    internal AsyncEventHandler<FormAfterActionEventArgs>? OnAfterProcessAsync { get; init; }

    public required string ElementName { get; init; }
    public override required string UserId { get; init; }
    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public string? ContentType { get; init; }
    public string? FormatId { get; init; }
    public required CsvImportOptions Options { get; init; }
    public required Dictionary<string, object?> RelationValues { get; init; }
    public required Dictionary<string, object?> UserValues { get; init; }
    public string? IpAddress { get; init; }
    public string? BrowserInfo { get; init; }
    public string? CommandBeforeProcess { get; init; }
    public string? CommandAfterProcess { get; init; }
}
