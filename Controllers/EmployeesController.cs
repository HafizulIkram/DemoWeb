using DemoWeb.Data;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DemoWeb.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly DemoWebContext _context;

        public EmployeesController(DemoWebContext context)
        {
            _context = context;
        }

        // GET:Employees
        // GET: Employees
        public async Task<IActionResult> Index(int? pageNumber, string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            int pageSize = 6;

            // Start query and load only required fields
            var employeesQuery = _context.Employees
                                .AsNoTracking();
                                

            // Apply search filter
            if (!String.IsNullOrEmpty(searchString))
            {
                employeesQuery = employeesQuery.Where(e => e.EmployeeEmail.Contains(searchString)
                                                       );
            }

            // Paginate result and convert to a list
            var employeesList = await PaginatedList<Employees>.CreateAsync(employeesQuery, pageNumber ?? 1, pageSize);

            return View(employeesList);
        }


        // GET: Movies/Details/id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employees = await _context.Employees
                .FirstOrDefaultAsync( e => e.EmployeeId == id);
            if (employees == null)
            {
                return NotFound();
            }

            return View(employees);
        }

        public async Task<IActionResult> Create()
        {
            Employees employees = new Employees();

            // Populate the EmployeeRole list
            employees.RoleList = new List<SelectListItem>
            {
                new SelectListItem { Text = "UI", Value = "UI" },
                new SelectListItem { Text = "Backend", Value = "Backend" },
                new SelectListItem { Text = "Fullstack", Value = "Fullstack" }
            };

            return View(employees);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Employees employees)
        {
            ModelState.Remove("RoleList"); // If you want to skip validation for RoleList
            if (ModelState.IsValid)
            {
                _context.Add(employees);
                await _context.SaveChangesAsync();
                return Json(new {  message = "Employee created successfully!" });
            }

            // Populate the EmployeeRole list
            employees.RoleList = new List<SelectListItem>
            {
                new SelectListItem { Text = "UI", Value = "UI" },
                new SelectListItem { Text = "Backend", Value = "Backend" },
                new SelectListItem { Text = "Fullstack", Value = "Fullstack" }
            };

            return Json(new { success = true, message = "Validation failed", errors = ModelState });
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employees = await _context.Employees.FindAsync(id);

            // Populate the EmployeeRole list
            employees.RoleList = new List<SelectListItem>
            {
                new SelectListItem { Text = "UI", Value = "UI" },
                new SelectListItem { Text = "Backend", Value = "Backend" },
                new SelectListItem { Text = "Fullstack", Value = "Fullstack" }
            };

            if (employees == null)
            {
                return NotFound();
            }

            return View(employees);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employees employees)
        {
            if (id != employees.EmployeeId)
            {
                return NotFound();
            }

            ModelState.Remove("RoleList");
            if (ModelState.IsValid)
            {

                try
                {

                    _context.Update(employees);
                    await _context.SaveChangesAsync();


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeesExists(employees.EmployeeId))
                    {
                        return Json(new { success = false, message = "Employee not found" });
                    }
                    else
                    {
                        throw;
                    }
                }
                return Json(new { success = true, message = "Employee edit successfully!" });
            }

            return Json(new { success = false, message = "Validation failed", errors = ModelState });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employees = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employees == null)
            {
                return NotFound();
            }

            return View(employees);
        }

        // POST: Movies/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employees = await _context.Employees.FindAsync(id);
            if (employees != null)
            {
                _context.Employees.Remove(employees);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeesExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

    }
}
