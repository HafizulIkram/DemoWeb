using System.ComponentModel.DataAnnotations;

namespace DemoWeb.Models
{
    public class Login
    {


        [Required]
        public virtual string EmployeeEmail { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W]).+$", ErrorMessage = "Password must contain at least one capital letter and one special character.")]
        public virtual string Password { get; set; }
    }
}
