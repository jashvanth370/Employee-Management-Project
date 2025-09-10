using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using CrudApi.Models;
using CrudApi.ImageAdd;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BCryptLib = BCrypt.Net.BCrypt;

namespace CrudApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IPhotoStorage _storage;
        private readonly IOptions<PhotoStorageOptions> _opt;
        private readonly IConfiguration _config;

        public EmployeesController(IConfiguration config, IPhotoStorage storage, IOptions<PhotoStorageOptions> opt)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _storage = storage;
            _opt = opt;
            _config = config;
        }

        // GET: api/employees
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetEmployees()
        {
            var employees = new List<Employee>();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT e.Id, e.Name, e.MobileNo, e.EmailId, e.DepartmentId, e.PhotoUrl, d.Name AS DeptName
                        FROM Employee e
                        LEFT JOIN Department d ON e.DepartmentId = d.Id";

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(new Employee
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    MobileNo = reader["MobileNo"] as string,
                    EmailId = reader.GetString(3),
                    DepartmentId = reader["DepartmentId"] as Guid?,
                    PhotoUrl = reader["PhotoUrl"] as string,
                    Department = reader["DeptName"] != DBNull.Value ? new Department { Name = reader.GetString(6) } : null
                });
            }

            return Ok(employees);
        }

        // GET: api/employees/{id}
        [HttpGet("{id}")]
        public IActionResult GetEmployee(Guid id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"SELECT e.Id, e.Name, e.MobileNo, e.EmailId, e.DepartmentId, e.PhotoUrl, d.Name AS DeptName
                        FROM Employee e
                        LEFT JOIN Department d ON e.DepartmentId = d.Id
                        WHERE e.Id=@id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var employee = new Employee
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1),
                    MobileNo = reader["MobileNo"] as string,
                    EmailId = reader.GetString(3),
                    DepartmentId = reader["DepartmentId"] as Guid?,
                    PhotoUrl = reader["PhotoUrl"] as string,
                    Department = reader["DeptName"] != DBNull.Value ? new Department { Name = reader.GetString(6) } : null
                };
                return Ok(employee);
            }

            return NotFound();
        }

        // POST: api/employees/register
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult RegisterEmployee([FromBody] CreateEmployeeRequest request)
        {
            var empId = Guid.NewGuid();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Check duplicates
            using var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM Employee WHERE EmailId=@Email OR MobileNo=@Mobile",
                conn);
            checkCmd.Parameters.AddWithValue("@Email", request.EmailId);
            checkCmd.Parameters.AddWithValue("@Mobile", request.MobileNo ?? "");
            var exists = (int)checkCmd.ExecuteScalar() > 0;
            if (exists) return BadRequest("Email or Mobile already exists");

            // Insert
            var sql = @"INSERT INTO Employee(Id, Name, MobileNo, EmailId, Password_Hash, DepartmentId, Created_At, Updated_At)
                        VALUES(@Id,@Name,@Mobile,@Email,@Password,@Dept,@Created,@Updated)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", empId);
            cmd.Parameters.AddWithValue("@Name", request.Name);
            cmd.Parameters.AddWithValue("@Mobile", request.MobileNo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", request.EmailId);
            cmd.Parameters.AddWithValue("@Password", BCryptLib.HashPassword(request.Password));
            cmd.Parameters.AddWithValue("@Dept", request.DepartmentId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Created", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@Updated", DateTime.UtcNow);

            cmd.ExecuteNonQuery();

            return CreatedAtAction(nameof(GetEmployee), new { id = empId }, request);
        }

        // PUT: api/employees/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(Guid id, [FromBody] UpdateEmployeeRequest request)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = @"UPDATE Employee SET Name=@Name, MobileNo=@Mobile, EmailId=@Email, DepartmentId=@Dept, Updated_At=@Updated
                        WHERE Id=@Id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", request.Name);
            cmd.Parameters.AddWithValue("@Mobile", request.MobileNo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", request.EmailId);
            cmd.Parameters.AddWithValue("@Dept", request.DepartmentId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Updated", DateTime.UtcNow);

            var rows = cmd.ExecuteNonQuery();
            return rows == 0 ? NotFound() : NoContent();
        }

        // DELETE: api/employees/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteEmployee(Guid id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("DELETE FROM Employee WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);

            var rows = cmd.ExecuteNonQuery();
            return rows == 0 ? NotFound() : NoContent();
        }

        // PATCH: api/employees/{id}/password
        [HttpPatch("{id}/password")]
        [Authorize]
        public IActionResult UpdatePassword(Guid id, [FromBody] UpdatePasswordRequest request)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("UPDATE Employee SET Password_Hash=@Password, Updated_At=@Updated WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Password", BCryptLib.HashPassword(request.NewPassword));
            cmd.Parameters.AddWithValue("@Updated", DateTime.UtcNow);

            var rows = cmd.ExecuteNonQuery();
            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
