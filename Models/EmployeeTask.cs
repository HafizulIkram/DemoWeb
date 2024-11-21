using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DemoWeb.Models
{
    public class EmployeeTask
    {
        [Key]
        public int EmployeeTaskId { get; set; }  // Primary Key

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }  // Foreign key to Employee


        [ForeignKey("Employee")]
        public int AssignedBy { get; set; }  // Foreign key to Employee

        [ForeignKey("Task")]
        public int TaskId { get; set; }      // Foreign key to Task

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Assign Date")]
        public DateTime AssignDate { get; set; }

        [ValidateNever]
        [Display(Name = "Status")]
        public string TaskStatus { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [AllowNull]
        [DataType(DataType.Date)]
        [Display(Name = "Finished Date")]
        public DateTime FinishedDate { get; set; }




        // Properties for the dropdown lists
        [ValidateNever]
        public List<SelectListItem> EmployeesList { get; set; }  // List for Employees dropdown

        [ValidateNever]
        public List<Tasks> TaskList { get; set; }

 
        public List<int> TaskListId { get; set; }


        public bool IsSelected { get; set; }

        [ValidateNever]
        public Tasks tasks { get; set; }

        [ValidateNever]
        public Employee employee { get; set; }

        // class
        public class Tasks
        {
            public int TaskId { get; set; }
            public string TaskTitle { get; set; }
            public string TaskStatus { get; set; }
            public string TaskPriority { get; set; }
            public string TaskDescription { get; set; }

        }

        public class Employee
        {
            public int EmployeeId { get; set; }
            public string EmployeeName { get; set; }
        }
    }
}
