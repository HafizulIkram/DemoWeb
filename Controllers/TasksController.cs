using DemoWeb.Data;
using DemoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DemoWeb.Controllers
{
    public class TasksController : Controller
    {
        private readonly NHibernateHelper _nhibernateHelper;

        public TasksController(NHibernateHelper nHibernateHelper)
        {
            _nhibernateHelper = nHibernateHelper;
        }

        public async Task<IActionResult> Index(string searchString)
        {


            using (var session = _nhibernateHelper.OpenSession())
            {
                var tasks = session.Query<Tasks>().ToList();

                return View(tasks);
            }


        }

        public IActionResult Create()
        {
           

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Create(Tasks tasks)
        {
            using (var session = _nhibernateHelper.OpenSession())
            {
                
                if (ModelState.IsValid)
                {
                    using (var transaction = session.BeginTransaction())
                    {
                        session.Save(tasks);
                        transaction.Commit();
                        return Json(new { success = true, message = "Tasks successfully created" });

                        //return RedirectToAction(nameof(Index));
                    }
                }

            }
             return Json(new { success = false, message = "Validation failed", errors = ModelState });

            // return View(tasks);
        }


        /* public async Task<IActionResult> Details(int? id)
         {
             if (id == null)
             {
                 return NotFound();
             }

             var tasks = await _context.Tasks
                 .FirstOrDefaultAsync(t => t.TaskId == id);
             if (tasks == null)
             {
                 return NotFound();
             }

             return View(tasks);
         }

         public async Task<IActionResult> Create()
         {
             Tasks task = new Tasks();

             task.StatusList = new List<SelectListItem>
             {
                 new SelectListItem
                 {
                     Text = "Not Complete",
                     Value = "Not Complete"
                 },

                 new SelectListItem {
                     Text = "In-Progress",
                     Value = "In-Progress"
                 },

                 new SelectListItem {
                     Text = "Complete",
                     Value = "Complete"
                 },

             };

             return View(task);
         }

         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Create(Tasks tasks)
         {
             ModelState.Remove("StatusList");
             if (ModelState.IsValid)
             {
                 _context.Add(tasks);
                 await _context.SaveChangesAsync();
                 return RedirectToAction(nameof(Index));
             }
             return View();
         }

         // GET: Movies/Edit/5
         public async Task<IActionResult> Edit(int? id)
         {
             if (id == null)
             {
                 return NotFound();
             }

             var tasks = await _context.Tasks.FindAsync(id);



             tasks.StatusList = new List<SelectListItem>
             {
                 new SelectListItem
                 {
                     Text = "Not Complete",
                     Value = "Not Complete"
                 },

                 new SelectListItem {
                     Text = "In-Progress",
                     Value = "In-Progress"
                 },

                 new SelectListItem {
                     Text = "Complete",
                     Value = "Complete"
                 },

             };

             if (tasks == null)
             {
                 return NotFound();
             }

             return View(tasks);
         }

         // POST: Movies/Edit/id

         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> Edit(int id, Tasks tasks)
         {
             if (id != tasks.TaskId)
             {
                 return NotFound();
             }

             ModelState.Remove("StatusList");
             if (ModelState.IsValid)
             {

                 try
                 {

                     _context.Update(tasks);
                     await _context.SaveChangesAsync();
                 }
                 catch (DbUpdateConcurrencyException)
                 {
                     if (!TasksExists(tasks.TaskId))
                     {


                         return Json(tasks, new { success = false, message = "Task not found" });
                     }
                     else
                     {
                         throw;
                     }
                 }
                 return Json(new { success = true, message = "Task edit successfully!" });
             }

             tasks.StatusList = new List<SelectListItem>
             {

                 new SelectListItem
                 {
                     Text = "Not Complete",
                     Value = "Not Complete"
                 },

                 new SelectListItem {
                     Text = "In-Progress",
                     Value = "In-Progress"
                 },

                 new SelectListItem {
                     Text = "Complete",
                     Value = "Complete"
                 },

             };

             return Json(tasks, new { success = false, message = "Validation failed", errors = ModelState });

         }

         // GET: Movies/Delete/5
         public async Task<IActionResult> Delete(int? id)
         {
             if (id == null)
             {
                 return NotFound();
             }

             var tasks = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == id);
             if (tasks == null)
             {
                 return NotFound();
             }

             return View(tasks);
         }

         // POST: Movies/Delete/id
         [HttpPost, ActionName("Delete")]
         [ValidateAntiForgeryToken]
         public async Task<IActionResult> DeleteConfirmed(int id)
         {
             var tasks = await _context.Tasks.FindAsync(id);
             if (tasks != null)
             {
                 _context.Tasks.Remove(tasks);
             }

             await _context.SaveChangesAsync();
             return RedirectToAction(nameof(Index));
         }

         private bool TasksExists(int id)
         {
             return _context.Tasks.Any(t => t.TaskId == id);
         }*/


    }
}
