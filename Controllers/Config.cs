using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ATM_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AuthController(IConfiguration configuration,ILogger<AuthController> logger)
        {
            _logger = logger;
            _configuration = configuration;
            try
            {
                _connectionString =
                    configuration.GetConnectionString("EmployeeDb")
                    ?? throw new ArgumentException("Connection string is required.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[SqlConn.{Method}] Failed loading connection string.",
                    nameof(AuthController));

                throw;
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login( [FromQuery] string username,[FromQuery] string password)
        {
            try
            {
                // Username Validation
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("Username was empty.");

                    return BadRequest(new
                    {
                        Message = "Username is required."
                    });
                }

                // Password Validation
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Password was empty.");

                    return BadRequest(new
                    {
                        Message = "Password is required."
                    });
                }

                username = username.Trim();

                if (username.Length < 5 || username.Length > 50)
                {
                    return BadRequest(new
                    {
                        Message = "Username must be between 5 and 50 characters."
                    });
                }

                if (password.Length < 6)
                {
                    return BadRequest(new
                    {
                        Message = "Password must contain at least 6 characters."
                    });
                }

                _logger.LogInformation("Login attempt for {Username}",username);

                string Hashed_password = PasswordHasher.ComputeHash(password);

                using SqlConnection connection = new SqlConnection(_connectionString);

                await connection.OpenAsync();

                string sql = @"SELECT Username FROM USERS WHERE Username=@Username AND Hashed_password=@Hashed_password";

                using SqlCommand command = new SqlCommand(sql, connection);

                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Hashed_password", Hashed_password);

                using SqlDataReader reader = await command.ExecuteReaderAsync();
                

                if (!await reader.ReadAsync())
                {
                    _logger.LogWarning("Invalid Login : {Username}",username);

                    return Unauthorized(new
                    {
                        Message = "Invalid Username or Password"
                    });
                }

                string dbUsername = reader["Username"].ToString()!;

                List<Claim> claims =
                [
                    new Claim(ClaimTypes.Name, dbUsername)
                ];

                SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                SigningCredentials credentials =
                    new SigningCredentials(
                        key,
                        SecurityAlgorithms.HmacSha256);

                JwtSecurityToken token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(
                        Convert.ToDouble(
                            _configuration["Jwt:ExpireMinutes"])),
                    signingCredentials: credentials);

                string jwt = new JwtSecurityTokenHandler().WriteToken(token);

                _logger.LogInformation( "User {Username} Logged In Successfully.",dbUsername);

                return Ok(new
                {
                    Username = dbUsername,
                    Token = jwt,
                });
            }
            catch (SqlException qlex)
            {
                _logger.LogError(qlex,"Database Error.");

                return StatusCode(500, new
                {
                    Message = "Database Error"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Unexpected Error.");

                return StatusCode(500, new
                {
                    Message = "Internal Server Error"
                });
            }
        }
    }
}