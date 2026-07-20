#nullable enable
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Security;

public class HmacHelper
{
    private readonly byte[] _key;
    private readonly TimeSpan _clockSkew;

    public HmacHelper(IOptions<HmacOptions> options)
    {
        var opt = options.Value;

        _key = Encoding.UTF8.GetBytes(opt.SecretKey ?? string.Empty);
        _clockSkew = opt.ClockSkew;
    }

    public string Generate(string content, DateTime? expiresUtc = null)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);

        var ticks = expiresUtc?.ToUniversalTime().Ticks;

        var data = ticks.HasValue
            ? Combine(contentBytes, BitConverter.GetBytes(ticks.Value))
            : contentBytes;

        using var hmac = new HMACSHA256(_key);
        var fullSig = hmac.ComputeHash(data);

        var truncated = fullSig[..16];

        var signature = Base64Url(truncated);

        return ticks.HasValue
            ? $"{signature}.{ticks.Value}"
            : signature;
    }

    public bool Validate(string content, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.');
    
        var signaturePart = parts[0];
        long? ticks = parts.Length > 1 ? long.Parse(parts[1]) : null;

        if (ticks.HasValue)
        {
            var now = DateTime.UtcNow.Ticks;
            if (now > ticks.Value + _clockSkew.Ticks)
                return false;
        }

        var contentBytes = Encoding.UTF8.GetBytes(content);

        var data = ticks.HasValue
            ? Combine(contentBytes, BitConverter.GetBytes(ticks.Value))
            : contentBytes;

        using var hmac = new HMACSHA256(_key);
        var expectedFull = hmac.ComputeHash(data);
        var expected = expectedFull[..16];

        var provided = Base64UrlDecode(signaturePart);

        return CryptographicOperations.FixedTimeEquals(expected, provided);
    }
    
    private static byte[] Combine(byte[] a, byte[] b)
    {
        var result = new byte[a.Length + b.Length];
        Buffer.BlockCopy(a, 0, result, 0, a.Length);
        Buffer.BlockCopy(b, 0, result, a.Length, b.Length);
        return result;
    }

    private static string Base64Url(byte[] input) =>
        Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static byte[] Base64UrlDecode(string input)
    {
        var padded = input
            .Replace('-', '+')
            .Replace('_', '/');

        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');

        return Convert.FromBase64String(padded);
    }

    public static DateTime? ParseExpirationUtc(string? expirationValue)
    {
        if (string.IsNullOrWhiteSpace(expirationValue)) return null;
        if (long.TryParse(expirationValue, out var ticks)) return new DateTime(ticks, DateTimeKind.Utc);
        if (DateTimeOffset.TryParse(expirationValue, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var expirationOffset))
        {
            return expirationOffset.UtcDateTime;
        }

        if (DateTime.TryParse(expirationValue, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var expirationDateTime))
        {
            return expirationDateTime.ToUniversalTime();
        }

        throw new ArgumentException("Invalid HMAC expiration. Use UTC date/time or ticks.");
    }
}