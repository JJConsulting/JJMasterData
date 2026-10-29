using JJMasterData.Commons.Background;
using JJMasterData.Core.DataManager.Importation.Background;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Importation;

public sealed class ImportJobServiceTests
{
    [Fact]
    public void LastStatusFindsCompletedJobForTheCurrentElementAndUser()
    {
        var id = BackgroundJobId.Create("import", "customers", "owner");
        var completed = new BackgroundJobSnapshot
        {
            Id = id,
            UserId = "owner",
            State = BackgroundJobState.Succeeded,
            CreatedAt = DateTimeOffset.UtcNow,
            StartedAt = null,
            CompletedAt = DateTimeOffset.UtcNow,
            Progress = null,
            Result = null,
            Error = null
        };
        var jobs = new Mock<IBackgroundJobClient>();
        jobs.Setup(client => client.GetStatus(id, "owner")).Returns(completed);
        var service = new ImportJobService(jobs.Object);

        Assert.Same(completed, service.GetLastStatus("customers", "owner"));
        Assert.Null(service.GetCurrentStatus("customers", "owner"));
        Assert.Null(service.GetLastStatus("customers", "other"));
        Assert.Null(service.GetLastStatus("orders", "owner"));
    }
}
