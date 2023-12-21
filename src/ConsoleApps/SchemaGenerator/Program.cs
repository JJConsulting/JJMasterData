using JJMasterData.Core.Configuration;
using JJMasterData.SchemaGenerator;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddJJMasterDataCore();
services.AddTransient<JsonSchemaService>();
var serviceProvider = services.BuildServiceProvider();

var service = serviceProvider.GetRequiredService<JsonSchemaService>();

service.GenerateJsonSchema("MasterDataOptions");