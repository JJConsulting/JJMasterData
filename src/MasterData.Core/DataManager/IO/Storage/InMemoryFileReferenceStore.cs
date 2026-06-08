using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.IO.Storage;

public sealed class InMemoryFileReferenceStore : IFileReferenceStore
{
    private sealed class Entry
    {
        public FileStorageReference Reference { get; init; }
        public DateTime ExpiresAt { get; init; }
    }

    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(20);
    private readonly ConcurrentDictionary<string, Entry> _entries = new();

    public string Create(FileStorageReference reference)
    {
        CleanupExpired();

        var token = RandomNumberGenerator.GetHexString(32);
        _entries[token] = new Entry
        {
            Reference = reference,
            ExpiresAt = DateTime.UtcNow.Add(TokenLifetime)
        };

        return token;
    }

    public Task<FileStorageReference> ResolveAsync(string token)
    {
        if (string.IsNullOrEmpty(token) || !_entries.TryGetValue(token, out var entry))
            throw new KeyNotFoundException("File token not found.");

        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            _entries.TryRemove(token, out _);
            throw new KeyNotFoundException("File token expired.");
        }

        return Task.FromResult(entry.Reference);
    }

    private void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var item in _entries)
        {
            if (item.Value.ExpiresAt < now)
                _entries.TryRemove(item.Key, out _);
        }
    }
}
