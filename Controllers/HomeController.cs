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


namespace FirstWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly NHibernateHelper _nhibernateHelper;

        public HomeController(NHibernateHelper nHibernateHelper)
        {
            _nhibernateHelper = nHibernateHelper;
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
                            .Where(x => x.EmployeeEmail == loginModel.EmployeeEmail && x.Password == loginModel.Password)
                            .SingleOrDefaultAsync();

                        // Check if the employee exists and is active
                        if (employeeEntity != null)
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
		public async Task<IActionResult> ForgetPassword(Employee employee)
		{
			using (var session = _nhibernateHelper.OpenSession())
			{
				try
				{
					if (employee.Password == null)
					{
						// Check if employee exists based on email, name, and address
						var employeeEntity = await session.QueryOver<EmployeeEntity>()
							.Where(x => x.EmployeeEmail == employee.EmployeeEmail &&
										x.EmployeeName == employee.EmployeeName)
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
					else
					{
						// Update the password if the employee exists
						var employeeEntity = await session.QueryOver<EmployeeEntity>()
							.Where(x => x.EmployeeEmail == employee.EmployeeEmail)
							.SingleOrDefaultAsync();

						if (employeeEntity != null)
						{
							employeeEntity.Password = employee.Password;
							session.Update(employeeEntity);
							await session.FlushAsync();

							return Json(new { success = true, message = "Password updated successfully." });
						}

						return Json(new { success = false, message = "Error updating password." });
					}
				}
				catch (Exception ex)
				{
					return Json(new { success = false, message = "An error occurred.", error = ex.Message });
				}
			}
		}

		[HttpPost]
		public async Task<IActionResult> UpdatePassword(string EmployeeEmail, string Password)
		{
			using (var session = _nhibernateHelper.OpenSession())
			{
				try
				{
					if (string.IsNullOrEmpty(Password))
					{
						return Json(new { success = false, message = "New password cannot be empty." });
					}

					var employeeEntity = await session.QueryOver<EmployeeEntity>()
						.Where(e => e.EmployeeEmail == EmployeeEmail)
						.SingleOrDefaultAsync();

					if (employeeEntity != null)
					{
						employeeEntity.Password = Password; // Update the password
						session.Update(employeeEntity);
						await session.FlushAsync(); // Flush the changes to the database

						return Json(new { success = true, message = "Password updated successfully." });
					}
					else
					{
						return Json(new { success = false, message = "Employee not found." });
					}
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
                        // Fetch the employee details based on the employee ID
                        var employeeEntity = await session.QueryOver<EmployeeEntity>()
                            .Where(x => x.EmployeeId == employeeId)
                            .SingleOrDefaultAsync();

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
                                DateJoined = employeeEntity.DateJoined
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
        public async Task<IActionResult> EditPassword(Employee employee)
        {
            // Ensure that both passwords are provided and match
            if (!string.IsNullOrWhiteSpace(employee.Password) && employee.Password == employee.ConfirmPassword)
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
                                employeeEntity.Password = employee.Password; // Make sure to hash the password before saving
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
            else
            {
                return Json(new { success = false, message = "Passwords do not match or are empty." });

            }

            return Json(new { success = false, message = "An error occurred."});
        }





    }



	
}
