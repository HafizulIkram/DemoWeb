using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DemoWeb.Models
{
	public class Employee
	{
		[Key]
		public virtual int EmployeeId { get; set; }

		[Required]
		public virtual string EmployeeName { get; set; }

        [Required]
        public virtual string EmployeeAddress { get; set; }

        [Required]
		public virtual string EmployeePosition { get; set; }

		[Required]
		public virtual string EmployeeEmail { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W]).+$", ErrorMessage = "Password must contain at least one capital letter and one special character.")]
        public virtual string Password { get; set; }

		[Required]
        [DefaultValue(true)]
        public virtual bool isActive { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public virtual DateTime DateJoined { get; set; }

        [AllowNull]
        public virtual List<SelectListItem> PositionList { get; set; }

    }

}
