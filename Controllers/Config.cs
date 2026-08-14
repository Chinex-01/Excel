using Excel.Service;
using Microsoft.AspNetCore.Mvc;

namespace Excel
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfigService _configService;

        public AuthController(IConfigService configService)
        {
            _configService = configService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var username = request.Username;
            var password = request.Password;
            var requestId = request.RequestId;

            var result = await _configService.LoginAsync(username, password, requestId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new
                {
                    Message = result.Message
                });
            }

            return Ok(new
            {
                Username = result.Username,
                Token = result.Token
            });
        }
    }
}