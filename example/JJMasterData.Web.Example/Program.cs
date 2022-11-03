using JJMasterData.Core.Extensions;
using JJMasterData.Web.Extensions;

namespace JJMasterData.Web.Example;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var root = Path.Join(builder.Environment.ContentRootPath, "..","..");
        var sharedSettings = Path.Combine(root, "appsettings.json");

        builder.Configuration.AddJsonFile(sharedSettings);
        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();

        builder.Services.AddJJMasterDataWeb().WithFormEvents()
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
    }
}