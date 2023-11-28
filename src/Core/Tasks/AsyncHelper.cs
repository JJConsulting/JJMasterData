
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
#if NETFRAMEWORK
using System.Web;
#endif
namespace JJMasterData.Core.Tasks;

public static class AsyncHelper
{
    private static readonly TaskFactory _taskFactory = new 
        TaskFactory(CancellationToken.None, TaskCreationOptions.None, 
            TaskContinuationOptions.None, TaskScheduler.Default);

    public static TResult RunSync<TResult>(Func<Task<TResult>> func)
    {
        var cultureUi = CultureInfo.CurrentUICulture;
        var culture = CultureInfo.CurrentCulture;
        #if NETFRAMEWORK
        return _taskFactory.StartNew( (context) =>
        {
            HttpContext.Current = context as HttpContext;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
        }, HttpContext.Current).Unwrap().GetAwaiter().GetResult();
        #else
        return _taskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
        }).Unwrap().GetAwaiter().GetResult();
        #endif
    }

    public static void RunSync(this Func<Task> func)
    {
        var cultureUi = CultureInfo.CurrentUICulture;
        var culture = CultureInfo.CurrentCulture;
#if NETFRAMEWORK
        _taskFactory.StartNew( (context) =>
        {
            HttpContext.Current = context as HttpContext;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
        }, HttpContext.Current).Unwrap().GetAwaiter().GetResult();
#else
        _taskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
        }).Unwrap().GetAwaiter().GetResult();
#endif
    }
}
