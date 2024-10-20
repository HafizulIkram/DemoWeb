using DemoWeb.Data;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DemoWeb.Entity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;


namespace DemoWeb.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly NHibernateHelper _nhibernateHelper;
        
        public EmployeeController(NHibernateHelper nHibernateHelper)
        {
            _nhibernateHelper = nHibernateHelper;
        }

        public async Task<IActionResult> Index(string searchString)
        {

			ViewData["CurrentFilter"] = searchString;
			

			using (var session = _nhibernateHelper.OpenSession())
            {
				var employees = await session.QueryOver<EmployeeEntity>().ListAsync();


				return View(employees);
            }

			
		}

        public IActionResult Create()
        {
            Employee employee = new Employee();

            // Populate the EmployeeRole list
            employee.PositionList = new List<SelectListItem>
            {
                new SelectListItem { Text = "UI", Value = "UI" },
                new SelectListItem { Text = "Backend", Value = "Backend" },
                new SelectListItem { Text = "Fullstack", Value = "Fullstack" }
            };

            return View(employee);
           
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee employee)
        {
            try
            {
                using (var session = _nhibernateHelper.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    ModelState.Remove("PositionList");
                    if (ModelState.IsValid)
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


        public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			using (var session = _nhibernateHelper.OpenSession())
			{
				// Retrieve a single employee based on the provided id
				var employeeEntity = await session.QueryOver<EmployeeEntity>().Where(x => x.EmployeeId == id).SingleOrDefaultAsync<EmployeeEntity>();
                				

				if (employeeEntity != null)
				{
					Employee employee = new Employee
					{
						EmployeeId = employeeEntity.EmployeeId,
						EmployeeName = employeeEntity.EmployeeName,
						EmployeeAddress = employeeEntity.EmployeeName,
						EmployeeEmail = employeeEntity.EmployeeEmail,
						EmployeePosition = employeeEntity.EmployeePosition,
						isActive = employeeEntity.isActive,
						Password = employeeEntity.Password,
						DateJoined = employeeEntity.DateJoined
					};

                    // Populate the EmployeeRole list
                    employee.PositionList = new List<SelectListItem>
            {
                new SelectListItem { Text = "UI", Value = "UI" },
                new SelectListItem { Text = "Backend", Value = "Backend" },
                new SelectListItem { Text = "Fullstack", Value = "Fullstack" }
            };

                    return View(employee);
                }

                return NotFound();
            }
		}


		[HttpPost]
		public async Task<IActionResult> Edit(Employee employee)
		{
			
			using (var session = _nhibernateHelper.OpenSession())
			{

				ModelState.Remove("PositionList");
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
						//return Json(new { success = true, message = "Employee successfully created" });

						return RedirectToAction(nameof(Index));
					}
				}

			}
			//return Json(new { success = false, message = "Validation failed", errors = ModelState });

			return View(employee);
		}

		public async Task<IActionResult> Delete(int? id)
		{

			if (id == null)
			{
				return NotFound();
			}

			using (var session = _nhibernateHelper.OpenSession())
			{
				// Retrieve a single employee based on the provided id
				var employee = session.Query<Employee>().FirstOrDefault(e => e.EmployeeId == id);

				if (employee == null)
				{
					return NotFound();
				}

				return View(employee);
			}
		}

		// POST: Movies/Delete/id
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int? id)
		{
			using (var session = _nhibernateHelper.OpenSession())
			{
                
                // Retrieve a single employee based on the provided id
                var employee = session.Query<Employee>().FirstOrDefault(e => e.EmployeeId == id);

                using (var transaction = session.BeginTransaction())
				{
					session.Delete(employee);
					transaction.Commit();
					//return Json(new { success = true, message = "Employee successfully created" });

					return RedirectToAction(nameof(Index));
				}

			}

		}

        
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login loginModel)
        {
            if (!ModelState.IsValid)
            {
                // Return validation errors
                return Json(new { success = false, message = "Please enter a valid email and password." });
            }

            using (var session = _nhibernateHelper.OpenSession())
            {
                // Find employee based on email and password
                var employeeEntity = await session.QueryOver<EmployeeEntity>()
                                     .Where(x => x.EmployeeEmail == loginModel.EmployeeEmail && x.Password == loginModel.Password)
                                     .SingleOrDefaultAsync();

                if (employeeEntity != null)
                {
                    // Create claims for the logged-in user
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, employeeEntity.EmployeeName),
                new Claim(ClaimTypes.Email, employeeEntity.EmployeeEmail),
                new Claim("EmployeeId", employeeEntity.EmployeeId.ToString())
            };

                    // Create the identity and sign in the user
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    // Return JSON response for a successful login
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Employee") });
                }

                // If login fails, return an error message
                return Json(new { success = false, message = "Invalid login attempt. Please check your email and password." });
            }
        }


    }

}
