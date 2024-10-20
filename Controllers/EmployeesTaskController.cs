using DemoWeb.Data;
using DemoWeb.DTO;
using DemoWeb.Entity;
using DemoWeb.Migrations;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NHibernate.Transform;

namespace DemoWeb.Controllers
{
    public class EmployeesTaskController : Controller
    {

        private readonly NHibernateHelper _nhibernateHelper;

        public EmployeesTaskController(NHibernateHelper nHibernateHelper)
        {
            _nhibernateHelper = nHibernateHelper;
        }

        public async Task<IActionResult> Index()
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                EmployeeTaskEntity employeeTaskAlias = null;  // Alias for EmployeeTaskEntity
                EmployeeEntity employeeAlias = null;         // Alias for EmployeeEntity
                TaskEntity taskAlias = null;                 // Alias for TaskEntity
                EmployeeTaskResult resultAlias = null;       // Alias for the result DTO

                // Asynchronous QueryOver to perform join between EmployeeTask, Employee, and Task
                var query = await session.QueryOver(() => employeeTaskAlias)
                    .JoinQueryOver(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                    .JoinQueryOver(() => employeeTaskAlias.Task, () => taskAlias)         // Join with Task
                    .SelectList(list => list
                        .Select(() => employeeAlias.EmployeeName).WithAlias(() => resultAlias.EmployeeName)  // Select EmployeeName
                        .Select(() => taskAlias.TaskTitle).WithAlias(() => resultAlias.TaskName)              // Select TaskTitle
                        .Select(() => employeeTaskAlias.AssignDate).WithAlias(() => resultAlias.DateAssigned)  // Select AssignDate
                    )
                    .TransformUsing(Transformers.AliasToBean<EmployeeTaskResult>())  // Map results to DTO
                    .ListAsync<EmployeeTaskResult>();  // Asynchronously fetch the list

                return View(query);
            }
        }

        public async Task<IActionResult> Create()
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // Retrieve Employees and Tasks to populate the dropdowns
                var employees = await session.QueryOver<EmployeeEntity>().ListAsync();

                var tasks = await session.QueryOver<TaskEntity>().ListAsync();

               
                var employeeSelectList = employees.Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.EmployeeName
                }).ToList();

                var taskSelectList = tasks.Select(t => new SelectListItem
                {
                    Value = t.TaskId.ToString(),
                    Text = t.TaskTitle
                }).ToList();

            
                var EmployeeTaskModel = new EmployeeTask
                {
                    Employees = employeeSelectList,
                    Tasks = taskSelectList
                };

                return View(EmployeeTaskModel); 
            }
        }



        [HttpPost]
        public async Task<IActionResult> Create(EmployeeTask EmployeeTaskModel)
        {
            ModelState.Remove("Tasks");
            ModelState.Remove("Employees");

            if (!ModelState.IsValid)
            {
                return View(EmployeeTaskModel);  // If the form is not valid, return the view with the model
            }

            using (var session = _nhibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    // Create a new EmployeeTaskEntity
                    var EmployeeTaskEntity = new EmployeeTaskEntity
                    {
                        Employee = session.Get<EmployeeEntity>(EmployeeTaskModel.EmployeeId),  // Fetch the Employee by ID
                        Task = session.Get<TaskEntity>(EmployeeTaskModel.TaskId),              // Fetch the Task by ID
                        AssignDate = EmployeeTaskModel.AssignDate                          // Set the assignment date
                    };

                    // Save the EmployeeTaskEntity to the database
                    await session.SaveAsync(EmployeeTaskEntity);
                    await transaction.CommitAsync();

                    return RedirectToAction("Index");  // Redirect to the Index action
                }
            }
        }


       /* [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Create(EmployeeTask EmployeeTask)
         {
             if (ModelState.IsValid)
             {
                 _context.Add(EmployeeTask);
                 await _context.SaveChangesAsync();
                 return Json(new { success = true, message = "Employee created successfully!" });
             }

             return View();
         }

         public async Task<IActionResult> Edit(int? id)
         {
             if (id == null)
             {
                 return NotFound();
             }

             var employeeTask = await _context.EmployeeTasks
             .Where(e => e.EmploTaskId == id).Include(e => e.Employee).
                                                             Include(t => t.Task)

     .FirstOrDefaultAsync();



             if (employeeTask == null)
             {
                 return NotFound();
             }

             // Populate the Task.StatusList for the dropdown
             employeeTask.Task.StatusList = new List<SelectListItem>
     {
         new SelectListItem { Text = "In-Progress", Value = "In-Progress" },
         new SelectListItem { Text = "Complete", Value = "Complete" }
     };

             return View(employeeTask);
         }




         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(int id, EmployeeTask employeeTask)
         {
             if (id != employeeTask.EmploTaskId)
             {
                 return NotFound();
             }

             ModelState.Remove("Task.StatusList");
             ModelState.Remove("Employee.RoleList");
             if (ModelState.IsValid)
             {

                 try
                 {

                     _context.Update(employeeTask);
                     await _context.SaveChangesAsync();
                 }
                 catch (DbUpdateConcurrencyException)
                 {
                     if (!EmployeesTaksExists(employeeTask.EmploTaskId))
                     {
                         return NotFound();
                     }
                     else
                     {
                         throw;
                     }
                 }
                 return RedirectToAction(nameof(Index));
             }



             return View();
         }

         public async Task<IActionResult> Delete(int? id)
         {


             if (id == null)
             {
                 return NotFound();
             }

             var employeeTask = await _context.EmployeeTasks.Include(e => e.Employee).
                                                             Include(t => t.Task)
                 .FirstOrDefaultAsync(e => e.EmploTaskId == id);
             if (employeeTask == null)
             {

                 return NotFound();
             }

             return View(employeeTask);
         }


         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Delete(int id)
         {
             var employeeTasks = await _context.EmployeeTasks.FindAsync(id);
             if (employeeTasks != null)
             {
                 _context.EmployeeTasks.Remove(employeeTasks);
             }

             await _context.SaveChangesAsync();
             return RedirectToAction(nameof(Index));
         }

         private bool EmployeesTaksExists(int id)
         {
             return _context.EmployeeTasks.Any(e => e.EmploTaskId == id);
         }
 */

    }
}
