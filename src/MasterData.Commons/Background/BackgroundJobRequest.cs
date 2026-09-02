using System;

namespace JJMasterData.Commons.Background;

public abstract class BackgroundJobRequest
{
    public abstract string UserId { get; init; }
    public Guid? Id { get; init; }
}
