using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.DI;

/// <summary>
/// Static service locator. We're aware this is a anti-pattern, this was made to prevent breaking changes at .NET Framework 4.8.
/// This class will be removed at the next version, DON'T USE at your applications, always use constructor injector.
/// </summary>
public static class JJService
{
    public static IServiceProvider Provider { get; internal set; }
}
