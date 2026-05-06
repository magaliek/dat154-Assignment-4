using Assignment_4.Data;
using Assignment_4.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Assignment_4.Controllers
{
    public class HomeController : Controller
    {
        private readonly Dat154Gr2Context _db;

        public HomeController(Dat154Gr2Context db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.Identity?.Name;
                if (!string.IsNullOrEmpty(email))
                {
                    var u = _db.CustomUsers.FirstOrDefault(x => x.Email == email);
                    if (u != null)
                        return RedirectToCasesForRole(u.Role);
                }

                return RedirectToAction("TeacherCases", "Case");
            }

            return Redirect("/Identity/Account/Login");
        }

        private IActionResult RedirectToCasesForRole(string? role)
        {
            if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("StudentCases", "Case");
            return RedirectToAction("TeacherCases", "Case");
        }

        public IActionResult Login()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}