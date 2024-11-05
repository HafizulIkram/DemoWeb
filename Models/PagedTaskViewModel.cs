namespace DemoWeb.Models
{
    public class PagedTaskViewModel
    {
        public List<EmployeeTask.Tasks> Tasks { get; set; } = new List<EmployeeTask.Tasks>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
