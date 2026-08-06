using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Excel.Service
{
    public class ConfigService : IConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigService> _logger;
        private readonly string _connectionString;

        public ConfigService(
            IConfiguration configuration,
            ILogger<ConfigService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _connectionString =
                configuration.GetConnectionString("EmployeeDb")
                ?? throw new ArgumentException("Connection string is required.");
        }

        // Generate a random 5-character role ID (non-cryptographic – consider using Guid or DB-generated ID)
        string GenerateRoleId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            try
            {
                // --- Input validation (unchanged) ---
                if (string.IsNullOrWhiteSpace(username))
                {
                    return new LoginResult
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Username is required."
                    };
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return new LoginResult
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Password is required."
                    };
                }

                username = username.Trim();

                if (username.Length < 5 || username.Length > 50)
                {
                    return new LoginResult
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Username must be between 5 and 50 characters."
                    };
                }

                if (password.Length < 6)
                {
                    return new LoginResult
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Password must contain at least 6 characters."
                    };
                }

                string hashedPassword = PasswordHasher.ComputeHash(password);

                // --- Database operations ---
                using SqlConnection connection = new(_connectionString);
                await connection.OpenAsync();

                // 1. SELECT query to verify credentials
                string sql = @"SELECT Username FROM USERS WHERE Username=@Username AND Hashed_password=@Hashed_password";

                using SqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Hashed_password", hashedPassword);

                // FIX 1: Scope the reader so it is closed/disposed BEFORE any other command on the same connection.
                string dbUsername;
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return new LoginResult
                        {
                            Success = false,
                            StatusCode = 401,
                            Message = "Invalid Username or Password"
                        };
                    }
                    dbUsername = reader["Username"].ToString()!;
                } // reader is closed and disposed here – connection is now free for other commands

                // 2. INSERT a new role (flaw: creates a new row on every login – consider checking existence first)
                string[] roles = { "Admin", "Guest User", "User" };
                string assignedRole = roles[new Random().Next(roles.Length)];
                string roleId = GenerateRoleId();

                string insertSql = @"INSERT INTO Roles (RoleId, RoleName) VALUES (@RoleId, @RoleName)";
                using (SqlCommand insertCmd = new SqlCommand(insertSql, connection))
                {
                    insertCmd.Parameters.AddWithValue("@RoleId", roleId);
                    insertCmd.Parameters.AddWithValue("@RoleName", assignedRole);
                    insertCmd.ExecuteNonQuery(); // Now safe – reader is already closed
                }

                // --- Build JWT token (unchanged) ---
                List<Claim> claims =
                [
                    new Claim(ClaimTypes.Name, dbUsername),
                    new Claim(ClaimTypes.Role, assignedRole)
                ];

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                JwtSecurityToken token = new(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials);

                string jwt = new JwtSecurityTokenHandler().WriteToken(token);

                return new LoginResult
                {
                    Success = true,
                    StatusCode = 200,
                    Username = dbUsername,
                    Token = jwt
                };
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database Error");
                return new LoginResult
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Database Error"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected Error");
                return new LoginResult
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Internal Server Error"
                };
            }
        }
    }
}