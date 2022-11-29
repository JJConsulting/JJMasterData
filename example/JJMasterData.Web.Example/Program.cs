using JJMasterData.MongoDB.Extensions;
using JJMasterData.Web.Extensions;

namespace JJMasterData.Web.Example;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var root = Path.Join(builder.Environment.ContentRootPath, "..", "..");
        var settingsPath = Path.Combine(root, "appsettings.json");
        
        builder.Configuration.AddJsonFile(settingsPath, false, true);

        builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews();

        builder.AddJJMasterDataWeb(settingsPath).WithMongoDB(mongo =>
        {
            var section = builder.Configuration.GetSection("JJMasterData:MongoDB");
            mongo.ConnectionString = section.GetValue<string>("ConnectionString")!;
            mongo.CollectionName = section.GetValue<string>("CollectionName")!;
            mongo.DatabaseName = section.GetValue<string>("DatabaseName")!;
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