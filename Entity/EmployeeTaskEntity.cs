using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemoWeb.Entity
{
    public class EmployeeTaskEntity
    {

        public virtual int EmployeeTaskId { get; set; }


        public virtual EmployeeEntity Employee { get; set; }

        public virtual TaskEntity Task { get; set; }

        public virtual DateTime AssignDate { get; set; }

        public virtual string TaskStatus { get; set; }
    }
}
