using DemoWeb.Data;
using DemoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DemoWeb.Entity;
using Microsoft.AspNetCore.Identity;
using NHibernate.Exceptions;

using System.Data.SqlClient;

namespace DemoWeb.Controllers
{
    [Authorize] // only login user can access the controller
    public class EmployeeController : Controller
    {

        private readonly NHibernateHelper _nhibernateHelper;
        private readonly IPasswordHasher<Employee> _passwordHasher;

        public EmployeeController(NHibernateHelper nHibernateHelper, IPasswordHasher<Employee> passwordHasher)
        {
            _nhibernateHelper = nHibernateHelper;
            _passwordHasher = passwordHasher;
        }

        // Specific HR Function
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Index(string searchString)
        {

            using (var session = _nhibernateHelper.OpenSession())
            {
                // Implement Entity for query
                var employeeEntity = await session.QueryOver<EmployeeEntity>().ListAsync();


                // convert entity into model to pass into views
                var employee = employeeEntity.Select(entity => new Employee
                {
                    EmployeeId = entity.EmployeeId,
                    EmployeeName = entity.EmployeeName,
                    EmployeeAddress = entity.EmployeeAddress,
                    EmployeeEmail = entity.EmployeeEmail,
                    EmployeePosition = entity.EmployeePosition,
                    isActive = entity.isActive,
                    Password = entity.Password.ToString(),
                    DateJoined = entity.DateJoined,
                });

                return View(employee);
            }
        }

        [Authorize(Roles = "HR")] // Ensure only authenticated users can access this action
        public async Task<IActionResult> Details(int? id)
        {
            try
            {

                if (id != null)
                {
                    using (var session = _nhibernateHelper.OpenSession())
                    {
                        EmployeeTaskEntity employeeTaskAlias = null;
                        EmployeeEntity employeeAlias = null;
                        TaskEntity taskAlias = null;

                        // Fetch the employee details based on the employee ID
                        var employeeEntity = await session.QueryOver<EmployeeEntity>()
                            .Where(x => x.EmployeeId == id)
                            .SingleOrDefaultAsync();
                        // Count the number of finished tasks for a specific employee
                        var finishTask = await session.QueryOver(() => employeeTaskAlias)
                                       .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                                       .Where(() => employeeAlias.EmployeeId == id) // Filter by employeeId
                                       .And(() => employeeTaskAlias.TaskStatus == "Finish") // Filter by TaskStatus "Finished"
                                       .RowCountAsync();

                        var pendingTask = await session.QueryOver(() => employeeTaskAlias)
                                      .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                                      .Where(() => employeeAlias.EmployeeId == id) // Filter by employeeId
                                      .And(() => employeeTaskAlias.TaskStatus == "Pending") // Filter by TaskStatus "Finished"
                                      .RowCountAsync();

                        var incompleteTask = await session.QueryOver(() => employeeTaskAlias)
                                      .JoinAlias(() => employeeTaskAlias.Employee, () => employeeAlias) // Join with Employee
                                      .Where(() => employeeAlias.EmployeeId == id) // Filter by employeeId
                                      .And(() => employeeTaskAlias.TaskStatus == "Incomplete") // Filter by TaskStatus "Finished"
                                      .RowCountAsync();


                        if (employeeEntity != null)
                        {
                            Employee employee = new Employee
                            {
                                EmployeeId = employeeEntity.EmployeeId,
                                EmployeeName = employeeEntity.EmployeeName,
                                EmployeeAddress = employeeEntity.EmployeeAddress,
                                EmployeeEmail = employeeEntity.EmployeeEmail,
                                EmployeePosition = employeeEntity.EmployeePosition,
                                isActive = employeeEntity.isActive,
                                Password = employeeEntity.Password,
                                DateJoined = employeeEntity.DateJoined,
                                finishTaskCount = finishTask,
                                pendingTaskCount = pendingTask,
                                incompleteTaskCount = incompleteTask,
                            };
                            return View(employee);
                        }
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Employee does not exist" });
                }
            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = "An error occurred while fetching employee details.", error = ex.Message });
            }

            return Json(new { success = false, message = "An error occurred." });
        }

        // Specific HR function
        [Authorize(Roles = "HR")]
        public IActionResult Create()
        {

            Employee employee = new Employee();

            // Populate the EmployeeRole list
            employee.PositionList = new List<SelectListItem>
            {
                new SelectListItem { Text = "HR", Value = "HR" },
                new SelectListItem { Text = "Team Leader", Value = "Team Leader" },
                new SelectListItem { Text = "Employee", Value = "Employee" }

            };

            return View(employee);
        }

        [HttpPost]
        [Authorize(Roles = "HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee) // recevie employee models
        {
            try
            {
                using (var session = _nhibernateHelper.OpenSession())

                // transaction are used for making changes in database
                using (var transaction = session.BeginTransaction())
                {
                    // remove null attribute 
                    ModelState.Remove("PositionList");
                    ModelState.Remove("isActive");

                    // Check if Password and ConfirmPassword match
                    if (employee.Password != employee.ConfirmPassword)
                    {
                        return Json(new { success = false, message = "Password and Confirm Password not similar" });
                    }
                    // validation user input
                    if (ModelState.IsValid)
                    {
                        // convert employee models into employee entity
                        EmployeeEntity employeeEntity = new EmployeeEntity
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.EmployeeName,
                            EmployeeAddress = employee.EmployeeAddress,
                            EmployeeEmail = employee.EmployeeEmail,
                            EmployeePosition = employee.EmployeePosition,
                            DateJoined = employee.DateJoined,
                            isActive = true,
                            Password = _passwordHasher.HashPassword(null, employee.Password),

                        };

                        // update the database
                        session.Save(employeeEntity);
                        await transaction.CommitAsync();  // If NHibernate supports async
                        return Json(new { success = true, message = "Employee successfully created" });
                    }
                }
            }
            catch (Exception ex) when (IsUniqueConstraintViolation(ex))
            {
                // Return a JSON response for unique constraint violation
                return Json(new { success = false, message = "An employee with this email already exists." });
            }
            catch (Exception ex)
            {
                // Handle other exceptions and log as needed
                return Json(new { success = false, message = "An error occurred while creating the employee.", error = ex.Message });
            }

            return Json(new { success = false, message = "Validation failed", errors = ModelState });
        }


        [Authorize(Roles = "HR, Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (var session = _nhibernateHelper.OpenSession())
            {
                // query for the employee and pass into employee entity
                var employeeEntity = await session.QueryOver<EmployeeEntity>().Where(x => x.EmployeeId == id).SingleOrDefaultAsync<EmployeeEntity>();

                if (employeeEntity != null)
                {
                    // employee models
                    Employee employee = new Employee
                    {
                        EmployeeId = employeeEntity.EmployeeId,
                        EmployeeName = employeeEntity.EmployeeName,
                        EmployeeAddress = employeeEntity.EmployeeAddress,
                        EmployeeEmail = employeeEntity.EmployeeEmail,
                        EmployeePosition = employeeEntity.EmployeePosition,
                        isActive = employeeEntity.isActive,
                        Password = employeeEntity.Password,
                        DateJoined = employeeEntity.DateJoined
                    };

                    employee.PositionList = new List<SelectListItem>
                    {
                         new SelectListItem { Text = "HR", Value = "HR" },
                         new SelectListItem { Text = "Team Leader", Value = "Team Leader" },
                         new SelectListItem { Text = "Employee", Value = "Employee" }
                    };

                    return View(employee);
                }

                return NotFound();
            }
        }

        [HttpPost]
        [Authorize(Roles = "HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee employee)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                ModelState.Remove("PositionList");
                ModelState.Remove("ConfirmPassword");

                if (ModelState.IsValid)
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        try
                        {
                            EmployeeEntity employeeEntity = new EmployeeEntity
                            {
                                EmployeeId = employee.EmployeeId,
                                EmployeeName = employee.EmployeeName,
                                EmployeeAddress = employee.EmployeeAddress,
                                EmployeeEmail = employee.EmployeeEmail,
                                EmployeePosition = employee.EmployeePosition,
                                DateJoined = employee.DateJoined,
                                isActive = employee.isActive,
                                Password = employee.Password,
                            };

                            session.Update(employeeEntity);
                            transaction.Commit();
                            return Json(new { success = true, message = "Employee successfully edited." });
                        }
                        catch (Exception ex)
                        {

                            return Json(new { success = false, message = "An error occurred while editing the employee: " + ex.Message });
                        }
                    }
                }
            }

            return Json(new { success = false, message = "Invalid model state." });
        }



        [HttpPost]
        [Authorize(Roles = "HR")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int? EmployeeId)
        {
            if (EmployeeId == null)
            {
                return Json(new { success = false, message = "Invalid employee ID." });
            }

            try
            {
                using (var session = _nhibernateHelper.OpenSession())
                {
                    var employee = await session.QueryOver<EmployeeEntity>()
                                                .Where(e => e.EmployeeId == EmployeeId)
                                                .SingleOrDefaultAsync();

                    if (employee == null)
                    {
                        return Json(new { success = false, message = "Employee not found." });
                    }

                    using (var transaction = session.BeginTransaction())
                    {
                        employee.isActive = false;
                        session.Update(employee);
                        await transaction.CommitAsync();
                    }

                    return Json(new { success = true, message = "Employee deactivated successfully." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                return Json(new { success = false, message = "An error occurred while deactivating the employee.", error = ex.Message });
            }
        }

        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GetTaskList(int page = 1, int pageSize = 5)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                var employeeEntity = await session.QueryOver<EmployeeEntity>().ListAsync();
                var totalTasks = employeeEntity.Count;
                var totalPages = (int)Math.Ceiling(totalTasks / (double)pageSize);

                var employeeList = employeeEntity
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => new Employee
                    {
                        EmployeeId = e.EmployeeId,
                       EmployeeName = e.EmployeeName,
                       EmployeeEmail = e.EmployeeEmail,
                       EmployeeAddress = e.EmployeeAddress,
                       EmployeePosition = e.EmployeePosition,
                       DateJoined = e.DateJoined,
                       isActive = e.isActive,
                    })
                    .ToList();

                var model = new PagedTaskViewModel
                {
                    Employees = employeeList,
                    CurrentPage = page,
                    TotalPages = totalPages
                };

                return PartialView("_EmployeePartialView", model);
            }
        }

        // Helper method to check if exception is a unique constraint violation
        private bool IsUniqueConstraintViolation(Exception ex)
        {
			// Check if exception is a GenericADOException, which NHibernate typically uses
			if (ex is GenericADOException adoEx && adoEx.InnerException is SqlException sqlEx)
			{
				// Iterate through the SQL errors to check for unique constraint violations
				foreach (SqlError error in sqlEx.Errors)
				{
					if (error.Number == 2627 || error.Number == 2601)
					{
						return true; // Unique constraint violation
					}
				}
			}
			return false; // Not a unique constraint violation

		}
	}
}
