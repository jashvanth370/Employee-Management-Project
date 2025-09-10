using System.ComponentModel.DataAnnotations;

namespace CrudApi.Models
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public string? PhotoUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DepartmentDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }


    public class CreateEmployeeRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string EmailId { get; set; } = string.Empty;
        
        [Required]
        public string MobileNo { get; set; } = string.Empty;
        
        public Guid? DepartmentId { get; set; }
        
        [Required]
        [MinLength(4)]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string? PhotoUrl { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string EmailId { get; set; } = string.Empty;
        
        [Required]
        public string MobileNo { get; set; } = string.Empty;
        
        public Guid? DepartmentId { get; set; }
        [Required]
        public string? PhotoUrl { get; set; }

    }

    public class UpdatePasswordRequest
    {
        [Required]
        [MinLength(4)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        public string EmailOrMobile { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
