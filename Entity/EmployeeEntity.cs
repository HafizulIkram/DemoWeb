using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations;

namespace DemoWeb.Entity
{
    public class EmployeeEntity
    {
       
        public virtual int EmployeeId { get; set; }

       
        public virtual string EmployeeName { get; set; }

      
        public virtual string EmployeeAddress { get; set; }

        public virtual string EmployeePosition { get; set; }

        [DataType(DataType.EmailAddress)]
		public virtual string EmployeeEmail { get; set; }



		public virtual string Password { get; set; }

   
        public virtual bool isActive { get; set; }

        public virtual DateTime DateJoined { get; set; }

        public virtual IList<EmployeeTaskEntity> EmployeeTasks { get; set; } = new List<EmployeeTaskEntity>();
        
    }
}
