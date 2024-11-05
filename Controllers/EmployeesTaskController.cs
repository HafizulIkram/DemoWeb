using DemoWeb.Data;
using DemoWeb.Entity;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NHibernate.Transform;
using Microsoft.AspNetCore.Authorization;


namespace DemoWeb.Controllers
{

    [Authorize] // Restrict access to authenticated users
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
                EmployeeTaskEntity employeeTaskAlias = null;
                EmployeeEntity employeeAlias = null;
                TaskEntity taskAlias = null;

                // Get the currently logged-in user's EmployeeId from claims
                var employeeIdClaim = User.FindFirst("EmployeeId");
                if (employeeIdClaim == null)
                {
                    return Unauthorized();
                }

                int employeeId = int.Parse(employeeIdClaim.Value);

                // Query to fetch EmployeeTask entities and join with Employee and Task
                var employeeTaskEntities = await session.QueryOver(() => employeeTaskAlias)
                    .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias)
                    .JoinAlias(() => employeeTaskAlias.Task, () => taskAlias)
                    .Where(() => employeeAlias.EmployeeId == employeeId)
                    .ListAsync<EmployeeTaskEntity>();

                // Convert entities to models
                var employeeTasks = employeeTaskEntities.Select(entity => new EmployeeTask
                {
                    EmployeeTaskId = entity.EmployeeTaskId,
                    EmployeeId = entity.Employee.EmployeeId,
                    TaskId = entity.Task.TaskId,
                    TaskStatus = entity.TaskStatus,
                    AssignDate = entity.AssignDate,

                    tasks = new EmployeeTask.Tasks
                    {
                        TaskTitle = entity.Task.TaskTitle,
                        TaskPriority = entity.Task.TaskPriority,

                    },

                    employee = new EmployeeTask.Employee
                    {
                        EmployeeName = entity.Employee.EmployeeName,
                    }
                });

                return View(employeeTasks);
            }
        }

        // GET: Create
        [Authorize(Roles = "Team Leader")] // Only allow TeamLeader role for Create GET
        public async Task<IActionResult> Create()
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // Retrieve Employees and Tasks to populate the dropdowns
                var employeesEntity = await session.QueryOver<EmployeeEntity>().ListAsync();
                var tasksEntity = await session.QueryOver<TaskEntity>().ListAsync();

                var employeeSelectList = employeesEntity.Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.EmployeeName
                }).ToList();

                // Convert tasksEntity to List<EmployeeTask.Tasks>
                var tasksList = tasksEntity.Select(t => new EmployeeTask.Tasks
                {
                    TaskId = t.TaskId,
                   TaskTitle = t.TaskTitle,
                    TaskDescription = t.TaskDescription
                }).ToList();

                /* var taskSelectList = tasks.Select(t => new SelectListItem
                 {
                     Value = t.TaskId.ToString(),
                     Text = t.TaskTitle + " " +
                     t.TaskDescription
                 }).ToList();*/

                var employeeTaskModel = new EmployeeTask
                {
                    EmployeesList = employeeSelectList,
                    TaskList = tasksList
                };

                return View(employeeTaskModel);
            }
        }

        // POST: Create
        [HttpPost]
        [Authorize(Roles = "Team Leader")] // Only allow TeamLeader role for Create POST
        public async Task<IActionResult> Create(EmployeeTask employeeTaskModel)
        {
            ModelState.Remove("TaskId");
            ModelState.Remove("TaskList");
            ModelState.Remove("EmployeesList");
            ModelState.Remove("employee");
            ModelState.Remove("tasks");
            ModelState.Remove("TaskStatus");
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "An error occurred"});
            }

            using (var session = _nhibernateHelper.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {

                    var employee = session.Get<EmployeeEntity>(employeeTaskModel.EmployeeId);

                    // Create a new EmployeeTaskEntity
                    foreach (var taskId in employeeTaskModel.TaskListId)
                    {
                        var task = session.Get<TaskEntity>(taskId);
                        if (task != null)
                        {
                            var employeeTaskEntity = new EmployeeTaskEntity
                            {
                                Employee = employee,
                                Task = task,
                                AssignDate = employeeTaskModel.AssignDate,
                                TaskStatus = "Incomplete"
                            };

                            await session.SaveAsync(employeeTaskEntity); // Save each employee-task assignment
                        }
                    }
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Task successfully Assign" });
                }
            }
        }

        [HttpGet]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> GetTaskList(int page = 1, int pageSize = 2)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                var tasksEntity = await session.QueryOver<TaskEntity>().ListAsync();
                var totalTasks = tasksEntity.Count;
                var totalPages = (int)Math.Ceiling(totalTasks / (double)pageSize);

                var tasksList = tasksEntity
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new EmployeeTask.Tasks
                    {
                        TaskId = t.TaskId,
                        TaskTitle = t.TaskTitle,
                        TaskDescription = t.TaskDescription
                    })
                    .ToList();

                var model = new PagedTaskViewModel
                {
                    Tasks = tasksList,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return PartialView("_TaskListPartialView", model);
            }
        }


        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> Delete(int? EmployeeTaskId)
        {
            if (EmployeeTaskId == null)
            {
                return NotFound();
            }

            using (var session = _nhibernateHelper.OpenSession())
            {

                // Define aliases for your entities
                EmployeeTaskEntity employeeTaskAlias = null;
                EmployeeEntity employeeAlias = null;
                TaskEntity taskAlias = null;

                // Asynchronous QueryOver to perform join between EmployeeTask, Employee, and Task
                var employeeTaskEntities = await session.QueryOver(() => employeeTaskAlias)
                    .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                    .JoinAlias(() => employeeTaskAlias.Task, () => taskAlias)         // Join with Task
                    .Where(() => employeeTaskAlias.EmployeeTaskId == EmployeeTaskId)      // Filter by EmployeeTaskId
                     .SelectList(list => list
                        .Select(() => employeeTaskAlias.EmployeeTaskId).WithAlias(() => employeeTaskAlias.EmployeeTaskId)  // Select EmployeeTaskId
                        .Select(() => employeeTaskAlias.AssignDate).WithAlias(() => employeeTaskAlias.AssignDate)          // Select AssignDate
                        .Select(() => employeeTaskAlias.Employee).WithAlias(() => employeeTaskAlias.Employee)          // Select AssignDate
                        .Select(() => employeeTaskAlias.Task).WithAlias(() => employeeTaskAlias.Task)          // Select AssignDate

                    )
                    .TransformUsing(Transformers.AliasToBean<EmployeeTaskEntity>())  // Map results to EmployeeTaskEntity model
                    .SingleOrDefaultAsync();

                if (employeeTaskEntities == null)
                {
                    return NotFound(); // Return 404 if the task is not found
                }

                // Convert entities to models
                EmployeeTask employeeTasks = new EmployeeTask
                {
                    EmployeeTaskId = employeeTaskEntities.EmployeeTaskId,
                    EmployeeId = employeeTaskEntities.Employee.EmployeeId,
                    TaskId = employeeTaskEntities.Task.TaskId,
                    TaskStatus = employeeTaskEntities.TaskStatus,
                    AssignDate = employeeTaskEntities.AssignDate,

                    tasks = new EmployeeTask.Tasks
                    {

                        TaskTitle = employeeTaskEntities.Task.TaskTitle,
                        TaskPriority = employeeTaskEntities.Task.TaskPriority,

                    },

                    employee = new EmployeeTask.Employee
                    {
                        EmployeeName = employeeTaskEntities.Employee.EmployeeName,
                    }
                };


                return View(employeeTasks);
            }
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> DeleteConfirmed(int? EmployeeTaskId)
        {
            try
            {
                using (var session = _nhibernateHelper.OpenSession())
                {
                    var employeeTaskEntity = await session.QueryOver<EmployeeTaskEntity>().Where(e => e.EmployeeTaskId == EmployeeTaskId).SingleOrDefaultAsync();

                    if (employeeTaskEntity != null)
                    {
                        using (var transaction = session.BeginTransaction())
                        {
                            session.Delete(employeeTaskEntity);
                            await transaction.CommitAsync();
                            

                            return Json(new { success = true, redirectUrl = Url.Action("Index", "EmployeesTask") });
                        }
                    }

                    return Json(new { success = false, message = "Error deleting the assigned task" });
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AcceptTask(int employeeTaskId)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // Define aliases for your entities
                EmployeeTaskEntity employeeTaskAlias = null;
                EmployeeEntity employeeAlias = null;
                TaskEntity taskAlias = null;

                // Asynchronous QueryOver to perform join between EmployeeTask, Employee, and Task
                var employeeTask = await session.QueryOver(() => employeeTaskAlias)
                    .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                    .JoinAlias(() => employeeTaskAlias.Task, () => taskAlias)         // Join with Task
                    .Where(() => employeeTaskAlias.EmployeeTaskId == employeeTaskId)      // Filter by EmployeeTaskId
                     .SelectList(list => list
                        .Select(() => employeeTaskAlias.EmployeeTaskId).WithAlias(() => employeeTaskAlias.EmployeeTaskId)  // Select EmployeeTaskId
                        .Select(() => employeeTaskAlias.AssignDate).WithAlias(() => employeeTaskAlias.AssignDate)          // Select AssignDate
                        .Select(() => employeeTaskAlias.Employee).WithAlias(() => employeeTaskAlias.Employee)          // Select AssignDate
                        .Select(() => employeeTaskAlias.Task).WithAlias(() => employeeTaskAlias.Task)          // Select AssignDate

                    )
                    .TransformUsing(Transformers.AliasToBean<EmployeeTaskEntity>())  // Map results to EmployeeTaskEntity model
                    .SingleOrDefaultAsync();

                if (employeeTask == null)
                {
                    return NotFound(); // Return 404 if the task is not found
                }

                employeeTask.TaskStatus = "Pending";

                // Save changes to the database
                using (var transaction = session.BeginTransaction())
                {
                    await session.UpdateAsync(employeeTask);
                    await transaction.CommitAsync();
                }

                // Redirect back to the task list or another view
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> FinishTask(int employeeTaskId)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // Define aliases for your entities
                EmployeeTaskEntity employeeTaskAlias = null;
                EmployeeEntity employeeAlias = null;
                TaskEntity taskAlias = null;

                // Asynchronous QueryOver to perform join between EmployeeTask, Employee, and Task
                var employeeTask = await session.QueryOver(() => employeeTaskAlias)
                    .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                    .JoinAlias(() => employeeTaskAlias.Task, () => taskAlias)         // Join with Task
                    .Where(() => employeeTaskAlias.EmployeeTaskId == employeeTaskId)      // Filter by EmployeeTaskId
                     .SelectList(list => list
                        .Select(() => employeeTaskAlias.EmployeeTaskId).WithAlias(() => employeeTaskAlias.EmployeeTaskId)  // Select EmployeeTaskId
                        .Select(() => employeeTaskAlias.AssignDate).WithAlias(() => employeeTaskAlias.AssignDate)          // Select AssignDate
                        .Select(() => employeeTaskAlias.Employee).WithAlias(() => employeeTaskAlias.Employee)          // Select AssignDate
                        .Select(() => employeeTaskAlias.Task).WithAlias(() => employeeTaskAlias.Task)          // Select AssignDate

                    )
                    .TransformUsing(Transformers.AliasToBean<EmployeeTaskEntity>())  // Map results to EmployeeTaskEntity model
                    .SingleOrDefaultAsync();

                if (employeeTask == null)
                {
                    return NotFound(); // Return 404 if the task is not found
                }

                employeeTask.TaskStatus = "Finish";

                // Save changes to the database
                using (var transaction = session.BeginTransaction())
                {
                    await session.UpdateAsync(employeeTask);
                    await transaction.CommitAsync();
                }

                // Redirect back to the task list or another view
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TaskDetails(int employeeTaskId)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // Define aliases for your entities
                EmployeeTaskEntity employeeTaskAlias = null;
                EmployeeEntity employeeAlias = null;
                TaskEntity taskAlias = null;

                // Asynchronous QueryOver to perform join between EmployeeTask, Employee, and Task
                var employeeTaskEntities = await session.QueryOver(() => employeeTaskAlias)
                    .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                    .JoinAlias(() => employeeTaskAlias.Task, () => taskAlias)         // Join with Task
                    .Where(() => employeeTaskAlias.EmployeeTaskId == employeeTaskId)      // Filter by EmployeeTaskId
                     .SelectList(list => list
                        .Select(() => employeeTaskAlias.EmployeeTaskId).WithAlias(() => employeeTaskAlias.EmployeeTaskId)  // Select EmployeeTaskId
                        .Select(() => employeeTaskAlias.TaskStatus).WithAlias(() => employeeTaskAlias.TaskStatus)  // Select EmployeeTaskId
                        .Select(() => employeeTaskAlias.AssignDate).WithAlias(() => employeeTaskAlias.AssignDate)          // Select AssignDate
                        .Select(() => employeeTaskAlias.Employee).WithAlias(() => employeeTaskAlias.Employee)          // Select AssignDate
                        .Select(() => employeeTaskAlias.Task).WithAlias(() => employeeTaskAlias.Task)          // Select AssignDate

                    )
                    .TransformUsing(Transformers.AliasToBean<EmployeeTaskEntity>())  // Map results to EmployeeTaskEntity model
                    .SingleOrDefaultAsync();

                if (employeeTaskEntities == null)
                {
                    return NotFound(); // Return 404 if the task is not found
                }

                // Convert entities to models
                EmployeeTask employeeTasks = new EmployeeTask
                {
                    EmployeeTaskId = employeeTaskEntities.EmployeeTaskId,
                    EmployeeId = employeeTaskEntities.Employee.EmployeeId,
                    TaskId = employeeTaskEntities.Task.TaskId,
                    TaskStatus = employeeTaskEntities.TaskStatus,
                    AssignDate = employeeTaskEntities.AssignDate,

                    tasks = new EmployeeTask.Tasks
                    {

                        TaskTitle = employeeTaskEntities.Task.TaskTitle,
                        TaskPriority = employeeTaskEntities.Task.TaskPriority,
                        TaskDescription = employeeTaskEntities.Task.TaskDescription,

                    },

                    employee = new EmployeeTask.Employee
                    {
                        EmployeeName = employeeTaskEntities.Employee.EmployeeName,
                    }
                };

                return View(employeeTasks);
            }
        }
    }
}
