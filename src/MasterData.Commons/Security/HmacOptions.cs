using System;

namespace JJMasterData.Commons.Security;

public class HmacOptions
{
    public string SecretKey { get; set; } = string.Empty; 
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
}