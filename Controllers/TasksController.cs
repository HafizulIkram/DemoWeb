using DemoWeb.Data;
using DemoWeb.Entity;
using DemoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NHibernate.Criterion;
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
        public IActionResult Index()
        {
            return View();
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
                try
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
                            CreatedAt = DateTime.Today, // If needed
                           
                        };

                        // Save task to the database
                        await session.SaveAsync(taskEntity);
                        await transaction.CommitAsync();

                        return Json(new { success = true, message = "Task successfully created." });
                    }
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "An error occurred while creating the task.", error = ex.Message });
                }
            }

            // Return validation errors if ModelState is invalid
            tasks.PriorityList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Urgent", Value = "Urgent" },
                new SelectListItem { Text = "Normal", Value = "Normal" },
            };

            // Collect ModelState errors to send in the response
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                           .Select(e => e.ErrorMessage)
                                           .ToList();

            return Json(new { success = false, message = "Validation failed.", errors = errors });
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
        [ValidateAntiForgeryToken]
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


                var taskList = taskEntity.Select(e => new Tasks
                {
                    TaskTitle = e.TaskTitle,
                    TaskId = e.TaskId,
                    TaskPriority = e.TaskPriority,
                    TaskDescription = e.TaskDescription,
                    CreatedAt = e.CreatedAt
                }).ToList();

                var model = new PagedTaskViewModel
                {
                    Tasks = taskList,
                    CurrentPage = page,
                    TotalPages = totalPages,

                };

                return PartialView("_TaskPartialView", model);
            }
        }
    }

} 

