using DemoWeb.Data;
using DemoWeb.Entity;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NHibernate.Transform;
using Microsoft.AspNetCore.Authorization;
using NHibernate.Criterion;


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
                    DueDate = entity.DueDate,


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

        [Authorize]
        public async Task<IActionResult> GetTaskData()
        {

            try
            {
                using (var session = _nhibernateHelper.OpenSession())
                {
                    EmployeeTaskEntity employeeTaskAlias = null;
                    EmployeeEntity employeeAlias = null;
                    TaskEntity taskAlias = null;

                    // Get the currently logged-in user's EmployeeId from claims
                    var employeeIdClaim = User.FindFirst("EmployeeId");


                    int employeeId = int.Parse(employeeIdClaim.Value);

                    // Query to fetch EmployeeTask entities and join with Employee and Task
                    // Count the number of finished tasks for a specific employee
                    var finishTask = await session.QueryOver(() => employeeTaskAlias)
                                   .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                                   .Where(() => employeeAlias.EmployeeId == employeeId) // Filter by employeeId
                                   .And(() => employeeTaskAlias.TaskStatus == "Finish") // Filter by TaskStatus "Finished"
                                   .RowCountAsync();

                    var pendingTask = await session.QueryOver(() => employeeTaskAlias)
                                  .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                                  .Where(() => employeeAlias.EmployeeId == employeeId) // Filter by employeeId
                                  .And(() => employeeTaskAlias.TaskStatus == "Pending") // Filter by TaskStatus "Finished"
                                  .RowCountAsync();

                    var incompleteTask = await session.QueryOver(() => employeeTaskAlias)
                                  .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                                  .Where(() => employeeAlias.EmployeeId == employeeId) // Filter by employeeId
                                  .And(() => employeeTaskAlias.TaskStatus == "Incomplete") // Filter by TaskStatus "Finished"
                                  .RowCountAsync();


                    // Convert entities to models
                    var TaskData = new TaskViewModel
                    {
                        finishTaskCount = finishTask,
                        pendingTaskCount = pendingTask,
                        incompleteTaskCount = incompleteTask,
                    };

                    return PartialView("TaskDataView", TaskData);
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An error occurred.", error = ex.Message });
            }

        }

        #region Assign New Task
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



                var employeeTaskModel = new EmployeeTask
                {
                    EmployeesList = employeeSelectList,
                    TaskList = tasksList
                };

                return View(employeeTaskModel);
            }
        }

        // POST: Assign new task to employee
        [HttpPost]
        [Authorize(Roles = "Team Leader")] // Only allow TeamLeader role for Create POST
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeTask employeeTaskModel)
        {
            ModelState.Remove("TaskId");
            ModelState.Remove("TaskList");
            ModelState.Remove("EmployeesList");
            ModelState.Remove("employee");
            ModelState.Remove("tasks");
            ModelState.Remove("TaskStatus");

            // Validate the model state
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data provided." });
            }

            try
            {
                // Get the currently logged-in user's EmployeeId from claims
                var employeeIdClaim = User.FindFirst("EmployeeId");


                int employeeId = int.Parse(employeeIdClaim.Value);

                using (var session = _nhibernateHelper.OpenSession())
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        var employee = session.Get<EmployeeEntity>(employeeTaskModel.EmployeeId);

                        if (employee == null)
                        {
                            return Json(new { success = false, message = "Employee not found." });
                        }

                        if (employeeTaskModel.DueDate < DateTime.Today)
                        {
                            return Json(new { success = false, message = "Due Date cannot be lower than today" });
                        }

                        // Create a new EmployeeTaskEntity for each task in the TaskListId
                        foreach (var taskId in employeeTaskModel.TaskListId)
                        {
                            var task = session.Get<TaskEntity>(taskId);
                            if (task != null)
                            {

                                // Query to count tasks with the same due date for the employee
                                int existingTaskCount = session.Query<EmployeeTaskEntity>()
                                                               .Where(x => x.Employee == employee
                                                                           && x.DueDate == employeeTaskModel.DueDate)
                                                               .Count();

                                int NotAcceptTask = session.Query<EmployeeTaskEntity>()
                                                               .Where(x => x.Employee == employee
                                                                           && x.TaskStatus == "Incomplete")
                                                               .Count();

                                // Check if there are already 2 or more tasks with the same due date
                                if (existingTaskCount >= 2)
                                {
                                    return Json(new { success = false, message = "Error: Maximum of 2 tasks can be assigned on the same due date." });
                                }

                                if (NotAcceptTask >= 3)
                                {
                                    return Json(new { success = false, message = "Maximum task had been reached" });
                                }

                                var assignBy = await session.QueryOver<EmployeeEntity>()
                                  .Where(e => e.EmployeeId == employeeId)
                                  .SingleOrDefaultAsync();

                                var employeeTaskEntity = new EmployeeTaskEntity
                                {
                                    Employee = employee,
                                    Task = task,
                                    AssignDate = DateTime.Today,
                                    DueDate = employeeTaskModel.DueDate,
                                    FinishedDate = null,
                                    TaskStatus = "Incomplete",
                                    AssignedBy = assignBy
                                    
                                };



                                await session.SaveAsync(employeeTaskEntity); // Save each employee-task assignment
                            }
                            else
                            {
                                return Json(new { success = false, message = $"Task with ID {taskId} not found." });
                            }
                        }
                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Tasks successfully assigned." });
                    }
                }
            }
            catch (Exception ex)
            {


                return Json(new { success = false, message = "An error occurred while assigning tasks.", error = ex.Message });
            }
        }

        #endregion

        // Partial View. List of Task
        [HttpGet]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> GetTaskList(int page = 1, int pageSize = 5, string query = null)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {

                var queryOver = session.QueryOver<TaskEntity>();

                if (!string.IsNullOrEmpty(query))
                {
                    queryOver.Where(e => e.TaskTitle.IsInsensitiveLike(query, MatchMode.Anywhere));
                }

                // Get the total number of matching employees
                var totalTasks = await queryOver.RowCountAsync(); // Get the total count
                var totalPages = (int)Math.Ceiling(totalTasks / (double)pageSize); // Calculate total pages

                // Apply pagination
                var taskEntity = await queryOver
                    .OrderBy(e => e.TaskTitle).Desc // Order by `isActive` descending
                    .Skip((page - 1) * pageSize) // Skip the previous pages
                    .Take(pageSize) // Take the current page size
                    .ListAsync();

               
                var taskList = taskEntity.Select(e => new Models.Tasks
                {
                   TaskTitle = e.TaskTitle,
                   TaskId = e.TaskId,
                   TaskDescription = e.TaskDescription,
                }).ToList();

                var model = new PagedTaskViewModel
                {
                    Tasks = taskList,
                    CurrentPage = page,
                    TotalPages = totalPages,

                };

                return PartialView("_TaskListPartialView", model);
            }
        }

  

        // Delete the task assigned to a specific employee
        [HttpPost, ActionName("DeleteTask")]
        [Authorize(Roles = "Team Leader")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? EmployeeTaskId)
        {
            if (EmployeeTaskId == null)
            {
                return Json(new { success = false, message = "EmployeeTaskId is required." });
            }

            try
            {
                using (var session = _nhibernateHelper.OpenSession())
                {
                    var employeeTaskEntity = await session.QueryOver<EmployeeTaskEntity>()
                        .Where(e => e.EmployeeTaskId == EmployeeTaskId)
                        .SingleOrDefaultAsync();

                    if (employeeTaskEntity != null)
                    {
                        using (var transaction = session.BeginTransaction())
                        {
                            session.Delete(employeeTaskEntity);
                            await transaction.CommitAsync();

                            return Json(new { success = true, message = "Task successfully deleted."});
                        }
                    }

                    return Json(new { success = false, message = "Task not found." });
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An error occurred while deleting the task.", error = ex.Message });
            }
        }



        // Update: Changing Task Status
        [HttpPost]
        public async Task<IActionResult> AcceptTask(int EmployeeTaskId)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // Define aliases for your entities
                EmployeeTaskEntity employeeTaskAlias = null;
                EmployeeEntity employeeAlias = null;
                TaskEntity taskAlias = null;

                try
                {
                    // Asynchronous QueryOver to perform join between EmployeeTask, Employee, and Task
                    var employeeTask = await session.QueryOver(() => employeeTaskAlias)
                        .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                        .JoinAlias(() => employeeTaskAlias.Task, () => taskAlias)         // Join with Task
                        .Where(() => employeeTaskAlias.EmployeeTaskId == EmployeeTaskId)  // Filter by EmployeeTaskId
                        .SelectList(list => list
                            .Select(() => employeeTaskAlias.EmployeeTaskId).WithAlias(() => employeeTaskAlias.EmployeeTaskId)  // Select EmployeeTaskId
                            .Select(() => employeeTaskAlias.AssignDate).WithAlias(() => employeeTaskAlias.AssignDate)          // Select AssignDate
                            .Select(() => employeeTaskAlias.AssignedBy).WithAlias(() => employeeTaskAlias.AssignedBy)          // Select AssignDate
                            .Select(() => employeeTaskAlias.DueDate).WithAlias(() => employeeTaskAlias.DueDate)          
                            .Select(() => employeeTaskAlias.Employee).WithAlias(() => employeeTaskAlias.Employee)              // Select Employee
                            .Select(() => employeeTaskAlias.Task).WithAlias(() => employeeTaskAlias.Task)                      // Select Task
                        )
                        .TransformUsing(Transformers.AliasToBean<EmployeeTaskEntity>())  // Map results to EmployeeTaskEntity model
                        .SingleOrDefaultAsync();

                    if (employeeTask == null)
                    {
                        return Json(new { success = false, message = "Task not found." }); // Return JSON if the task is not found
                    }

                    employeeTask.TaskStatus = "Pending";
                    employeeTask.FinishedDate = null;

                    // Save changes to the database
                    using (var transaction = session.BeginTransaction())
                    {
                        await session.UpdateAsync(employeeTask);
                        await transaction.CommitAsync();
                    }

                    // Return JSON response for success
                    return Json(new { success = true, message = "Task successfully accepted.", redirectUrl = Url.Action("Index", "EmployeesTask") });
                }
                catch (Exception ex)
                {

                    return Json(new { success = false, message = "An error occurred while updating the task status: " + ex.Message });
                }
            }
        }


        // Update: Changing Task Status
        [HttpPost]
        public async Task<IActionResult> FinishTask(int employeeTaskId)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // Define aliases for your entities
                EmployeeTaskEntity employeeTaskAlias = null;
                EmployeeEntity employeeAlias = null;
                TaskEntity taskAlias = null;

                try
                {
                    // Asynchronous QueryOver to perform join between EmployeeTask, Employee, and Task
                    var employeeTask = await session.QueryOver(() => employeeTaskAlias)
                        .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                        .JoinAlias(() => employeeTaskAlias.Task, () => taskAlias)         // Join with Task
                        .Where(() => employeeTaskAlias.EmployeeTaskId == employeeTaskId)  // Filter by EmployeeTaskId
                        .SelectList(list => list
                            .Select(() => employeeTaskAlias.EmployeeTaskId).WithAlias(() => employeeTaskAlias.EmployeeTaskId)  // Select EmployeeTaskId
                            .Select(() => employeeTaskAlias.AssignDate).WithAlias(() => employeeTaskAlias.AssignDate)          // Select AssignDate
                            .Select(() => employeeTaskAlias.DueDate).WithAlias(() => employeeTaskAlias.DueDate)          // Select AssignDate
                            .Select(() => employeeTaskAlias.AssignedBy).WithAlias(() => employeeTaskAlias.AssignedBy)          // Select AssignDate
                            .Select(() => employeeTaskAlias.Employee).WithAlias(() => employeeTaskAlias.Employee)              // Select Employee
                            .Select(() => employeeTaskAlias.Task).WithAlias(() => employeeTaskAlias.Task)                      // Select Task
                        )
                        .TransformUsing(Transformers.AliasToBean<EmployeeTaskEntity>())  // Map results to EmployeeTaskEntity model
                        .SingleOrDefaultAsync();

                    var date = employeeTask.AssignDate.ToString();

                    if (employeeTask == null)
                    {
                        return Json(new { success = false, message = "Task not found." }); // Return JSON if the task is not found
                    }

                    employeeTask.TaskStatus = "Finish";
                    employeeTask.FinishedDate = DateTime.Today;

                    // Save changes to the database
                    using (var transaction = session.BeginTransaction())
                    {
                        await session.UpdateAsync(employeeTask);
                        await transaction.CommitAsync();
                    }

                    return Json(new { success = true, message = "Task finished.", redirectUrl = Url.Action("Index", "EmployeesTask") });

                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred while updating the task status: " + ex.Message });
                }
            }
        }


        //GET: Task Details
        public async Task<IActionResult> TaskDetails(int employeeTaskId)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // Define aliases for your entities
                EmployeeTaskEntity employeeTaskAlias = null;
                EmployeeEntity employeeAlias = null;
                TaskEntity taskAlias = null;

                try
                {
                    // Asynchronous QueryOver to perform join between EmployeeTask, Employee, and Task
                    var employeeTaskEntities = await session.QueryOver(() => employeeTaskAlias)
                        .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                        .JoinAlias(() => employeeTaskAlias.Task, () => taskAlias)         // Join with Task
                        .Where(() => employeeTaskAlias.EmployeeTaskId == employeeTaskId)  // Filter by EmployeeTaskId
                        .SelectList(list => list
                            .Select(() => employeeTaskAlias.EmployeeTaskId).WithAlias(() => employeeTaskAlias.EmployeeTaskId)  // Select EmployeeTaskId
                            .Select(() => employeeTaskAlias.TaskStatus).WithAlias(() => employeeTaskAlias.TaskStatus)          // Select TaskStatus
                            .Select(() => employeeTaskAlias.AssignDate).WithAlias(() => employeeTaskAlias.AssignDate)          // Select AssignDate
                            .Select(() => employeeTaskAlias.FinishedDate).WithAlias(() => employeeTaskAlias.FinishedDate)          // Select AssignDate
                            .Select(() => employeeTaskAlias.DueDate).WithAlias(() => employeeTaskAlias.DueDate)          // Select AssignDate
                            .Select(() => employeeTaskAlias.Employee).WithAlias(() => employeeTaskAlias.Employee)              // Select Employee
                            .Select(() => employeeTaskAlias.Task).WithAlias(() => employeeTaskAlias.Task)                      // Select Task
                        )
                        .TransformUsing(Transformers.AliasToBean<EmployeeTaskEntity>())  // Map results to EmployeeTaskEntity model
                        .SingleOrDefaultAsync();

                    if (employeeTaskEntities == null)
                    {
                        return Json(new { success = false, message = "Task not found." }); // Return JSON if the task is not found
                    }

                    // Convert entities to models
                    var employeeTasks = new EmployeeTask
                    {
                        EmployeeTaskId = employeeTaskEntities.EmployeeTaskId,
                        EmployeeId = employeeTaskEntities.Employee.EmployeeId,
                        TaskId = employeeTaskEntities.Task.TaskId,
                        TaskStatus = employeeTaskEntities.TaskStatus,
                        AssignDate = employeeTaskEntities.AssignDate,
                        DueDate = employeeTaskEntities.DueDate,
                        FinishedDate = employeeTaskEntities.FinishedDate ?? DateTime.MinValue,


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

                    return View(employeeTasks); // Return JSON with task details
                }
                catch (Exception ex)
                {

                    return Json(new { success = false, message = "An error occurred while fetching task details: " + ex.Message });
                }
            }
        }

        [Authorize(Roles ="Team Leader")]
        public async Task<IActionResult> GetAllData()
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
                    .Where(() => employeeTaskAlias.AssignedBy.EmployeeId == employeeId)
                    .ListAsync<EmployeeTaskEntity>();


                
                // Convert entities to models
                var employeeTasks = employeeTaskEntities.Select(entity => new EmployeeTask
                {
                    EmployeeTaskId = entity.EmployeeTaskId,
                    EmployeeId = entity.Employee.EmployeeId,
                    TaskId = entity.Task.TaskId,
                    TaskStatus = entity.TaskStatus,
                    AssignDate = entity.AssignDate,
                    AssignedBy = entity.AssignedBy.EmployeeId,
                    DueDate = entity.DueDate,



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

               
                return View("LeaderView", employeeTasks);

            }
        }
    }
}
