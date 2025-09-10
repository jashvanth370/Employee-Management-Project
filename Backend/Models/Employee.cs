using CrudApi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    // make optional if you don’t want fake numbers
    [Column("MobileNo")]
    public string? MobileNo { get; set; }

    [Required]
    [EmailAddress]
    public string EmailId { get; set; } = string.Empty;

    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? PhotoFileName { get; set; }
    public string? PhotoUrl { get; set; }
    public string? PhotoContentType { get; set; }

    // 🔥 new field for Google auth
    public string? GoogleId { get; set; }
}
