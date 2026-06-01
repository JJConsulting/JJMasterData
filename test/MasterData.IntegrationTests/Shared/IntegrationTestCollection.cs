namespace JJMasterData.IntegrationTests.Shared;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection
{
    public const string Name = "MasterData Integration Tests";
}
