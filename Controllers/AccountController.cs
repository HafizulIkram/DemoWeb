using DemoWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DemoWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Employees> signInManager;

        public IActionResult Index()
        {
            return View();
        }
    }
}
