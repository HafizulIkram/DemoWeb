using DemoWeb.Data;
using DemoWeb.Entity;
using DemoWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using static DemoWeb.Models.EmployeeTask;
using Employee = DemoWeb.Models.Employee;


namespace FirstWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly NHibernateHelper _nhibernateHelper;
        private readonly IPasswordHasher<DemoWeb.Models.Employee> _passwordHasher;

        public HomeController(NHibernateHelper nHibernateHelper, IPasswordHasher<DemoWeb.Models.Employee> passwordHasher)
        {
            _nhibernateHelper = nHibernateHelper;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login loginModel)
        {
            try
            {
                using (var session = _nhibernateHelper.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    // Validate user input
                    if (ModelState.IsValid)
                    {
                        // Find employee based on email and password
                        var employeeEntity = await session.QueryOver<EmployeeEntity>()
                            .Where(x => x.EmployeeEmail == loginModel.EmployeeEmail)
                            .SingleOrDefaultAsync();

                        if (employeeEntity != null)
                        {

                            var employee = new Employee
                            {
                                EmployeeId = employeeEntity.EmployeeId,
                                EmployeeName = employeeEntity.EmployeeName,
                                EmployeeEmail = employeeEntity.EmployeeEmail,
                                Password = employeeEntity.Password,
                            };

							
							if (_passwordHasher.VerifyHashedPassword(employee, employee.Password, loginModel.Password) == PasswordVerificationResult.Success)
							{
								if (!employeeEntity.isActive) // Check if employee is active
								{
									return Json(new { success = false, message = "Your account is inactive. Please contact support." });
								}

								// Create claims for the logged-in user
								var claims = new List<Claim>
							{
								new Claim(ClaimTypes.Name, employeeEntity.EmployeeName),
								new Claim(ClaimTypes.Email, employeeEntity.EmployeeEmail),
								new Claim("EmployeeId", employeeEntity.EmployeeId.ToString()),
								new Claim(ClaimTypes.Role, employeeEntity.EmployeePosition) // Add role claims as necessary
							};

								// Create the identity and sign in the user
								var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
								var principal = new ClaimsPrincipal(identity);
								await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

								// Return success response with redirect URL
								return Json(new { success = true, redirectUrl = Url.Action("Index", "EmployeesTask") });
							}
						}

                    }
                   
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred", error = ex.Message });
            }

            // If login fails, return an error message
            return Json(new { success = false, message = "Invalid login attempt. Please check your email and password." });
        }

        public IActionResult ForgetPassword()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgetPassword(Employee employee)
		{
			using (var session = _nhibernateHelper.OpenSession())
			{
				try
				{
                    // Check if employee exists based on email, name, and address
                    var employeeEntity = await session.QueryOver<EmployeeEntity>()
                        .Where(x => x.EmployeeEmail == employee.EmployeeEmail)
                                  
                        .SingleOrDefaultAsync();

                    if (employeeEntity != null)
                    {
                        // Employee exists, prompt to enter new password
                        return Json(new { success = true, message = "Employee found. Enter new password." });
                    }
                    else
                    {
                        // Employee does not exist
                        return Json(new { success = false, message = "Employee does not exist." });
                    }

                }
				catch (Exception ex)
				{
					return Json(new { success = false, message = "An error occurred.", error = ex.Message });
				}
			}
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdatePassword(string EmployeeEmail, string Password, string ConfirmPassword)
		{
			using (var session = _nhibernateHelper.OpenSession())
			{
				try
				{


					if (string.IsNullOrEmpty(Password))
					{
						return Json(new { success = false, message = "New password cannot be empty." });
					}

                    if (!Password.Equals(ConfirmPassword))
                    {
						return Json(new { success = false, message = "Password does not match" });
					}

					var employeeEntity = await session.QueryOver<EmployeeEntity>()
						.Where(e => e.EmployeeEmail == EmployeeEmail)
						.SingleOrDefaultAsync();

					employeeEntity.Password = _passwordHasher.HashPassword(null, Password); // Update the password
					session.Update(employeeEntity);
					await session.FlushAsync(); // Flush the changes to the database

					return Json(new { success = true, message = "Password updated successfully." });
				}
				catch (Exception ex)
				{
					return Json(new { success = false, message = "An error occurred.", error = ex.Message });
				}
			}
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync();
			return RedirectToAction("Index");
		}

        [Authorize] // Ensure only authenticated users can access this action
        public async Task<IActionResult> Profile()
        {
            try
            {

               
                // Get the employee ID from the claims
                var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

                if (employeeIdClaim != null && int.TryParse(employeeIdClaim, out int employeeId))
                {
                    using (var session = _nhibernateHelper.OpenSession())
                    {
                        EmployeeTaskEntity employeeTaskAlias = null;
                        EmployeeEntity employeeAlias = null;
                        TaskEntity taskAlias = null;

                        // Fetch the employee details based on the employee ID
                        var employeeEntity = await session.QueryOver<EmployeeEntity>()
                            .Where(x => x.EmployeeId == employeeId)
                            .SingleOrDefaultAsync();
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
            }
            catch (Exception ex)
            {
               
                return Json(new { success = false, message = "An error occurred while fetching your profile.", error = ex.Message });
            }

            return Json(new { success = false, message = "An error occurred." });
        }

        [Authorize]
        public async Task<IActionResult> EditPassword()
        {
            return View();
        }

        // Method to handle the Edit Password form submission
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPassword(Employee employee)
        {
            // Ensure that both passwords are provided and match
            if (!string.IsNullOrWhiteSpace(employee.Password) && employee.Password == employee.ConfirmPassword)
            {
                return Json(new { success = false, message = "Passwords are empty." });
            }
            else if(employee.Password == employee.ConfirmPassword)
            {
                return Json(new { success = false, message = "Passwords do not match." });
            }
            else
            {
                try
                {
                    var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
                    if (employeeIdClaim != null && int.TryParse(employeeIdClaim, out int employeeId))
                    {
                        using (var session = _nhibernateHelper.OpenSession())
                        using (var transaction = session.BeginTransaction())
                        {
                            // Fetch the employee based on their ID
                            var employeeEntity = await session.QueryOver<EmployeeEntity>()
                                .Where(x => x.EmployeeId == employeeId)
                                .SingleOrDefaultAsync();

                            if (employeeEntity != null)
                            {
                                // Update the password
                                employeeEntity.Password = _passwordHasher.HashPassword(null, employee.Password); // Make sure to hash the password before saving
                                await session.UpdateAsync(employeeEntity);
                                await transaction.CommitAsync();

                                return Json(new { success = true, redirectUrl = Url.Action("Profile", "Home") });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                   
                    return Json(new { success = false, message = "Error updating password." + ex.Message });
                }
            }
           

            return Json(new { success = false, message = "An error occurred."});
        }




    }


}
