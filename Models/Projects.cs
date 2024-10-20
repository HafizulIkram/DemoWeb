using System.ComponentModel.DataAnnotations;

namespace DemoWeb.Models
{
    public class Projects
    {
        [Key]
        public int ProjectId { get; set; }

        [Required]
        [Display (Name = "Project Name")]
        public string ProjectName { get; set; }

        [Required]
        [Display (Name = "Description")]
        public string ProjectDescription { get; set; }

        [Required]
        [Display (Name = "Start Date")]
        [DataType (DataType.Date)]
        public DateOnly ProjectStartDate { get; set; }

        [Required]
        [Display (Name = "End Date")]
        [DataType (DataType.Date)]
        public DateOnly ProjectEndDate { get; set; }

        [Required]
        [Display (Name = "Project Status")]
        public string ProjectStatus { get; set; }




    }
}
