using System.ComponentModel.DataAnnotations;

namespace DemoWeb.Entity
{
    public class TaskEntity
    {
        
        public virtual int TaskId { get; set; }

        
        public virtual string TaskTitle { get; set; }

        public virtual string TaskDescription { get; set; }

        
       

       
        public virtual string TaskPriority { get; set; }

        
        public virtual DateTime DueDate { get; set; }

      
        public virtual DateTime CreatedAt { get; set; }

        public virtual IList<EmployeeTaskEntity> EmployeeTasks { get; set; } = new List<EmployeeTaskEntity>();
    }
}
