using DemoWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoWeb.Models;

namespace DemoWeb.Controllers
{
    public class ProjectsController : Controller
    {

        private readonly DemoWebContext _context;

        public ProjectsController(DemoWebContext context)
        {
            _context = context;
        }

        // GET:Employees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Projects.ToListAsync());
        }

        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Projects project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }
    }
}
