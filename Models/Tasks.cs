using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DemoWeb.Models
{
    public class Tasks
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        [Display(Name ="Title")]
        public string TaskTitle { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string TaskDescription { get; set; }

        [Required]
        [Display(Name = "Priority")]
        public string TaskPriority { get; set; }

     

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }


        [AllowNull]
    
        public List<SelectListItem> PriorityList { get; set; }



    }
}
