using Assignment_4.Data;
using Assignment_4.Mapping;
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
            var vm = new TeacherCasesViewModel
            {
                Patients = _db.Patients.OrderBy(p => p.Id).ToList(),
                Students = _db.CustomUsers.Where(u => u.Role == "Student").OrderBy(u => u.Id).ToList()
            };
            return View("CasesTeachers", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignStudent(int patientId, string? studentCustomUserId)
        {
            var patient = _db.Patients.FirstOrDefault(p => p.Id == patientId);
            if (patient == null)
                return RedirectToAction(nameof(TeacherCases));

            if (string.IsNullOrWhiteSpace(studentCustomUserId))
                patient.StudentId = null;
            else if (int.TryParse(studentCustomUserId, out var sid))
                patient.StudentId = sid;

            _db.SaveChanges();
            return RedirectToAction(nameof(TeacherCases));
        }

        public async Task<IActionResult> StudentCases()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser?.Email is not { } email)
                return Challenge();
            var user = _db.CustomUsers.FirstOrDefault(u => u.Email == email);
            var patient = user == null ? null : _db.Patients.FirstOrDefault(p => p.StudentId == user.Id);
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

            return Ok(CaseMapper.ToDto(patient));
        }
    }
}