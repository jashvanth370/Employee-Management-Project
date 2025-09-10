using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using CrudApi.Models;

namespace CrudApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly string _connectionString;

        public DepartmentsController(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetDepartments()
        {
            var departments = new List<Department>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("SELECT Id, Name FROM Department", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                departments.Add(new Department
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1)
                });
            }

            return Ok(departments);
        }

        [HttpGet("{id}")]
        public IActionResult GetDepartment(Guid id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("SELECT Id, Name FROM Department WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return Ok(new Department
                {
                    Id = reader.GetGuid(0),
                    Name = reader.GetString(1)
                });
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult PostDepartment([FromBody] Department dep)
        {
            dep.Id = Guid.NewGuid();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("INSERT INTO Department(Id, Name) VALUES(@id, @name)", conn);
            cmd.Parameters.AddWithValue("@id", dep.Id);
            cmd.Parameters.AddWithValue("@name", dep.Name);
            cmd.ExecuteNonQuery();

            return CreatedAtAction(nameof(GetDepartment), new { id = dep.Id }, dep);
        }

        [HttpPut("{id}")]
        public IActionResult PutDepartment(Guid id, [FromBody] Department dep)
        {
            if (id != dep.Id) return BadRequest();

            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("UPDATE Department SET Name=@name WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", dep.Name);

            var rows = cmd.ExecuteNonQuery();
            return rows == 0 ? NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteDepartment(Guid id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("DELETE FROM Department WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            var rows = cmd.ExecuteNonQuery();
            return rows == 0 ? NotFound() : NoContent();
        }
    }
}
