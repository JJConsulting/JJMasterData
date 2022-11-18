using JJMasterData.Commons.Settings;
using JJMasterData.Core.Extensions;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Extensions;
using JJMasterData.Web.Models;

namespace JJMasterData.Web.Example;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var root = Path.Join(builder.Environment.ContentRootPath, "..","..");
        var settingsPath = Path.Combine(root, "appsettings.json");
        builder.Configuration.AddJsonFile(settingsPath,false,true);
        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();

        builder.Services.AddOptions<JJMasterDataOptions>();
        builder.Services.AddJJMasterDataWeb().WithFormEvents();
        
        builder.Services.ConfigureOptionsWriter<JJMasterDataOptions>(builder.Configuration.GetSection("JJMasterData"), settingsPath);
        builder.Services.ConfigureOptionsWriter<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"), settingsPath);
        
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