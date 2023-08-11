#if NET48
using System;

namespace JJMasterData.Commons.Configuration;

/// <summary>
/// Static service locator. We're aware this is a anti-pattern, this was made to prevent breaking changes at .NET Framework 4.8.
/// This class will be removed at the next version, DON'T USE at your applications, always use constructor injector.
/// </summary>

[Obsolete("Static service locator can lead to runtime errors.")]

public static class StaticServiceLocator
{
    public static IServiceProvider Provider { get; internal set; }
}

#endif