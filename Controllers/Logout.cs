/*using System.IdentityModel.Tokens.Jwt;
using Excel.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Excel.Controllers
{
    [ApiController]

    [Route("api/Log Out")]
    [AllowAnonymous]
    public class LogoutController : ControllerBase
    {
        private readonly ITokenBlacklist _tokenBlacklist;
        private readonly ILogger<LogoutController> _logger;

        public LogoutController(ITokenBlacklist tokenBlacklist, ILogger<LogoutController> logger)
        {
            _tokenBlacklist = tokenBlacklist;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult Logout()
        {
            const string method = nameof(Logout);

            // The JwtBearer handler stored the raw token when it authenticated this request.
            var token = HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token").Result;

            if (string.IsNullOrWhiteSpace(token))
            {
                // Fallback: read it straight off the Authorization header.
                var authHeader = Request.Headers.Authorization.ToString();
                token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    ? authHeader["Bearer ".Length..].Trim()
                    : authHeader.Trim();
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { Message = "No token found on the request." });
            }

            DateTime expiresUtc;
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                expiresUtc = jwt.ValidTo; // already UTC
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[LogoutController.{Method}] Could not read token expiry.", method);
                return BadRequest(new { Message = "Invalid token." });
            }

            _tokenBlacklist.Revoke(token, expiresUtc);

            var username = User.Identity?.Name;
            _logger.LogInformation("[LogoutController.{Method}] User {Username} logged out.", method, username);

            return Ok(new { Message = "Logged out successfully." });
        }
    }
}
*/