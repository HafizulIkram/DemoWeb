using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DemoWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DemoWeb.Data
{
    public class DemoWebContext : IdentityDbContext<IdentityUser>
    {
        public DemoWebContext (DbContextOptions<DemoWebContext> options)
            : base(options)
        {
        }

      

     

        public DbSet<DemoWeb.Models.EmployeeTask> EmployeeTasks { get; set; } = default!;

        public DbSet<DemoWeb.Models.Tasks> Tasks { get; set; } = default!;

    }
}
