using System.Net;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Brasil.Configuration;
using JJMasterData.Web.Configuration;
using Microsoft.AspNetCore.ResponseCompression;
using JJMasterData.Pdf;
using JJMasterData.WebApi.Configuration;
using JJMasterData.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
    throw new InvalidOperationException("The MasterData example host can only run in the Development environment.");

var root = Path.GetFullPath(Path.Join(builder.Environment.ContentRootPath, "..", ".."));
var settingsPath = Path.Combine(root, "appsettings.json");

builder.Configuration.AddJsonFile(settingsPath, optional: false, reloadOnChange: true);
builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
{
    ["AllowedHosts"] = "localhost;127.0.0.1;[::1]"
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = 
        BackgroundServiceExceptionBehavior.Ignore;
});
builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithPdfExportation()
    .WithBrasilActionPlugins(builder.Configuration);
builder.Services.AddJJMasterDataWebApi();
builder.Services.AddResponseCaching();
var app = builder.Build();

app.Use(async (context, next) =>
{
    var address = context.Connection.RemoteIpAddress;
    if (address?.IsIPv4MappedToIPv6 == true)
        address = address.MapToIPv4();

    if (address is null || !IPAddress.IsLoopback(address))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    await next(context);
});

using var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<IDataDictionaryRepository>().CreateStructureIfNotExistsAsync();

app.UseResponseCompression();

app.UseDeveloperExceptionPage();

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseResponseCaching();
await app.UseMasterDataSeedingAsync();

app.MapDataDictionary();
app.MapMasterData();
app.MapDictionaries();
app.MapMasterDataApi();
app.Run();
