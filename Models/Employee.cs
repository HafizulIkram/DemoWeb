using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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
        [Display(Name = "Employee Name")]
		public string EmployeeName { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string EmployeeAddress { get; set; }

        [Required]
        [Display(Name = "Position")]
        public string EmployeePosition { get; set; }

        [Display(Name = "Email")]
		[DataType(DataType.EmailAddress)]
        public string EmployeeEmail { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W]).+$", ErrorMessage = "Password must contain at least one capital letter and one special character.")]
        public  string Password { get; set; }

        [Required]
        [Display(Name = "Confirm Password")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[\W]).+$", ErrorMessage = "Password must contain at least one capital letter and one special character.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DefaultValue(true)]
        public  bool isActive { get; set; }

		[Required]
        [Display(Name = "Date Joined")]
		[DataType(DataType.Date)]
        public  DateTime DateJoined { get; set; }

        [AllowNull]
        public  List<SelectListItem> PositionList { get; set; }

        [AllowNull]
        public int finishTaskCount { get; set; }

        [AllowNull]
        public int pendingTaskCount { get; set; }

        [AllowNull]
        public int incompleteTaskCount { get; set; }
    }

}
