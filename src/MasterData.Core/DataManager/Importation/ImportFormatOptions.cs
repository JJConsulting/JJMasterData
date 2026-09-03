using System.Collections.Generic;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.DataManager.Importation;

public abstract class ImportFormatOptions : FormatOptions
{
    protected internal abstract IReadOnlyList<string> FileExtensions { get; }
    protected internal abstract IReadOnlyList<string> ContentTypes { get; }
}

public sealed class ImportFormatMetadata(
    string id,
    string displayName,
    IReadOnlyList<string> fileExtensions,
    IReadOnlyList<string> contentTypes,
    IReadOnlyList<FormatOptionMetadata> options)
{
    public string Id { get; } = id;
    public string DisplayName { get; } = displayName;
    public IReadOnlyList<string> FileExtensions { get; } = fileExtensions;
    public IReadOnlyList<string> ContentTypes { get; } = contentTypes;
    public IReadOnlyList<FormatOptionMetadata> Options { get; } = options;
}
