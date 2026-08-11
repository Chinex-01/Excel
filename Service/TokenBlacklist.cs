using Microsoft.Extensions.Caching.Memory;

namespace Excel.Service
{
    // Revoked tokens are kept in memory only until their own expiry, after which
    // ValidateLifetime already rejects them. Note: this is per-process, so it does
    // not survive an app restart or work across multiple instances.
    public class TokenBlacklist : ITokenBlacklist
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<TokenBlacklist> _logger;

        public TokenBlacklist(IMemoryCache cache, ILogger<TokenBlacklist> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public void Revoke(string token, DateTime expiresUtc)
        {
            const string method = nameof(Revoke);

            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            var ttl = expiresUtc - DateTime.UtcNow;
            if (ttl <= TimeSpan.Zero)
            {
                // Already expired – nothing to keep around.
                return;
            }

            _cache.Set(CacheKey(token), true, expiresUtc);
            _logger.LogInformation("[TokenBlacklist.{Method}] Token revoked until {ExpiresUtc:o}.", method, expiresUtc);
        }

        public bool IsRevoked(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            return _cache.TryGetValue(CacheKey(token), out _);
        }

        private static string CacheKey(string token) => $"revoked_token:{token}";
    }
}
