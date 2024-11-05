using DemoWeb.Data;
using DemoWeb.Entity;
using DemoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static DemoWeb.Models.Tasks;

namespace DemoWeb.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly NHibernateHelper _nhibernateHelper;

        public TasksController(NHibernateHelper nHibernateHelper)
        {
            _nhibernateHelper = nHibernateHelper;
        }

        // Index action to list tasks
        [Authorize(Roles = "Team Leader")] // Example: Only HR and TeamLeaders can view tasks
        public async Task<IActionResult> Index(string searchString)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                // QueryOver to fetch tasks, with optional filtering by search string
                var taskEntity = await session.QueryOver<TaskEntity>().ListAsync();

                var employees = await session.QueryOver<EmployeeEntity>().ListAsync();

                if (!string.IsNullOrEmpty(searchString))
                {
                    //taskEntity = taskEntity.WhereRestrictionOn(x => x.TaskTitle)
                    //             .IsInsensitiveLike($"%{searchString}%");
                }


                // Tasks models
                var tasks = taskEntity.Select(entity => new Tasks
                {
                    TaskId = entity.TaskId,
                    TaskDescription = entity.TaskDescription,
                    TaskPriority = entity.TaskPriority,
                   
                    TaskTitle = entity.TaskTitle,
                    DueDate = entity.DueDate,
                    CreatedAt = entity.CreatedAt,

                });


              

                return View(tasks);
            }
        }

        // Create action for task creation (Only TeamLeaders allowed to create tasks)
        [Authorize(Roles = "Team Leader")]
        public IActionResult Create()
        {
            Tasks tasks = new Tasks();

            tasks.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Urgent", Value = "Urgent" },
                new SelectListItem { Text = "Normal", Value = "Normal" },

             };
            return View(tasks);
        }

        // POST: Create task
        [HttpPost]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> Create(Tasks tasks)
        {
           
            ModelState.Remove("PriorityList");
            if (ModelState.IsValid)
            {
                using (var session = _nhibernateHelper.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    // Convert TaskViewModel to TaskEntity
                    var taskEntity = new TaskEntity
                    {
                        TaskTitle = tasks.TaskTitle,
                        TaskDescription = tasks.TaskDescription,
                        TaskPriority = tasks.TaskPriority,
                     
                        CreatedAt = tasks.CreatedAt, // If needed
                        DueDate = tasks.DueDate         // If needed
                    };

                    // Save task to the database
                    await session.SaveAsync(taskEntity);
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "Task successfully created" });
                }
            }

            tasks.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Urgent", Value = "Urgent" },
                new SelectListItem { Text = "Normal", Value = "Normal" },

            };

            return Json(new { success = false, message = "Validation failed", errors = ModelState });
        }

        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (var session = _nhibernateHelper.OpenSession())
            {
                // query for the employee and pass into employee entity
                var TaskEntity = await session.QueryOver<TaskEntity>().Where(t => t.TaskId == id).SingleOrDefaultAsync<TaskEntity>();

                if (TaskEntity == null)
                {
                    return NotFound();
                }

                // Tasks models
                Tasks tasks = new Tasks
                {
                    TaskId = TaskEntity.TaskId,
                    TaskDescription = TaskEntity.TaskDescription,
                    TaskPriority = TaskEntity.TaskPriority,
                    TaskTitle = TaskEntity.TaskTitle,
                    DueDate = TaskEntity.DueDate,
                    CreatedAt = TaskEntity.CreatedAt,
                };


                tasks.PriorityList = new List<SelectListItem>
                    {
                         new SelectListItem { Text = "Urgent", Value = "Urgent" },
                         new SelectListItem { Text = "Normal", Value = "Normal" },

                    };

                return View(tasks);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Team Leader")]
        public async Task<IActionResult> Edit(Tasks tasks)
        {
            try
            {
                using (var session = _nhibernateHelper.OpenSession())
                {
                    ModelState.Remove("PriorityList");
                    if (ModelState.IsValid)
                    {
                        using (var transaction = session.BeginTransaction())
                        {
                            TaskEntity taskEntity = new TaskEntity
                            {
                                TaskId = tasks.TaskId,
                                TaskDescription = tasks.TaskDescription,
                                TaskTitle = tasks.TaskTitle,
                                TaskPriority = tasks.TaskPriority,
                             
                                DueDate = tasks.DueDate,
                                CreatedAt = tasks.CreatedAt,
                            };

                            session.Update(taskEntity);
                            transaction.Commit();
                            return Json(new { success = true, message = "Successfuly edit task" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tasks.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Urgent", Value = "Urgent" },
                new SelectListItem { Text = "Normal", Value = "Normal" },

            };

                return Json(new { success = false, message = "An error occurred", error = ex.Message });
            }

            tasks.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Urgent", Value = "Urgent" },
                new SelectListItem { Text = "Normal", Value = "Normal" },

            };


            return Json(new { success = false, message = "Validation failed", errors = ModelState });

        }

    }

} 

