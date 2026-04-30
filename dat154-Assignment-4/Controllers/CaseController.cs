using Assignment_4.Data;
using Assignment_4.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignment_4.Controllers
{
    public class CaseController : Controller
    {
        private readonly Dat154Gr2Context _db;
        private readonly ApplicationDbContext _appDb;
        private readonly UserManager<IdentityUser> _userManager;

        public CaseController(Dat154Gr2Context db, ApplicationDbContext appDb, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _appDb = appDb;
            _userManager = userManager;
        }

        public IActionResult TeacherCases()
        {
            var patients = _db.Patients.ToList();
            return View("CasesTeachers", patients);
        }

        public async Task<IActionResult> StudentCases()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            var user = _appDb.CustomUsers.FirstOrDefault(u => u.Email == identityUser.Email);
            var patient = _db.Patients.FirstOrDefault(p => p.StudentId == user.Id);
            return View("CasesStudents", patient);
        }

        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            var patient = _db.Patients.FirstOrDefault(p => p.Id == id);
            if (patient != null)
            {
                patient.IsActive = !patient.IsActive;
                _db.SaveChanges();
            }
            return RedirectToAction("TeacherCases");
        }

        [HttpGet("api/case/{studentId}")]
        public IActionResult GetCaseForStudent(int studentId)
        {
            var patient = _db.Patients
                .Include(p => p.Medications)
                .Include(p => p.Allergies)
                .Include(p => p.Diagnoses)
                .Include(p => p.Goals)
                .FirstOrDefault(p => p.StudentId == studentId && p.IsActive);

            if (patient == null)
                return NotFound("No active case for this student");

            return Ok(patient);
        }
    }
}