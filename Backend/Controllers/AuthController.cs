using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using CrudApi.Models;
using Google.Apis.Auth;
using BCryptLib = BCrypt.Net.BCrypt;

namespace CrudApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _config = config;
        }

        // POST: api/auth/login
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT Id, Name, EmailId, MobileNo, Password_Hash FROM Employee WHERE EmailId=@Input OR MobileNo=@Input";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Input", request.EmailOrMobile.Trim());

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return Unauthorized("Invalid credentials");

            var storedPassword = reader["Password_Hash"] as string;
            if (string.IsNullOrEmpty(storedPassword) || !BCryptLib.Verify(request.Password, storedPassword))
                return Unauthorized("Invalid credentials");

            var employee = new Employee
            {
                Id = (Guid)reader["Id"],
                Name = reader["Name"] as string,
                EmailId = reader["EmailId"] as string
            };

            var token = GenerateJwtToken(employee);
            return Ok(new { token, userId = employee.Id, email = employee.EmailId, name = employee.Name });
        }

        // POST: api/auth/google
        [AllowAnonymous]
        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
                return BadRequest(new { message = "Token is required" });

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(request.Token,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _config["GoogleAuth:ClientId"] }
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid Google token", detail = ex.Message });
            }

            var email = payload.Email ?? $"unknown_{Guid.NewGuid()}@example.com";
            var name = payload.Name ?? "Unknown";

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Check if employee exists
            var selectSql = "SELECT Id, Name, EmailId, GoogleId FROM Employee WHERE EmailId=@Email OR GoogleId=@GoogleId";
            using var selectCmd = new SqlCommand(selectSql, conn);
            selectCmd.Parameters.AddWithValue("@Email", email);
            selectCmd.Parameters.AddWithValue("@GoogleId", payload.Subject ?? "");

            Employee employee = null;
            using (var reader = selectCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    employee = new Employee
                    {
                        Id = (Guid)reader["Id"],
                        Name = reader["Name"] as string,
                        EmailId = reader["EmailId"] as string,
                        GoogleId = reader["GoogleId"] as string
                    };
                }
            }

            if (employee == null)
            {
                // Create new employee
                var newId = Guid.NewGuid();
                var insertSql = @"INSERT INTO Employee(Id, Name, EmailId, GoogleId, Created_At, Updated_At)
                                  VALUES(@Id,@Name,@Email,@GoogleId,@Created,@Updated)";

                using var insertCmd = new SqlCommand(insertSql, conn);
                insertCmd.Parameters.AddWithValue("@Id", newId);
                insertCmd.Parameters.AddWithValue("@Name", name);
                insertCmd.Parameters.AddWithValue("@Email", email);
                insertCmd.Parameters.AddWithValue("@GoogleId", payload.Subject ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Created", DateTime.UtcNow);
                insertCmd.Parameters.AddWithValue("@Updated", DateTime.UtcNow);

                insertCmd.ExecuteNonQuery();

                employee = new Employee
                {
                    Id = newId,
                    Name = name,
                    EmailId = email,
                    GoogleId = payload.Subject
                };
            }
            else if (string.IsNullOrEmpty(employee.GoogleId) && !string.IsNullOrEmpty(payload.Subject))
            {
                // Update GoogleId
                var updateSql = "UPDATE Employee SET GoogleId=@GoogleId, Updated_At=@Updated WHERE Id=@Id";
                using var updateCmd = new SqlCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@GoogleId", payload.Subject);
                updateCmd.Parameters.AddWithValue("@Updated", DateTime.UtcNow);
                updateCmd.Parameters.AddWithValue("@Id", employee.Id);
                updateCmd.ExecuteNonQuery();
            }

            var jwt = GenerateJwtToken(employee);
            return Ok(new { token = jwt, userId = employee.Id, email = employee.EmailId, name = employee.Name });
        }

        // Microsoft login would follow similar pattern with token validation via ADO.NET
        // POST: api/auth/microsoft
        [AllowAnonymous]
        [HttpPost("microsoft")]
        public async Task<IActionResult> MicrosoftLogin([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
                return BadRequest(new { message = "Microsoft token is required" });

            try
            {
                // Read Microsoft ID token
                var handler = new JwtSecurityTokenHandler();
                var msToken = handler.ReadJwtToken(request.Token);

                // Extract claims (email, name, etc.)
                var email = msToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value
                            ?? msToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
                var name = msToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

                if (string.IsNullOrEmpty(email))
                    return BadRequest(new { message = "Invalid Microsoft token - email not found" });

                Employee employee = null;

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                // Check if employee exists
                var selectSql = "SELECT Id, Name, EmailId FROM Employee WHERE EmailId=@Email";
                using (var cmd = new SqlCommand(selectSql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        employee = new Employee
                        {
                            Id = (Guid)reader["Id"],
                            Name = reader["Name"] as string,
                            EmailId = reader["EmailId"] as string
                        };
                    }
                }

                // If not exists, insert new employee
                if (employee == null)
                {
                    var newId = Guid.NewGuid();
                    var insertSql = @"INSERT INTO Employee (Id, Name, EmailId, Password_Hash, Created_At, Updated_At)
                              VALUES (@Id, @Name, @Email, @PasswordHash, @Created, @Updated)";

                    using var insertCmd = new SqlCommand(insertSql, conn);
                    insertCmd.Parameters.AddWithValue("@Id", newId);
                    insertCmd.Parameters.AddWithValue("@Name", name ?? email.Split('@')[0]);
                    insertCmd.Parameters.AddWithValue("@Email", email);
                    insertCmd.Parameters.AddWithValue("@PasswordHash", BCryptLib.HashPassword(Guid.NewGuid().ToString())); // random password
                    insertCmd.Parameters.AddWithValue("@Created", DateTime.UtcNow);
                    insertCmd.Parameters.AddWithValue("@Updated", DateTime.UtcNow);

                    await insertCmd.ExecuteNonQueryAsync();

                    employee = new Employee
                    {
                        Id = newId,
                        Name = name ?? email.Split('@')[0],
                        EmailId = email
                    };
                }

                // Generate JWT
                var jwt = GenerateJwtToken(employee);
                return Ok(new { token = jwt, userId = employee.Id, email = employee.EmailId, name = employee.Name });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Microsoft Login Error: " + ex.Message);
                return BadRequest(new { message = "Invalid Microsoft token", detail = ex.Message });
            }
        }



        private string GenerateJwtToken(Employee employee)
        {
            var jwtKey = _config["Jwt:Key"] ?? "dev_super_secret_key_change_me";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, employee.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, employee.EmailId ?? ""),
                new Claim("name", employee.Name ?? "")
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Request DTOs
    public class LoginRequest
    {
        public string EmailOrMobile { get; set; }
        public string Password { get; set; }
    }

    public class TokenRequest
    {
        public string Token { get; set; }
    }
}
