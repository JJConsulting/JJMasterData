using JJMasterData.Core.Extensions;
using JJMasterData.Web.Extensions;

namespace JJMasterData.Web.Bootstraper;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews().AddViewLocalization();
        builder.Services.AddJJMasterDataWeb().WithFormEvents();
        
        var app = builder.Build();

        if (app.Environment.IsProduction())
            app.UseHsts();

        app.UseExceptionHandler("/Error");
        app.UseStatusCodePagesWithReExecute("/Error","?statusCode={0}");

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseJJMasterDataWeb();

        app.Run();
    }
}