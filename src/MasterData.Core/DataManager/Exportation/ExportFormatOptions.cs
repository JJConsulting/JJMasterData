using System.Collections.Generic;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.DataManager.Exportation;

public abstract class ExportFormatOptions : FormatOptions
{
    protected internal abstract string FileExtension { get; }
    protected internal abstract string ContentType { get; }
}

public sealed class ExportFormatMetadata(
    string id,
    string displayName,
    string fileExtension,
    string contentType,
    IReadOnlyList<FormatOptionMetadata> options)
{
    public string Id { get; } = id;
    public string DisplayName { get; } = displayName;
    public string FileExtension { get; } = fileExtension;
    public string ContentType { get; } = contentType;
    public IReadOnlyList<FormatOptionMetadata> Options { get; } = options;
}
