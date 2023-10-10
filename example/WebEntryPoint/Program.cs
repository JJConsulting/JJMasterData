using JJMasterData.Brasil.Configuration;
using JJMasterData.Protheus.Configuration;
using JJMasterData.Web.Extensions;
using ReportPortal.Auth;

var builder = WebApplication.CreateBuilder(args);

var root = Path.GetFullPath(Path.Join(builder.Environment.ContentRootPath, "..", ".."));
var settingsPath = Path.Combine(root, "appsettings.json");
builder.Configuration.AddJsonFile(settingsPath, optional: true, reloadOnChange: true);

var authentication = builder.Configuration.GetValue<string>("Authentication");

builder.Services.AddJJMasterDataWeb(builder.Configuration)
    .WithProtheusServices()
    .WithBrasilActionPlugins()
    .WithWebOptimizer(options =>
    {
        options.AddCssBundle("/css/bootstrap.min.css", "css/bootstrap/bootstrap.css").MinifyCss();
    });

builder.Services.AddControllersWithViews().AddViewLocalization();

if (authentication == "ReportPortal")
{
    builder.Services.AddAuthentication().WithReportPortal();
}

var app = builder.Build();


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
app.UseStaticFiles();
app.UseRouting();
if (authentication is not null)
{
    app.UseAuthentication();
}
app.UseAuthorization();
app.UseJJMasterDataWeb();

var mapJJMasterData = app.MapJJMasterData();
if (authentication is not null)
{
    mapJJMasterData.RequireAuthorization();
}

app.Run();
