using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Excel.Service
{
    public class LogoutService : ILogoutService
    {
        private readonly ITokenBlacklist _tokenBlacklist;
        private readonly ILogger<LogoutService> _logger;

        public LogoutService(
            ITokenBlacklist tokenBlacklist,
            ILogger<LogoutService> logger)
        {
            _tokenBlacklist = tokenBlacklist;
            _logger = logger;
        }

        public async Task<string?> LogoutAsync(HttpContext httpContext)
        {
            var token = await httpContext.GetTokenAsync(
                JwtBearerDefaults.AuthenticationScheme,
                "access_token");

            // Fallback to Authorization header
            if (string.IsNullOrWhiteSpace(token))
            {
                var authHeader = httpContext.Request.Headers.Authorization.ToString();

                token = authHeader.StartsWith(
                    "Bearer ",
                    StringComparison.OrdinalIgnoreCase)
                    ? authHeader["Bearer ".Length..].Trim()
                    : authHeader.Trim();
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("No token found on the request.");
            }

            DateTime expiresUtc;

            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                expiresUtc = jwt.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not read token expiry during logout.");

                throw new ArgumentException("Invalid token.");
            }

            _tokenBlacklist.Revoke(token, expiresUtc);

            var username = httpContext.User.Identity?.Name;

            _logger.LogInformation(
                "User {Username} logged out.",
                username);

            return username;
        }
    }
}