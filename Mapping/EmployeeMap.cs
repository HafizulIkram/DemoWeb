using FluentNHibernate.Mapping;
using DemoWeb.Entity;

public class EmployeeMap : ClassMap<EmployeeEntity>
{
	public EmployeeMap()
	{
		Table("Employee"); // The name of the table in the database
        DynamicInsert();

        DynamicUpdate();

        Id(x => x.EmployeeId).GeneratedBy.Identity(); // Primary key with auto-increment
		Map(x => x.EmployeeName); // 'Name' column
		Map(x => x.EmployeeAddress); // 'Role' column
		Map(x => x.EmployeePosition); // 'HireDate' column
		Map(x => x.DateJoined); // 'HireDate' column
		Map(x => x.isActive); // 'HireDate' column
		Map(x => x.Password); // 'HireDate' column
		Map(x => x.EmployeeEmail); // 'HireDate' column

        HasMany(x => x.EmployeeTasks)  
            .Cascade.All()
            .Inverse()
            .KeyColumn("EmployeeId");
    }
}
