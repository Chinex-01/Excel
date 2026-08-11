using Excel.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Excel.Controllers
{
    [ApiController]
    [Route("api/Logout")]
    [AllowAnonymous]
    public class LogoutController : ControllerBase
    {
        private readonly ILogoutService _logoutService;

        public LogoutController(ILogoutService logoutService)
        {
            _logoutService = logoutService;
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var username = await _logoutService.LogoutAsync(HttpContext);

                return Ok(new
                {
                    Message = "Logged out successfully.",
                    Username = username
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
        }
    }
}