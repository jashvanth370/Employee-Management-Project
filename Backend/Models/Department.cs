using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CrudApi.Models
{
    public class Department
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [JsonIgnore]
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}


