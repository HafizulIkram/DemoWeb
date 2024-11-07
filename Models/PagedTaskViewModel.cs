namespace DemoWeb.Models
{
    public class PagedTaskViewModel
    {
        public List<EmployeeTask.Tasks> EmployeeTasks { get; set; } = new List<EmployeeTask.Tasks>();
        public List<Tasks> Tasks { get; set; } = new List<Tasks>();
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
