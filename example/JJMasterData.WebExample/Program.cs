using JJMasterData.Core.Extensions;
using JJMasterData.Web.Extensions;
using JJMasterData.Pdf;

namespace JJMasterData.WebExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var root = Path.GetFullPath(Path.Join(builder.Environment.ContentRootPath, "..", ".."));
        var settingsPath = Path.Combine(root, "appsettings.json");

        //Without this IConfiguration don't work. If we add inside AddJJMasterDataWeb, I don't know why performance goes to trash.
        builder.Configuration.AddJsonFile(settingsPath, optional: false, reloadOnChange: true);
        builder.Services.AddRazorPages().AddViewLocalization();
        builder.Services.AddControllersWithViews();
        builder.Services.AddJJMasterDataWeb(settingsPath)
            .WithFormEventResolver()
            .WithPdfExportation();
            
            //.WithMongoDbDataDictionary();
            //.WithPythonFormEventResolver(options => options.ScriptsPath = "../../example/JJMasterData.WebExample/FormEvents/Python");
        
        //You can also:
        // builder.Services.AddJJMasterDataWeb(options =>
        // {
        //     options.JJMasterData.BootstrapVersion = 5;
        // });
        
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