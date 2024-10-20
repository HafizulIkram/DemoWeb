using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoWeb.Models
{
    public class EmployeeTask
    {
        [Key]
        public int EmploTaskId { get; set; }  // Primary Key

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }  // Foreign key to Employee

        [ForeignKey("Task")]
        public int TaskId { get; set; }      // Foreign key to Task

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Joined Date")]
        public DateTime AssignDate { get; set; }


        // Properties for the dropdown lists
        public List<SelectListItem> Employees { get; set; }  // List for Employees dropdown
        public List<SelectListItem> Tasks { get; set; }

    }
}
