using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DemoWeb.Models
{
    public class Employees
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [Display (Name = "Name")]
        public string EmployeeName { get; set; }

        [Required]
        [Display (Name = "Address")]
        public string EmployeeAddress { get; set; }

        [Required]
        [EmailAddress]
        [Display (Name = "Email")]
        public string EmployeeEmail { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string EmployeeRole { get; set; }

        [NotMapped]
        [AllowNull]
        public List<SelectListItem> RoleList { get; set; }

        [Required]
        [DataType (DataType.Date)]
        [Display(Name ="Joined Date")]
        public DateOnly EmployeeJoinedDate { get; set; }

        // For UI developers
        [Display(Name = "UI Framework")]
        [AllowNull]
        public string? UiFramework { get; set; }  // e.g., Angular, React

        [Display(Name = "Design Tools")]
        [AllowNull]
        public string? DesignTools { get; set; }  // e.g., Figma, Sketch, Adobe XD

        // For Backend developers
        [Display(Name = "Backend Language")]
        [AllowNull]
        public string? BackendLanguage { get; set; }  // e.g., C#, Java, Python

        [Display(Name = "Database Technology")]
        [AllowNull]
        public string? DatabaseTechnology { get; set; }  // e.g., SQL

        [Display(Name = "Fullstack Level (Junior/Senior)")]
        [AllowNull]
        public string? FullstackLevel { get; set; }


        public List<EmployeeTask>? EmployeeTasks { get; set; }

       












    }
}
