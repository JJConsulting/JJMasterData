// This is a debug and example purposes Program.cs

using JJMasterData.Web.Configuration;
using Microsoft.AspNetCore.ResponseCompression;
using JJMasterData.Pdf;

var builder = WebApplication.CreateBuilder(args);

var root = Path.GetFullPath(Path.Join(builder.Environment.ContentRootPath, "..", ".."));
var settingsPath = Path.Combine(root, "appsettings.json");

builder.Configuration.AddJsonFile(settingsPath, optional: false, reloadOnChange: true);

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
builder.Services.AddJJMasterDataWeb(builder.Configuration).WithPdfExportation();

var app = builder.Build();

app.UseResponseCompression();

if (app.Environment.IsProduction())
{
    app.UseHsts();
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseJJMasterDataWeb();

//Here you can also app.MapJJMasterData().RequireAuthorization();
app.MapJJMasterData();

app.Run();
