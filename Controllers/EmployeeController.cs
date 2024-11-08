﻿using DemoWeb.Data;
using DemoWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DemoWeb.Entity;

namespace DemoWeb.Controllers
{
    [Authorize] // only login user can access the controller
    public class EmployeeController : Controller
    {

        private readonly NHibernateHelper _nhibernateHelper;

        public EmployeeController(NHibernateHelper nHibernateHelper)
        {
            _nhibernateHelper = nHibernateHelper;
        }

        // Specific HR Function
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Index(string searchString)
        {

            ViewData["CurrentFilter"] = searchString;

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
       #region MyRegionName
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
                            isActive = employee.isActive,
                            Password = employee.Password,
                        };

                        // update the database
                        session.Save(employeeEntity);
                        await transaction.CommitAsync();  // If NHibernate supports async
                        return Json(new { success = true, message = "Employee successfully created" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred", error = ex.Message });
            }

            return Json(new { success = false, message = "Validation failed", errors = ModelState });
        }
        #endregion

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
        [Authorize(Roles = "HR, Employee")]
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
                        return Json(new { success = true, message = "Employee successfully Edit" });
                    }
                }
            }

            return View(employee);
        }

        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            using (var session = _nhibernateHelper.OpenSession())
            {
                var employeeEntity = await session.QueryOver<EmployeeEntity>().Where(e => e.EmployeeId == id).SingleOrDefaultAsync();

                if (employeeEntity == null)
                {
                    return NotFound();
                }

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

                return View(employee);
            }
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> DeleteConfirmed(int? EmployeeId)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                var employee = await session.QueryOver<EmployeeEntity>().Where(e => e.EmployeeId == EmployeeId).SingleOrDefaultAsync();

                if (employee != null)
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Delete(employee);
                        await transaction.CommitAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }

                return NotFound();
            }
        }

		[HttpPost]
		[Authorize(Roles = "HR")]
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
	}
}
