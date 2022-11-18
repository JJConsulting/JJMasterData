using System;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Settings;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.DI;

public static class JJService
{
    public static IServiceProvider Provider { get; internal set; }

    public static IEntityRepository EntityRepository
    {
        get
        {
            using var scope = Provider?.CreateScope();
            return scope?.ServiceProvider.GetService<IEntityRepository>() ?? new Factory();
        }
    }
    public static ISettings Settings => Provider?.GetService<ISettings>() ?? new JJMasterDataSettings();
    public static IBackgroundTask BackgroundTask => Provider?.GetService<IBackgroundTask>() ?? Tasks.BackgroundTask.GetInstance();
    public static ITranslator Translator => Provider?.GetService<ITranslator>() ?? new DbTranslatorProvider();
    public static ILogger Logger => Provider?.GetService<ILogger>() ?? new Logger();
}
