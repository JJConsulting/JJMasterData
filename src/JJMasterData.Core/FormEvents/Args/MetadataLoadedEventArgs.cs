using System;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.FormEvents.Args;

public class MetadataLoadEventArgs : EventArgs
{
    public Metadata Metadata { get; }
    
    public MetadataLoadEventArgs(Metadata metadata)
    {
        Metadata = metadata;
    }
}