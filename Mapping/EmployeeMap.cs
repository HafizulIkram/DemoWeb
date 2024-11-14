using FluentNHibernate.Mapping;
using DemoWeb.Entity;

public class EmployeeMap : ClassMap<EmployeeEntity>
{
	public EmployeeMap()
	{
		Table("Employee"); // The name of the table in the database
        DynamicInsert();

        DynamicUpdate();

        Id(x => x.EmployeeId).GeneratedBy.Identity(); 
		Map(x => x.EmployeeName); 
		Map(x => x.EmployeeAddress); 
		Map(x => x.EmployeePosition);
		Map(x => x.DateJoined); 
		Map(x => x.isActive); 
		Map(x => x.Password); 
		Map(x => x.EmployeeEmail); 
        HasMany(x => x.EmployeeTasks)  
            .Cascade.All()
            .Inverse()
            .KeyColumn("EmployeeId");
    }
}
