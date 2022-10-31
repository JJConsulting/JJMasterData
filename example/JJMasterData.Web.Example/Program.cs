using JJMasterData.Core.Extensions;
using JJMasterData.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddJJMasterDataWeb()
    .WithFormEvents()
    .WithSettings(s =>
    {
        s.LayoutPath = "_Layout";
    });

var app = builder.Build();

if (app.Environment.IsProduction())
    app.UseHsts();

app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.UseJJMasterDataWeb();
app.UseAuthorization();
app.Run();

public partial class Program { }