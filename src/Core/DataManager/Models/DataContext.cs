﻿#nullable enable

using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Models;

public class DataContext
{
    public DataContextSource Source { get; private set; }

    public string? UserId { get; private set; }

    public string? IpAddress { get; internal set; }
    
    public string? BrowserInfo { get; internal set; }

    public DataContext()
    {
        
    }
    
    public DataContext(IHttpRequest request, DataContextSource source, string? userId)
    {
        Source = source;
        UserId = userId;
        IpAddress = request.UserHostAddress;
        BrowserInfo = request.UserAgent;
    }

}