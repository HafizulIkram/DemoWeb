using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DemoWeb.Models
{
    public class Movies
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Movie Title")]
        public string Title { get; set; }

        [AllowNull]
        public string? Description { get; set; }

        [Display(Name = "Rating")]
        [Range(1,10)]
        [Required]
        public int MoviesCount { get; set; }

        [Required]
        [Display(Name = "Movie Genre")]
        public string Genre { get; set; }

        [Required]
        [Display(Name = "Movie Type")]
        public string Type { get; set; }

        [Column(TypeName = "decimal(18, 2) ")]
        public decimal? Price { get; set; }

        [DataType(DataType.Date)]
        [Required]
        public DateTime RealeasedDate { get; set; }

        [NotMapped]
        public List<SelectListItem> GenreList { get; set; }

        [NotMapped]
        public List<SelectListItem> TypeList { get; set; }

    }
}