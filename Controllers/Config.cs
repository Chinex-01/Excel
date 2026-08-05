using Excel.Service;
using Microsoft.AspNetCore.Mvc;

namespace ATM_API.Controllers
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
        public async Task<IActionResult> Login(
            [FromQuery] string username,
            [FromQuery] string password)
        {
            var result = await _configService.LoginAsync(username, password);

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