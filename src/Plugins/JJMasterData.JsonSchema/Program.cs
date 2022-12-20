using JJMasterData.JsonSchema.Writers;

var optionsWriter = new JJMasterDataOptionsWriter();
var metadataWriter = new MetadataWriter();

await optionsWriter.WriteAsync();
await metadataWriter.WriteAsync();
