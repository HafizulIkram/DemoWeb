using DemoWeb.Entity;
using FluentNHibernate.Mapping;
namespace DemoWeb.Mapping
{
    public class TasksMap : ClassMap<TaskEntity>
    {

        public TasksMap()
        {
            Table("Tasks"); // The name of the table in the database
            DynamicInsert();

            DynamicUpdate();

            Id(x => x.TaskId).GeneratedBy.Identity(); // Primary key with auto-increment
            Map(x => x.TaskTitle).Not.Nullable(); 
            Map(x => x.TaskDescription).Not.Nullable(); 
            Map(x => x.TaskStatus).Not.Nullable();
            Map(x => x.TaskPriority).Not.Nullable(); 
            Map(x => x.DueDate);
            Map(x => x.CreatedAt).Not.Nullable();

            HasMany(x => x.EmployeeTasks)
            .Cascade.All()
            .Inverse()
            .KeyColumn("TaskId");

        }
    }
}
