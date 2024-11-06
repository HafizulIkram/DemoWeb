using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoWeb.Models
{
    public class EmployeeTask
    {
        [Key]
        public int EmployeeTaskId { get; set; }  // Primary Key

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }  // Foreign key to Employee

        [ForeignKey("Task")]
        public int TaskId { get; set; }      // Foreign key to Task

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Assign Date")]
        public DateTime AssignDate { get; set; }

        [Required]
        public string TaskStatus { get; set; }


        // Properties for the dropdown lists
        public List<SelectListItem> EmployeesList { get; set; }  // List for Employees dropdown
        public List<Tasks> TaskList { get; set; }
        public List<int> TaskListId { get; set; }


        public bool IsSelected { get; set; }

        public Tasks tasks { get; set; }

        public Employee employee { get; set; }

        // class
        public class Tasks
        {
            public int TaskId { get; set; }
            public string TaskTitle { get; set; }
            public string TaskStatus { get; set; }
            public string TaskPriority { get; set; }
            public string TaskDescription { get; set; }
            public DateTime DueDate { get; set; }

        }

        public class Employee
        {
            public int EmployeeId { get; set; }
            public string EmployeeName { get; set; }
        }
    }
}
