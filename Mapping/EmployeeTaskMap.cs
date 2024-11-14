using DemoWeb.Entity;
using FluentNHibernate.Mapping;

namespace DemoWeb.Mapping
{
    public class EmployeeTaskMap: ClassMap<EmployeeTaskEntity>
    {
        public EmployeeTaskMap()
        {
            Table("EmployeeTask"); 
            DynamicInsert();

            DynamicUpdate();

            Id(x => x.EmployeeTaskId).GeneratedBy.Identity(); 
            References(x => x.Employee).Column("EmployeeId").LazyLoad();  
            References(x => x.Task).Column("TaskId").LazyLoad();  
            Map(x => x.TaskStatus);
            Map(x => x.AssignDate); 
            Map(x => x.DueDate);
            Map(x => x.FinishedDate);
        }
    }
}
