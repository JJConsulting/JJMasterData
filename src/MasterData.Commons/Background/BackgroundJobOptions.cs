using System;

namespace JJMasterData.Commons.Background;

public sealed class BackgroundJobOptions
{
    public const string SectionName = "JJMasterData:BackgroundJobs";

    public int Capacity { get; set; } = int.MaxValue;
    public int MaxConcurrency { get; set; } = 1000;
    public TimeSpan CompletedJobRetention { get; set; } = TimeSpan.FromHours(1);
}
