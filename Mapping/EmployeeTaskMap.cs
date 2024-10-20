using DemoWeb.Entity;
using FluentNHibernate.Mapping;

namespace DemoWeb.Mapping
{
    public class EmployeeTaskMap: ClassMap<EmployeeTaskEntity>
    {
        public EmployeeTaskMap()
        {
            Table("EmployeeTask"); // The name of the table in the database
            DynamicInsert();

            DynamicUpdate();

            Id(x => x.EmployeeTaskId).GeneratedBy.Identity(); // Primary key with auto-increment
            References(x => x.Employee).Column("EmployeeId").PropertyRef(e => e.EmployeeId).ReadOnly().LazyLoad();  // Yes, that's all.
            References(x => x.Task).Column("TaskId").PropertyRef(t => t.TaskId).ReadOnly().LazyLoad();  // Yes, that's all.
            Map(x => x.AssignDate); // 'HireDate' column
        }
    }
}
